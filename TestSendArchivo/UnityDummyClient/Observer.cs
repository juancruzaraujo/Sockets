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

        public void EventTrigger(ISubject subject)
        {

            if ((subject as UnityClient).UnityClientEvent.GetEventType == EventParameters.EventType.CLIENT_CONNECTION_OK)
            {
                Console.WriteLine("conectado ok....");
                (subject as UnityClient).UnityClientEvent.GetUnityClientInstance.Send("Hello!");

            }

            if ((subject as UnityClient).UnityClientEvent.GetEventType == EventParameters.EventType.DATA_IN)
            {
                Console.WriteLine((subject as UnityClient).UnityClientEvent.GetData);
            }
        }
    }
}
