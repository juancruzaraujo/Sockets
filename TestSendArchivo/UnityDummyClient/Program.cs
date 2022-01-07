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
            UnityClient unityClient = new UnityClient();


            Observer observer = new Observer();
            unityClient.Attach(observer);

            unityClient.SetProtocol = Protocol.ConnectionProtocol.TCP;
            unityClient.Connect(0, "127.0.0.1", 1987);



            Console.ReadKey();
        }
    }



}
