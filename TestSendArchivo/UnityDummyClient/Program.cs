using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityClientSocket;

namespace UnityDummyClient
{
    class Program
    {
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
    }



}
