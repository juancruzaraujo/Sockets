using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace udpClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new UdpClient();
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1492); // endpoint where server is listening
            client.Connect(ep);

            for (int i = 0; i < 10; i++)
            {
                Byte[] sendBytes = Encoding.ASCII.GetBytes("hola mundo");
                client.Send(sendBytes, sendBytes.Length);
                Console.WriteLine("envie hola mundo");
                Thread.Sleep(10000);
                // then receive data
                var receivedData = client.Receive(ref ep);

                Console.WriteLine("receive data from " + ep.ToString());
                Console.WriteLine(Encoding.ASCII.GetString(receivedData, 0, receivedData.Length));
            }
            Console.Read();
        }
    }
}
