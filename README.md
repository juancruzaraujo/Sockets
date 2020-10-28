# DLL Sockets

to create a server or a client, first of all you have to add sockets.dll in the project references.


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
