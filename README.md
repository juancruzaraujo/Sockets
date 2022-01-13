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
            conparam.SetPort(1789).SetHost("127.0.0.1").SetConnectionTag("connection_" + i);
            
            //SetConnectionTag setea un tag que se guarda para identificar al cliente y este es devuelto en todos los eventos
            //de dicho cliente.
            //sirve para poder identificar el cliente en el caso de tener varias conexiones a un mismo host y puerto 
            
            conparam.SetProtocol(Protocol.ConnectionProtocol.TCP); 
            clientTCP.ConnectClient(conparam);
            
            
            //UDP
            conparam.SetProtocol(Protocol.ConnectionProtocol.UDP);
            clientUDP.ConnectClient(conparam);

            //se tiene que hacer que se le pueda pasar el tag de conexíón
            //y que no haga falta crear la conexión creando un método que sea 
            //SendUDP(conectionParameter,mensaje)
            clientUDP.Send(i+1, "hola mundo"); 
            

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

class Observer : IObserver
    {
        public void Update(ISubject subject)
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

public class ConsoleCommands : MonoBehaviour
{

    string _msgToSend;
    UnityClient unityClient;
    Protocol connectionProtocol;

    

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        /*Socket*/
        
        CreateCommands();
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        Debug.Log("saliendo =(");
    }

    private void CreateCommands()
    {
        Commands.commandInstance.AddCommand("/testlevel", "move to test level", ChangeLevel);
        Commands.commandInstance.AddCommand("/send", "envía un mensaje al servidor", SendMessageToServer);

    }

    private void Write(string text)
    {
        ManagerConsola.instance.WriteLine(text);
    }

    private void ChangeLevel(string command, string parameters)
    {
        Write("Chage to " + parameters);
        SceneManager.LoadScene("battleRoyale_testLevel", LoadSceneMode.Additive);
        ManagerConsola.instance.OpenCloseConsola(false);
    }

    private void SendMessageToServer(string command, string parameters)
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

public class SocketObserver : IObserver
{

    string _msg;
    public string SetMgs
    {
        set
        {
            _msg = value;
        }
    }

    public void Update(ISubject subject)
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

