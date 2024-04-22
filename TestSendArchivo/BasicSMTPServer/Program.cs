using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sockets;

namespace BasicSMTPServer
{
    internal class Program
    {

        static Socket _sockets;
        static bool _keep = true;
        static void Main(string[] args)
        {
            

            _sockets = new Socket();
            _sockets.SetServer(1810, Protocol.ConnectionProtocol.TCP, 100, 120);
            _sockets.Event_Socket += _sockets_Event_Socket;
            _sockets.StartServer();
            Console.WriteLine("basic smtp server start....");
            while (_keep)
            {
                Console.WriteLine("type \"stop\" to stop server");
                Console.WriteLine("type \"kill\" to kill all connections");
                string command = Console.ReadLine();
                if (command == "stop")
                {
                    _keep = false;
                    _sockets.KillServer();
                }
                else if (command == "kill")
                {
                    _sockets.DisconnectAllConnectedClientsToMe();
                }
            }

        }

        private static void _sockets_Event_Socket(EventParameters eventParameters)
        {
            Socket aux = eventParameters.GetSocketInstance;
            switch (eventParameters.GetEventType)
            {
                case EventParameters.EventType.SERVER_NEW_CONNECTION:
                    Console.WriteLine("nueva conexión");
                    break;

                case EventParameters.EventType.DATA_IN:
                    string response = "";
                    bool result = DataIN(eventParameters.GetData,ref response);
                    if (result)
                    {
                        aux.Send(eventParameters.GetConnectionNumber, response);
                    }
                    else
                    {
                        aux.Disconnect(eventParameters.GetConnectionNumber);
                    }
                    break;

            }
        }

        private static bool DataIN(string data,ref string response)
        {
            if (data.Length > 0)
            {
                if (data.StartsWith("QUIT"))
                {
                    return false;
                    //client.Close();
                    //break;//exit while
                }
                //message has successfully been received
                if (data.StartsWith("EHLO"))
                {
                    response = "250 OK";
                }

                if (data.StartsWith("RCPT TO"))
                {
                    response = "250 OK";
                }

                if (data.StartsWith("MAIL FROM"))
                {

                    response = "250 OK";
                }

                if (data.StartsWith("DATA"))
                {
                    response = "354 Start mail input; end with";
                    //strMessage = Read(); //viene el email?
                    response = "250 OK";
                }
            }
            Console.WriteLine("data------->" + data);
            Console.WriteLine("response--->" + response);
            return true; 
        }
    }
}
