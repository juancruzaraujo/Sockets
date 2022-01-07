using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityClientSocket
{
    public class ConnectionParameters
    {
        public const int C_DEFALT_CODEPAGE = 65001;

        private int _port;
        private string _host;
        private Protocol.ConnectionProtocol _connectionProtocol;
        private int _timeOut;
        private int _codePage;
        private int _receiveTimeout;
        private string _connectionTag;
        
        public ConnectionParameters()
        {
            _timeOut = 0;
            _codePage = C_DEFALT_CODEPAGE;
            _receiveTimeout = 0;
        }

        public ConnectionParameters SetPort(int port)
        {
            _port = port;
            return this;
        }

        public ConnectionParameters SetHost(string host)
        {
            _host = host;
            return this;
        }

        public ConnectionParameters SetProtocol(Protocol.ConnectionProtocol connectionProtocol)
        {
            _connectionProtocol = connectionProtocol;
            return this;
        }

        public ConnectionParameters SetTimeOut(int timeOut)
        {
            _timeOut = timeOut;
            return this;
        }

        public ConnectionParameters SetCodePage(int codePage)
        {
            _codePage = codePage;
            return this;
        }

        public ConnectionParameters SetRecieveTimeOut(int recieveTimeOut)
        {
            _receiveTimeout = recieveTimeOut;
            return this;
        }

        public ConnectionParameters SetConnectionTag(string connectionTag)
        {
            _connectionTag = connectionTag;
            return this;
        }

        public int GetPort
        {
            get
            {
                return _port;
            }
        }

        public string GetHost
        {
            get
            {
                return _host;
            }
        }

        public Protocol.ConnectionProtocol GetConnectionProtocol
        {
            get
            {
                return _connectionProtocol;
            }
        }

        public int GetTimeOut
        {
            get
            {
                return _timeOut;
            }
        }

        public int GetCodePage
        {
            get
            {
                return _codePage;
            }
        }

        public int GetRecieveTimeOut
        {
            get
            {
                return _receiveTimeout;
            }
        }

        public string GetConnectionTag
        {
            get
            {
                return _connectionTag;
            }
        }
    }
}
