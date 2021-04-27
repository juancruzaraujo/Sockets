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
        _obSocket = new Sockets.Sockets();
        _obSocket.Event_Socket += new Sockets.Sockets.Delegate_Socket_Event(EvSockets); 
    }

    static void Cliente()
        {
            string message = "";

            Console.WriteLine("modo cliente");
            Console.Title = "MODO CLIENTE";

            _obSocket.ClientMode = true;
 
            _obSocket.ConnectClient(1492, "127.0.0.1", 5);
            int connectionNumber = _obSocket.GetConnectionNumber();

            if (message != "")
            {
                Console.WriteLine(message);
            }

        }

        static void ClienteUDP()
        {
            //string Message = "";

            Console.WriteLine("modo cliente UDP");
            Console.Title = "MODO CLIENTE UDP";

            _obSocket.ClientMode = true;
            _obSocket.ConnectClient(1492, "127.0.0.1", 5,false);
            int connectionNumber = _obSocket.GetConnectionNumber();

        }
        
        void send(int connectionNumber, string message)
        {
            _obSocket.Send(connectionNumber, message); //send a message using the connection number
            
        }
        
        static void EvSockets(EventParameters ev)
        {
            
            switch (ev.GetEventType)
            {
                case EventParameters.EventType.NEW_CONNECTION:
                    Console.WriteLine (corchete(ev.GetConnectionNumber.ToString()) +  " conectado desde " + ev.GetIpOrigen);
                    _obSocket.Send("<SOS> " + ev.GetConnectionNumber.ToString(), ev.GetListIndex);
                    //_obSocket.Send("<SOS> " + ev.GetConnectionNumber.ToString(), 0);
                    break;

                case EventParameters.EventType.DATA_IN:
                    //DatosIn(indice, datos, true, ipOrigen);
                    Console.writeLine(ev.GetData);
                    break;
             }
          }
    
```

<<<<<<< HEAD

=======
>>>>>>> master
