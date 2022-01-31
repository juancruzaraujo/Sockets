using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityClientSocket
{
    public interface ISubject
    {
         void Attach(IUnityClientSocketEventObserver observer);

        // Detach an observer from the subject.
        void Detach(IUnityClientSocketEventObserver observer);

        // Notify all observers about an event.
        void Notify();
    }
}
