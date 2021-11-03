using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sockets
{
    public class EventParameters
    {
        private int _conectionNumber;
        private int _connectionNumberTCP;
        private int _connectionNumberUDP;
        private int _listIndex;
        private EventType _eventType;
        private bool _listen;
        private long _size;
        private string _data;
        private long _position;
        private string _clientIp;
        private string _serverIp;
        private int _errorCode;
        private int _lineNumberError;
        //private bool _tcp;
        private Protocol.ConnectionProtocol _connectionProtocol;

        private Socket _socketInstance;

        /// <summary>
        /// Numero de evento que se dispara
        /// </summary>
        public enum EventType
        {
            ERROR,
            SEND_COMPLETE,
            DATA_IN,
            /// <summary>
            /// la comunucación con el cliente ya esta establecida
            /// </summary>
            SERVER_NEW_CONNECTION,
            END_CONNECTION,
            /// <summary>
            /// acepta conexión y se obtiene el ip del cliente, pero todavía no establece la comunucación
            /// </summary>
            SERVER_ACCEPT_CONNECTION,
            SERVER_WAIT_CONNECTION,
            SEND_POSITION,
            CLIENT_CONNECTION_OK,
            CLIENT_TIME_OUT,
            SERVER_START,
            CONNECTION_LIMIT,
            SERVER_STOP,
            SEND_ARRAY_COMPLETE,
            RECIEVE_TIMEOUT
        }

        internal EventParameters SetConnectionNumber(int conectionNumber)
        {
            _conectionNumber = conectionNumber;
            return this;
        }

        internal EventParameters setConnectionNumberTCP(int connectionNumberTCP)
        {
            _connectionNumberTCP = connectionNumberTCP;
            return this;
        }

        internal EventParameters setConnectionNumberUDP(int connectionNumberUDP)
        {
            _connectionNumberUDP = connectionNumberUDP;
            return this;
        }

        internal EventParameters SetListIndex(int listIndex)
        {
            _listIndex = listIndex;
            return this;
        }

        internal EventParameters SetEvent(EventType eventType)
        {
            _eventType = eventType;
            return this;
        }

        internal EventParameters SetListening(bool listen)
        {
            _listen = listen;
            return this;
        }

        internal EventParameters SetSize(long size)
        {
            _size = size;
            return this;
        }

        internal EventParameters SetData(string data)
        {
            _data = data;
            return this;
        }

        internal EventParameters SetPosition(long position)
        {
            _position = position;
            return this;
        }

        internal EventParameters SetClientIp(string clientIp)
        {
            _clientIp = clientIp;
            return this;
        }

        internal EventParameters SetServerIp(string serverIp)
        {
            _serverIp = serverIp;
            return this;
        }

        internal EventParameters SetErrorCode(int errorCode)
        {
            _errorCode = errorCode;
            return this;
        }

        internal EventParameters SetLineNumberError(int lineNumber)
        {
            _lineNumberError = lineNumber;
            return this;
        }

        internal EventParameters SetTCP(Protocol.ConnectionProtocol connectionProtocol)
        {
            _connectionProtocol = connectionProtocol;
            return this;
        }


        internal EventParameters SetSocketInstance(Socket socket)
        {
            _socketInstance = socket;
            return this;
        }

        /// <summary>
        /// retorna el numero de conexión actual del cliente conectado
        /// </summary>
        public int GetConnectionNumber
        {
            get
            {
                return _conectionNumber;
            }
        }

        public int GetListIndex
        {
            get
            {
                return _listIndex;
            }
        }

        public EventType GetEventType
        {
            get
            {
                return _eventType;
            }
        }

        public bool GetListening
        {
            get
            {
                return _listen;
            }
        }

        public long GetSize
        {
            get
            {
                return _size;
            }
        }

        public string GetData
        {
            get
            {
                return _data;
            }
        }

        public long GetPosition
        {
            get
            {
                return _position;
            }
        }

        public string GetClientIp
        {
            get
            {
                return _clientIp;
            }
        }

        public string GetServerIp
        {
            get
            {
                return _serverIp;
            }
        }

        public int GetErrorCode
        {
            get
            {
                return _errorCode;
            }
        }

        public int GetLineNumberError
        {
            get
            {
                return _lineNumberError;
            }
        }

        /*
        public int GetConnectionNumberTCP
        {
            get
            {
                return _connectionNumberTCP;
            }
        }

        public int GetConnectionNumberUDP
        {
            get
            {
                return _connectionNumberUDP;
            }
        }
        */
        public Protocol.ConnectionProtocol GetProtocol
        {
            get
            {
                return _connectionProtocol;
            }
        }

        public Socket GetSocketInstance
        {
            get
            {
                return _socketInstance;
            }
        }
    }
}
