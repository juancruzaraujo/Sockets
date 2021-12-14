using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Sockets;


namespace DummyClients
{
    class Program
    {
        static Socket clientTCP;
        static Socket clientUDP;
        
        static int port;
        static string host;
        static int maxCon;
        static string msgType;

        static void Main(string[] args)
        {

            //ejecutar con parámetros
            //ip, puerto, cantidad conexiones
            //DummyClient 200.1.1.10 1987 10

            //la cantidad de argumentos es mala
            if (args.Count() != 3)
            {
                
                Console.WriteLine("ejecutar con los paramteros: IP, numero de puerto, cantidad maxima de conexiones.");
                Console.WriteLine(" DummyClients 127.0.0.1 8080 10");
                
                Console.ReadKey();
            }

            host = args[0];
            port = int.Parse(args[1]);
            maxCon = int.Parse(args[2]);

            clientTCP = new Socket();
            clientTCP.Event_Socket += ClientTCP_Event_Socket;

            clientUDP = new Socket();
            clientUDP.Event_Socket += ClientUDP_Event_Socket;

            Console.WriteLine("start client");

            for (int i =0;i<maxCon;i++)
            {
                Console.WriteLine(" Connectinig " + i);

                ConnectionParameters conparam = new ConnectionParameters();
                conparam.SetPort(port).SetHost(host).SetConnectionTag("connection_" + i);

                clientTCP.ConnectClient(conparam);
                Thread.Sleep(500);
            }

            Console.WriteLine(" press enter to exit");
            Console.ReadKey();
            System.Environment.Exit(0);
        }

       

        private static void ClientUDP_Event_Socket(EventParameters eventParameters)
        {
            
        }

        private static void ClientTCP_Event_Socket(EventParameters eventParameters)
        {
            switch(eventParameters.GetEventType)
            {
                case EventParameters.EventType.CLIENT_CONNECTION_OK:
                    Console.WriteLine("connection to " + host + " connection number " + eventParameters.GetConnectionNumber + " " + eventParameters.GetClientTag);
                    clientTCP.Send(eventParameters.GetConnectionNumber, "hola mundo");
                    break;

                case EventParameters.EventType.DATA_IN:
                    Console.WriteLine(" " + eventParameters.GetData);
                    //clientTCP.Disconnect(eventParameters.GetConnectionNumber);
                    
                    //clientTCP.ConnectClient(port, host);
                    break;

                case EventParameters.EventType.ERROR:
                    Console.WriteLine(eventParameters.GetData);
                    break;
            }
        }
    }
}
