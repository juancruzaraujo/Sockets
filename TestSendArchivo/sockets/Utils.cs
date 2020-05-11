using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Sockets
{
    internal class Utils
    {
      
        internal int GetCodigoError(Exception err)
        {

            var w32ex = err as Win32Exception;
            int cod = -1;
            if (w32ex == null)
            {
                w32ex = err.InnerException as Win32Exception;
            }
            if (w32ex != null)
            {
                cod = w32ex.ErrorCode;
            }
            if (cod ==-1)
            {
                cod = err.HResult;
            }
           
            return cod;
        }
    }
}
