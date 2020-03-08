using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace udpClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new UdpClient();
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1492); // endpoint where server is listening
            client.Connect(ep);


            Byte[] sendBytes = Encoding.ASCII.GetBytes("hola mundo");

            
            client.Send(sendBytes, sendBytes.Length);

            

            // then receive data
            var receivedData = client.Receive(ref ep);

            Console.WriteLine("receive data from " + ep.ToString());
            Console.WriteLine(Encoding.ASCII.GetString(receivedData, 0, receivedData.Length));

            Console.Read();
        }
    }
}
