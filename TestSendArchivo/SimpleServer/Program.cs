using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sockets;

namespace SimpleServer
{
    class Program
    {
        //static Socket _serverTCP;
        //static Socket _serverUDP;
        static int port;
        static int maxCon;
        static bool keepRuning;

        static void Main(string[] args)
        {
            if (args.Count() != 2)
            {
                Console.WriteLine(" \r\nparams[port number] [max connections]");
                Console.ReadLine();
                System.Environment.Exit(0);
            }
            keepRuning = true;

            CreateServer(args);
            

            Console.WriteLine("salí del Test");
            while(keepRuning)
            {
                Console.WriteLine("exit to terminate program");
                Console.WriteLine("start to start new server");
                string command = Console.ReadLine();
                if (command == "exit")
                {
                    keepRuning = false;
                }

                if (command == "start")
                {
                    CreateServer(args);
                }
            }
        }

        private static void CreateServer(string[] args)
        {
            Socket _serverTCP;
            Socket _serverUDP;

            bool keep = true;

            port = int.Parse(args[0]);
            maxCon = int.Parse(args[1]);

            maxCon = 1000;

            Console.WriteLine(@"port " + port);
            Console.WriteLine(@"max connections " + maxCon);

            _serverTCP = new Socket();
            //_serverTCP.SetServer(port, Protocol.ConnectionProtocol.TCP, maxCon,60);
            _serverTCP.SetServer(port, Protocol.ConnectionProtocol.TCP, maxCon,10);
            _serverTCP.Event_Socket += ServerTCP_Event_Socket;

            _serverUDP = new Socket();
            //_serverUDP.SetServer(port, Protocol.ConnectionProtocol.UDP, 0);
            _serverUDP.Event_Socket += ServerUDP_Event_Socket;

            _serverTCP.StartServer();
            //_serverUDP.StartServer();

            while (keep)
            {
                Console.WriteLine("type \"stop\" to stop server");
                Console.WriteLine("type \"kill\" to kill all connections");
                string command = Console.ReadLine();
                if (command == "stop")
                {
                    keep = false;
                    _serverTCP.KillServer();
                }
                else if (command == "kill")
                {
                    _serverTCP.DisconnectAllConnectedClientsToMe();
                }
            }
        }

        private static void ServerTCP_Event_Socket(EventParameters eventParameters)
        {

            Socket aux = eventParameters.GetSocketInstance;
            switch (eventParameters.GetEventType)
            {
                case EventParameters.EventType.SERVER_NEW_CONNECTION:

                    Console.WriteLine("connection number " + eventParameters.GetConnectionNumber + " " + eventParameters.GetClientIp);
                    aux.Send(eventParameters.GetConnectionNumber, "Welcome! "+ eventParameters.GetConnectionNumber  + "\r\n ");
                    break;

                case EventParameters.EventType.ERROR:
                    Console.WriteLine(eventParameters.GetData);
                    break;

                case EventParameters.EventType.DATA_IN:
                    Console.WriteLine(eventParameters.GetConnectionNumber + " " + eventParameters.GetData);
                    //aux.DisconnectConnectedClientToMe(eventParameters.GetConnectionNumber);
                    aux.Send(eventParameters.GetConnectionNumber, "ok" + eventParameters.GetConnectionNumber + "\r\n ");
                    break;

                case EventParameters.EventType.END_CONNECTION:
                    Console.WriteLine("client disconected-->" + eventParameters.GetConnectionNumber);
                    break;

                case EventParameters.EventType.RECIEVE_TIMEOUT:
                    Console.WriteLine("recieve timeout-->" + eventParameters.GetConnectionNumber);
                    break;

                case EventParameters.EventType.CONNECTION_LIMIT:
                    Console.WriteLine("connection limit");
                    break;

                case EventParameters.EventType.SERVER_STOP:
                    Console.WriteLine("SERVER STOPED");
                    break;
            }
        }

        private static void ServerUDP_Event_Socket(EventParameters eventParameters)
        {

        }


    }
}
