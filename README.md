# DLL Sockets

to create a server or a client, first of all you have to add sockets.dll in the project references.
messages from the client, both in server and client mode, arrive by events

para crear un servidor o cliente, primero hay que agregar en referencias la dll de sockets
los mensajes que llegan generan un evento

see EventParameters.EventType

# simple server example


```csharp

class Program
    {

        static Sockets.Sockets obSocket;

        static void Main(string[] args)
        {
            SetSocket();

            while(true)
            {

            }
        }

        static void SetSocket()
        {
            obSocket = new Sockets.Sockets();
            obSocket.Event_Socket += new Sockets.Sockets.Delegate_Socket_Event(EvSockets);

            obSocket.ServerMode = true;
            obSocket.SetServer(1492, Sockets.Sockets.C_DEFALT_CODEPAGE, true, 10);
            obSocket.StartServer();
        }

        static void EvSockets(EventParameters eventParameters)
        {
            switch (eventParameters.GetEventType)
            {
                case EventParameters.EventType.NEW_CONNECTION:
                    obSocket.Send("HELLO THERE MY FRIEND!\n\r", eventParameters.GetListIndex);
                    obSocket.DisconnectConnectedClientToMe(eventParameters.GetConnectionNumber); 
                    break;

                case EventParameters.EventType.ERROR:
                    Console.WriteLine(eventParameters.GetData);
                    System.Environment.Exit(eventParameters.GetErrorCode);
                    break;
            }
        }

    }
```
    
# simple client example

```csharp

    void main()
    {
        clientTCP = new Socket();
        clientTCP.Event_Socket += ClientTCP_Event_Socket;

        clientUDP = new Socket();
        clientUDP.Event_Socket += ClientUDP_Event_Socket;
        
        Connect();
    }

    static void Connect()
        {
            ConnectionParameters conparam = new ConnectionParameters();
            conparam.SetPort(1789).SetHost("127.0.0.1").SetConnectionTag("clientTag");
            
            //SetConnectionTag setea un tag que se guarda para identificar al cliente y este es devuelto en todos los eventos
            //de dicho cliente.
            //sirve para poder identificar el cliente en el caso de tener varias conexiones a un mismo host y puerto 
            
            conparam.SetProtocol(Protocol.ConnectionProtocol.TCP); 
            clientTCP.ConnectClient(conparam);
            
            
            //UDP
            conparam.SetProtocol(Protocol.ConnectionProtocol.UDP);
            clientUDP.ConnectClient(conparam);

            clientUDP.Send("clientTag"", "hola mundo"); 
            

        }

        private static void ClientUDP_Event_Socket(EventParameters eventParameters)
        {
            switch (eventParameters.GetEventType)
            {
                case EventParameters.EventType.CLIENT_CONNECTION_OK:
                    Console.WriteLine("connection to " + host + " connection number " + eventParameters.GetConnectionNumber + " " + eventParameters.GetClientTag);
                    clientUDP.Send(eventParameters.GetConnectionNumber, "hola mundo");
                    //o se puede usar en caso de que clientUDP no sea global a la clase
                    //eventParameters.GetSocketInstance.Send(eventParameters.GetConnectionNumber, "hola mundo");
                    break;

                case EventParameters.EventType.DATA_IN:
                    Console.WriteLine("clientTag " + eventParameters.GetClientTag + " recieve " + eventParameters.GetData);
                    break;

                case EventParameters.EventType.ERROR:
                    Console.WriteLine(eventParameters.GetData);
                    break;
            }


        }

        private static void ClientTCP_Event_Socket(EventParameters eventParameters)
        {
            switch(eventParameters.GetEventType)
            {
                case EventParameters.EventType.CLIENT_CONNECTION_OK:
                    Console.WriteLine("connection to " + host + " connection number " + eventParameters.GetConnectionNumber + " " + eventParameters.GetClientTag);
                    clientUDP.Send(eventParameters.GetConnectionNumber, "hola mundo");
                    //o se puede usar en caso de que clientTCP no sea global a la clase
                    //eventParameters.GetSocketInstance.Send(eventParameters.GetConnectionNumber, "hola mundo");
                    break;

                case EventParameters.EventType.DATA_IN:
                    Console.WriteLine("clientTag " + eventParameters.GetClientTag + " recieve " + eventParameters.GetData);
                    //clientTCP.Disconnect(eventParameters.GetConnectionNumber);
                    break;

                case EventParameters.EventType.ERROR:
                    Console.WriteLine(eventParameters.GetData);
                    break;
            }
        }
    
```

# UnityClientSocket

```csharp
using UnityClientSocket;

    static void Main(string[] args)
    {
        bool keepRunning = true;

        UnityClient unityClient = new UnityClient();


        Observer observer = new Observer();
        unityClient.Attach(observer);

        //unityClient.SetProtocol = Protocol.ConnectionProtocol.TCP;
        //unityClient.CodePage(65001);
        //unityClient.Connect(0, "127.0.0.1", 1987);

        ConnectionParameters connectionParameters = new ConnectionParameters();
        connectionParameters.SetHost("127.0.0.1").SetPort(1987);
        //connectionParameters.SetProtocol(Protocol.ConnectionProtocol.UDP);

        unityClient.Connect(connectionParameters);

        while(keepRunning)
        {
            Console.WriteLine("stop to exit program");
            Console.WriteLine("anything else is sent to the server as a message");
            string command = Console.ReadLine();

            if (command == "stop")
            {
                keepRunning = false;
                unityClient.CloseConnection();
            }
            else
            {
                unityClient.Send(command);
            }
        }

        Console.WriteLine("press enter to exit =p");
        Console.ReadKey();
    }
    
    //Observer class
    
using UnityClientSocket;

class Observer : IUnityClientSocketEventObserver
    {
        public void EventTrigger(ISubject subject)
        {

            if ((subject as UnityClient).UnityClientEvent.GetEventType == EventParameters.EventType.CLIENT_CONNECTION_OK)
            {
                Console.WriteLine("conectado ok....");
                (subject as UnityClient).UnityClientEvent.GetUnityClientInstance.Send("Hello!");

            }

            if ((subject as UnityClient).UnityClientEvent.GetEventType == EventParameters.EventType.DATA_IN)
            {
                Console.WriteLine((subject as UnityClient).UnityClientEvent.GetData);
            }
        }
    }
    

```

# UnityClientSocket ejemplo de uso en unity

```csharp
//agregar la dll UnityClientSocket.dll en el directorio de Assets

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InGameConsole;
using UnityEngine.SceneManagement;
using UnityClientSocket;

public class TuClaseEnUnity : MonoBehaviour
{

    string _msgToSend;
    UnityClient unityClient;
    Protocol connectionProtocol;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        Debug.Log("saliendo =(");
    }

    private void SendMessageToServer(string parameters)
    {
        if (unityClient == null)
        {
            SocketObserver socketObserver = new SocketObserver();
            socketObserver.SetMgs = parameters;
            unityClient = new UnityClient();
            unityClient.Attach(socketObserver);

            ConnectionParameters connectionParameters = new ConnectionParameters();
            connectionParameters.SetHost("127.0.0.1");
            connectionParameters.SetPort(1987);
            connectionParameters.SetProtocol(Protocol.ConnectionProtocol.TCP);
            connectionParameters.SetCodePage(ConnectionParameters.C_DEFALT_CODEPAGE);

            unityClient.Connect(connectionParameters);

        }
        else
        {
            unityClient.Send(parameters);
        }
        
    }

    
}

//clase observer

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityClientSocket;
using InGameConsole;

public class SocketObserver : IUnityClientSocketEventObserver
{

    string _msg;
    public string SetMgs
    {
        set
        {
            _msg = value;
        }
    }

    public void EventTrigger(ISubject subject)
    {

        //ManagerConsola.instance.WriteLine("EVENTO-->" + (subject as UnityClient).UnityClientEvent.GetEventType.ToString());

        Debug.Log("EVENTO-->" + (subject as UnityClient).UnityClientEvent.GetEventType.ToString());

        if ((subject as UnityClient).UnityClientEvent.GetEventType == EventParameters.EventType.CLIENT_CONNECTION_OK)
        {
            
            Debug.Log("CONECTADO OK");
            (subject as UnityClient).UnityClientEvent.GetUnityClientInstance.Send(_msg+"");
        }

        if ((subject as UnityClient).UnityClientEvent.GetEventType == EventParameters.EventType.DATA_IN)
        {
            
            Debug.Log((subject as UnityClient).UnityClientEvent.GetData);
        }

        if ((subject as UnityClient).UnityClientEvent.GetEventType == EventParameters.EventType.ERROR)
        {
            
            Debug.Log((subject as UnityClient).UnityClientEvent.GetData);
        }

    }
}


```

# otro ejemplo de uso de UnityClientSocket

```csharp

using UnityClientSocket;

public class ConsoleCommands : MonoBehaviour, IUnityClientSocketEventObserver
{
    string _msgToSend;
    
    UnityClient unityClientTCP;
    UnityClient unityClientUDP;
    Protocol connectionProtocol;
    
    string msgFromServer;
    string msgFromServerToShow;


    private void SendTCPMessageToServer(string parameters)
    {
        if (unityClientTCP == null)
        {

            _msgToSend = parameters;

            SocketObserver socketObserver = new SocketObserver();
            socketObserver.SetMgs = parameters;
            socketObserver.SetConsoleCommands = this;

            unityClientTCP = new UnityClient();
            unityClientTCP.Attach(this);

            UnityClientSocket.ConnectionParameters connectionParameters = new UnityClientSocket.ConnectionParameters();
            connectionParameters.SetHost("127.0.0.1");
            connectionParameters.SetPort(1987);
            connectionParameters.SetProtocol(UnityClientSocket.Protocol.ConnectionProtocol.TCP);
            connectionParameters.SetCodePage(UnityClientSocket.ConnectionParameters.C_DEFALT_CODEPAGE);

            unityClientTCP.Connect(connectionParameters);

        }
        else
        {
            if (unityClientTCP.conected)
            {
                unityClientTCP.Send(parameters);
            }
        }
        
    }

    private void SendUDPMessageToServer(string command, string parameters)
    {
        if (unityClientUDP == null)
        {

            _msgToSend = parameters;

            SocketObserver socketObserver = new SocketObserver();
            socketObserver.SetMgs = parameters;
            socketObserver.SetConsoleCommands = this;

            unityClientUDP = new UnityClient();
            unityClientUDP.Attach(this);

            UnityClientSocket.ConnectionParameters connectionParameters = new UnityClientSocket.ConnectionParameters();
            connectionParameters.SetHost("127.0.0.1");
            connectionParameters.SetPort(1987);
            connectionParameters.SetProtocol(UnityClientSocket.Protocol.ConnectionProtocol.UDP);
            connectionParameters.SetCodePage(UnityClientSocket.ConnectionParameters.C_DEFALT_CODEPAGE);

            unityClientUDP.Connect(connectionParameters);
            unityClientUDP.Send(parameters);

        }

        unityClientUDP.Send(parameters);
    }


    public void EventTrigger(ISubject subject)
    {
        
        Debug.Log("EVENTO-->" + (subject as UnityClient).UnityClientEvent.GetEventType.ToString());

        if ((subject as UnityClient).UnityClientEvent.GetEventType == EventParameters.EventType.CLIENT_CONNECTION_OK)
        {
            Debug.Log("CONECTADO OK");
            (subject as UnityClient).UnityClientEvent.GetUnityClientInstance.Send(_msgToSend + "");
        }

        if ((subject as UnityClient).UnityClientEvent.GetEventType == EventParameters.EventType.DATA_IN)
        {
            msgFromServer = (subject as UnityClient).UnityClientEvent.GetData;
            Debug.Log((subject as UnityClient).UnityClientEvent.GetData);
        }

        if ((subject as UnityClient).UnityClientEvent.GetEventType == EventParameters.EventType.ERROR)
        {
            Debug.Log((subject as UnityClient).UnityClientEvent.GetData);
        }

    }

    void Update()
    {
        if (msgFromServer != "" && msgFromServer != msgFromServerToShow)
        {
            msgFromServerToShow = msgFromServer;
            //hago lo que quiero con el string msgFromServerToShow
   
        }
    }

    private void OnDestroy()
    {
        Debug.Log("saliendo =(");
        if (unityClientTCP != null && unityClientTCP.conected)
        {
            unityClientTCP.CloseConnection();
        }

    }
}

```
# Ejemplo de cliente usando lib Socket en unity
copiar la lib socket al directorio de assets

```csharp

using Sockets;

public class TuClaseDeUnity : MonoBehaviour
{
    private void Send(string command, string parameters)
    {
        _msgToSend = parameters;

        if (socket == null)
        {
            socket = new Socket();
            Sockets.ConnectionParameters connectionParameters = new Sockets.ConnectionParameters();
            connectionParameters.SetHost("127.0.0.1").SetPort(1987).SetConnectionTag("socketlib");

            socket.Event_Socket += Socket_Event_Socket;
            socket.ConnectClient(connectionParameters);
        }
        else
        {
            socket.Send("socketlib", _msgToSend);
        }

    }
    
    private void Socket_Event_Socket(Sockets.EventParameters eventParameters)
    {
        switch(eventParameters.GetEventType)
        {
            case Sockets.EventParameters.EventType.CLIENT_CONNECTION_OK:
                Debug.Log("connection ok");
                eventParameters.GetSocketInstance.Send(eventParameters.GetConnectionNumber, _msgToSend);
                break;

            case Sockets.EventParameters.EventType.DATA_IN:
                Debug.Log(eventParameters.GetData);
                msgFromServer = eventParameters.GetData;
                break;

        }
    }
    
    private void OnDestroy()
    {
        Debug.Log("saliendo =(");

        if (socket !=null)
        {
            socket.DisconnectAll();
        }


    }
    
    void Update()
    {
        if (msgFromServer != "" && msgFromServer != msgFromServerToShow)
        {
            msgFromServerToShow = msgFromServer;
            ManagerConsola.instance.WriteLine(msgFromServerToShow);
            msgFromServer = "";
            msgFromServerToShow = "";
        }
    }
    
}

```

# Ejemplo de Server usando lib Socket en unity
copiar la lib socket al directorio de assets

```csharp

using Sockets;

public class TuClaseDeUnity : MonoBehaviour
{
    private void StartServer()
    {
        if (socketServer == null)
        {
            socketServer = new Socket();
            socketServer.SetServer(1987, Sockets.Protocol.ConnectionProtocol.TCP,10);
            socketServer.Event_Socket += Socket_Event_Socket;
            socketServer.StartServer();
            ManagerConsola.instance.WriteLine("server iniciado");
        }
        else
        {
            ManagerConsola.instance.WriteLine("server ya iniciado");
        }
    }
    
    private void Socket_Event_Socket(Sockets.EventParameters eventParameters)
    {
        switch(eventParameters.GetEventType)
        {
            case Sockets.EventParameters.EventType.DATA_IN:
                Debug.Log(eventParameters.GetData);
                msgFromServer = eventParameters.GetData;
                break;

            case Sockets.EventParameters.EventType.SERVER_NEW_CONNECTION:
                eventParameters.GetSocketInstance.Send(eventParameters.GetConnectionNumber, "vienbenido todo funsiona josha!");
                break;
        }
    }
    
    private void OnDestroy()
    {
        Debug.Log("saliendo =(");
        
        if (socketServer !=null)
        {
            socketServer.KillServer();
        }
    }
    
    void Update()
    {
        if (msgFromServer != "" && msgFromServer != msgFromServerToShow)
        {
            msgFromServerToShow = msgFromServer;
            ManagerConsola.instance.WriteLine(msgFromServerToShow);
            msgFromServer = "";
            msgFromServerToShow = "";
        }
    }
    
}

```
