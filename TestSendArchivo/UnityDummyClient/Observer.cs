using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityClientSocket;

namespace UnityDummyClient
{
    class Observer : IObserver
    {
        public void Update(ISubject subject)
        {

            if ((subject as UnityClient).State.GetEventType == EventParameters.EventType.CLIENT_CONNECTION_OK)
            {
                Console.WriteLine("conectado ok....");
            }

            if ((subject as UnityClient).State.GetEventType == EventParameters.EventType.DATA_IN)
            {
                Console.WriteLine((subject as UnityClient).State.GetData);
            }
        }
    }
}
