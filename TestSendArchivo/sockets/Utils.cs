using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Sockets
{
    internal class Utils
    {
      
        internal int GetErrorCode(Exception err)
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
        
        /*
        internal int GetNumeroDeLineaError(Exception err)
        {
            var st = new StackTrace(err, true);
            var frame = st.GetFrame(0);
            return frame.GetFileLineNumber();
        }*/
    }
}
