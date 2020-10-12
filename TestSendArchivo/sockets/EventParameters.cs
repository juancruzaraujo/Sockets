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
        private string _ipOrigen;
        private string _ipDestino;
        private int _errorCode;
        private int _lineNumberError;

        /// <summary>
        /// Numero de evento que se dispara
        /// </summary>
        public enum EventType
        {
            ERROR = 0,
            SEND_COMPLETE = 1,
            DATA_IN = 2,
            /// <summary>
            /// la comunucación con el cliente ya esta establecida
            /// </summary>
            NEW_CONNECTION = 3,
            END_CONNECTION = 4,
            /// <summary>
            /// acepta conexión y se obtiene el ip del cliente, pero todavía no establece la comunucación
            /// </summary>
            ACCEPT_CONNECTION = 5,
            WAIT_CONNECTION = 6,
            SEND_POSITION = 7,
            CONNECTION_OK = 8,
            TIME_OUT = 9,
            SERVER_STAR = 10,
            CONNECTION_LIMIT = 11,
            SERVER_STOP = 12,
            SEND_ARRAY_COMPLETE = 13
        };

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

        internal EventParameters SetIpOrigen(string ipOrigen)
        {
            _ipOrigen = ipOrigen;
            return this;
        }

        internal EventParameters SetIpDestino(string ipDestino)
        {
            _ipDestino = ipDestino;
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

        public string GetIpOrigen
        {
            get
            {
                return _ipOrigen;
            }
        }

        public string GetIpDestino
        {
            get
            {
                return _ipDestino;
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

    }
}
