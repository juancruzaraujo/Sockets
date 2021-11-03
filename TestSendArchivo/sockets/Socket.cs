using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sockets
{
    public class Socket
    {
        private List<Client> _lstObjClient;
        private List<ServerTCP> _lstObjServerTCP;
        private ServerUDP _objServerUDP;

        private string _message;

        private bool _serverMode;
        private bool _clientMode;


        private int _serverPortListening;
        private int _codePage;
        private int _connectionNumber;
        private int _clientConnectionNumberTCP;
        private int _clientConnectionNumberUDP;
        private bool _listening;
        private int _clientPort;
        private long _size;
        private string _ipClient;
        private string _data;
        private string _host;
        //private bool _tcp;
        //private ConnectionProtocol _connectionProtocol;
        private Protocol.ConnectionProtocol _connectionProtocol;
        private int _maxServerConnectionNumber;
        private int _numberServerConnections;
        private int _numCliConServer;
        private bool _serverListening;
        private bool _stopingServer;
        private bool _serverStarted;
        private int _serverReceiveTimeout;
        private bool _lastClientConnected;

        public const int C_DEFALT_CODEPAGE = 65001;


        class SendArrayParamsContainer
        {
            internal byte[] memArray;
            internal int clusterSize;
            internal int connectionNumber;
        }

        public delegate void Delegate_Socket_Event(EventParameters eventParameters);
        public event Delegate_Socket_Event Event_Socket;
        public void EventSocket(EventParameters parameters)
        {
            this.Event_Socket(parameters);
        }


        public int GetMaxServerConexiones
        {
            get
            {
                return _maxServerConnectionNumber;
            }
        }

        public Protocol.ConnectionProtocol protocol
        {
            get
            {
                return _connectionProtocol;
            }
        }

        public int MaxNumberServerConnections
        {
            set
            {
                _maxServerConnectionNumber = value;
                if (_serverMode)
                {
                    //if (!_tcp)
                    if (_connectionProtocol == Protocol.ConnectionProtocol.UDP)
                    {
                        _objServerUDP.MaxClientUDP = _maxServerConnectionNumber;
                    }
                }
            }
            get
            {
                return _maxServerConnectionNumber;
            }
        }

        public Socket()
        {

        }

        void Error(string errorMessage)
        {
            EventParameters ev = new EventParameters();

            ev.SetData(errorMessage)
                .SetEvent(EventParameters.EventType.ERROR)
                .SetConnectionNumber(_connectionNumber).SetListening(_listening)
                .SetSize(_size)
                .SetClientIp(_ipClient);

            EventSocket(ev);
        }


        public void ConnectClient(int port, string host, Protocol.ConnectionProtocol connectionProtocol = Protocol.ConnectionProtocol.TCP, int timeOut = 30, int codePage = C_DEFALT_CODEPAGE, int receiveTimeout = 0)
        {

            int connNumber;

            if (_lstObjClient == null)
            {
                _lstObjClient = new List<Client>();
            }

            _clientPort = port;
            _codePage = codePage;
            _host = host;


            Client objClient = new Client(connectionProtocol);

            //objClient = new Client(tcp);
            objClient.SetGetTimeOut = timeOut;
            objClient.ReceiveTimeout = receiveTimeout;
            objClient.CodePage(_codePage);


            objClient.clientEvent += new Client.Delegated_Client_Event(Evsocket);

            _clientMode = true;

            _lstObjClient.Add(objClient);

            if (_connectionProtocol == Protocol.ConnectionProtocol.TCP)
            {
                _clientConnectionNumberTCP++;
                connNumber = _clientConnectionNumberTCP;
            }
            else
            {
                _clientConnectionNumberUDP++;
                connNumber = _clientConnectionNumberUDP;

            }
            _lstObjClient[_lstObjClient.Count() - 1].Connect(connNumber, host, port);
        }



        /// <summary>
        /// set the server parameters
        /// </summary>
        /// <param name="port">listening port</param>
        /// <param name="codePage">codepage communication</param>
        /// <param name="tcp">protocol, tpc = true is default value</param>
        /// <param name="maxCon">maximum number of connections, in udp mode 0 is for unlimited connections</param>
        /// <param name="receiveTimeout">(only tcp mode) maximum waiting time in seconds for an incoming message, if it is 0 the waiting time is unlimited</param>
        public void SetServer(int port, Protocol.ConnectionProtocol connectionProtocol, int maxCon = 0, int receiveTimeout = 0, int codePage = C_DEFALT_CODEPAGE)
        {
            string res = "";
            _listening = false;
            _serverPortListening = port;
            _codePage = codePage;
            //_tcp = tcp;
            _connectionProtocol = connectionProtocol;
            _maxServerConnectionNumber = maxCon;
            _numberServerConnections = 0;
            _numCliConServer = 1;
            _serverReceiveTimeout = receiveTimeout;
            this.ServerMode = true;


            if (!ServerMode) //ya esta activado de antes el modo cliente
            {
                Error(ServerMode.ToString());
                return;
            }
            //if (_tcp)
            if (_connectionProtocol == Protocol.ConnectionProtocol.TCP)
            {
                _lstObjServerTCP = new List<ServerTCP>();
            }

            CreateServer(ref res);

            if (res != "")
            {
                _message = res;
                Error(res);
                return;
            }
            _listening = true;
            _serverMode = true;
        }

        private void CreateServer(ref string message)
        {
            
            
            int indexList = GetLastSpaceFree();
            
            if (_connectionProtocol == Protocol.ConnectionProtocol.TCP)
            {
                ServerTCP objServidor = new ServerTCP(_serverPortListening, _codePage, ref message);
                if (message != "")
                {
                    Error(message);
                    return;
                }
                objServidor.evento_servidor += new ServerTCP.Delegate_Server_Event(Evsocket);
                _lstObjServerTCP.Add(objServidor);

                _lstObjServerTCP[indexList].IndexConnection = _numCliConServer;
                _lstObjServerTCP[indexList].ListIndex = indexList;
                _lstObjServerTCP[indexList].ReceiveTimeout = _serverReceiveTimeout;
            }
            else
            {
                _objServerUDP = new ServerUDP(_serverPortListening);
                _objServerUDP.MaxClientUDP = _maxServerConnectionNumber;
                _objServerUDP.CodePage(_codePage);
                if (message != "")
                {
                    Error(message);
                    return;
                }
                _objServerUDP.evento_servidor += new ServerUDP.Delegate_Server_Event(Evsocket);

            }

        }

        /// <summary>
        /// start the server
        /// </summary>
        public void StartServer()
        {

            
            string res = "";
            
            if (_connectionProtocol == Protocol.ConnectionProtocol.TCP)
            {
                _lstObjServerTCP[_lstObjServerTCP.Count() - 1].Start(ref res);

                if (res != "")
                {
                    Error(res);
                    return;
                }

                _serverListening = true;
            }
            else
            {
                _objServerUDP.Start();
            }

            if (!_serverStarted)
            {
                _serverStarted = true;
                EventParameters ev = new EventParameters();
                ev.SetEvent(EventParameters.EventType.SERVER_START);
                Evsocket(ev);
            }

        }

        #region propiedades
        public bool ServerMode
        {
            get
            {
                return _serverMode;
            }
            set
            {
                _serverMode = value;

            }
        }

        public bool ClientMode
        {
            get
            {
                return _clientMode;
            }
            set
            {
                _clientMode = value;
            }
        }

        public int PortListeninServer
        {
            get
            {
                return _serverPortListening;
            }
            set
            {
                _serverPortListening = value;
            }
        }

        public int CodePage
        {
            get
            {
                return _codePage;
            }
            set
            {
                _codePage = value;
            }
        }

        public int ConnectionNumber
        {
            get
            {
                return _connectionNumber;
            }
            set
            {
                _connectionNumber = value;
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }
        }

        public int GetConnectionNumber()
        {
            return _numberServerConnections;
        }

        public int GetUltimoNumeroClienteConectado
        {
            get
            {
                return _numCliConServer;
            }
        }


        public string Host
        {
            get
            {
                return _host;
            }
            set
            {
                _host = value;
            }
        }

        public void Disconnect(int connectionNumber)
        {
            if (ClientMode)
            {
                int index = GetClientListIndex(connectionNumber);
                _lstObjClient[index].CloseConnection();
            }
        }

        public void DisconnectAll()
        {
            for (int i =0;i<_lstObjClient.Count();i++)
            {
                _lstObjClient[i].CloseConnection();
            }
        }

        /// <summary>
        /// disconnect a client from the server
        /// </summary>
        /// <param name="connectionNumber">Cliente connection number</param>
        public void DisconnectConnectedClientToMe(int connectionNumber)
        {
            if (ServerMode)
            {
                if (_connectionProtocol == Protocol.ConnectionProtocol.TCP)
                {
                    int clientIndexDisconnect = GetListIndexConnectedClientToServer(connectionNumber);
                    if (clientIndexDisconnect >= 0)
                    {
                        
                        _lstObjServerTCP[clientIndexDisconnect].DisconnectClient(_stopingServer, _lastClientConnected);
                        //_lastClientConnected = false;
                    }

                }
                else
                {
                    _objServerUDP.Disconnect(connectionNumber);
                }
            }
        }

        public void DisconnectAllConnectedClientsToMe()
        {
            _lastClientConnected = false;

            if (_connectionProtocol == Protocol.ConnectionProtocol.TCP)
            {
                List<int> lstObjServerClientsIndex = new List<int>();
                for (int i = 0; i < _lstObjServerTCP.Count(); i++)
                {
                    //desconecto y elimino los thread de todos incluso el objeto que esta ala espera de la nueva conexión
                    lstObjServerClientsIndex.Add(_lstObjServerTCP[i].IndexConnection);
                }
                
                for (int i=0; i< lstObjServerClientsIndex.Count();i++)
                {
                    if (i== lstObjServerClientsIndex.Count() -1)
                    {
                        _lastClientConnected = true;
                    }
                    DisconnectConnectedClientToMe(lstObjServerClientsIndex[i]);
                }
            }
            else
            {
                _objServerUDP.DisconnectAll();
            }
        }

        public void KillServer()
        {
            if (_serverMode)
            {
                _serverListening = false;
                _stopingServer = true;

                if (_connectionProtocol == Protocol.ConnectionProtocol.TCP)
                {
                    DisconnectAllConnectedClientsToMe();
                }
                else
                {
                    _objServerUDP.StopServer();
                }
            }
        }

        public bool ClientConnected(int connectionNumber)
        {

            return _lstObjClient[GetClientListIndex(connectionNumber)].conected;
            
        }
        #endregion

        //TODOS LOS EVENTOS DEL SERVIDOR Y CLIENTES, PRIMERO PASAN POR ACÁ
        private void Evsocket(EventParameters ev)
        {
            
            string message = "";
            bool showEvMaxConnections = false;

            ev.SetSocketInstance(this);

            if ((_ipClient != ""))
            {
                if (_ipClient != ev.GetClientIp)
                {
                    _ipClient = ev.GetClientIp;
                }
            }

            
            switch (ev.GetEventType)
            {
                case EventParameters.EventType.SERVER_NEW_CONNECTION:

                    if (_serverMode)
                    {
                        
                        if (_connectionProtocol == Protocol.ConnectionProtocol.TCP)
                        {
                            if (_numberServerConnections >= (_maxServerConnectionNumber - 1)) //muy cabeza, pero funciona
                            {
                                showEvMaxConnections = true; //genero el evento de limite de conexiones
                            }
                            _numberServerConnections++;
                            _numCliConServer++;
                            if (GetConnectionNumber() < _maxServerConnectionNumber)
                            {
                                Thread.Sleep(25); //por las dudas de que entren varias conexiones al mismo tiempo
                                CreateServer(ref message);
                                StartServer();
                            }
                        }
                    }
                        
                    break;

                case EventParameters.EventType.END_CONNECTION: //UDP no dispara este evento
                        
                    if (_serverMode)
                    {

                        _lstObjServerTCP.RemoveAt(ev.GetListIndex);
                        OrderClientList();
                        
                        
                        _numberServerConnections--;

                        /*
                        if (!_serverListening && !_stopingServer)
                        {
                            CreateServer(ref message);
                            StartServer();
                        }*/
                        
                    }
                    else
                    {
                        int cliIndex;
                        {
                            cliIndex = GetClientListIndex(ev.GetConnectionNumber);
                            _lstObjClient.RemoveAt(cliIndex);
                        }

                    }
                    break;

            }

            EventSocket(ev); //envío el evento a quien lo este consumiendo(?)

            //pongo esto acá ya que tengo que ser lo último que muestro
            if (showEvMaxConnections)
            {
                showEvMaxConnections = false;
                _serverListening = false;
                EventParameters evMaxCon = new EventParameters();
                evMaxCon.SetEvent(EventParameters.EventType.CONNECTION_LIMIT);
                EventSocket(evMaxCon);
            }

            if (_stopingServer && !_serverListening && _numberServerConnections == 0)
            {
                _stopingServer = false;
                _serverStarted = false;
                if (_connectionProtocol == Protocol.ConnectionProtocol.TCP)
                {
                    _lstObjServerTCP.Clear();
                }

                EventParameters evStopServer = new EventParameters();
                evStopServer.SetEvent(EventParameters.EventType.SERVER_STOP);
                EventSocket(evStopServer);
            }
        }



        public void Send(int connectionNumber,string message)
        {
            if (_serverMode)
            {
                //if (_tcp)
                if (_connectionProtocol == Protocol.ConnectionProtocol.TCP)
                {
                    _lstObjServerTCP[GetListIndexConnectedClientToServer(connectionNumber)].Send(message);
                }
                else
                {
                    _objServerUDP.Send(connectionNumber, message);
                }
            }
            else
            {
                _lstObjClient[GetClientListIndex(connectionNumber)].Send(message);
            }
        }

        /// <summary>
        /// envía un message al cliente o al servidor. si hay un error se dispara un evento de error
        /// </summary>
        /// <param name="message">message a enviar</param>
        public void Send(string message, int index = 0)
        {

            if (_serverMode)
            {
                //if (_tcp)
                if (_connectionProtocol == Protocol.ConnectionProtocol.TCP)
                {
                    if (_lstObjServerTCP[index].Connected)
                    {
                        _lstObjServerTCP[index].Send(message); //saczar ref res
                    }
                }
                else
                {
                    _objServerUDP.Send(message, index);
                }
            }

            if (_clientMode)
            {
                _lstObjClient[index].Send(message);
            }
        }

        /// <summary>
        /// send a message to all clientes connected
        /// </summary>
        /// <param name="message"></param>
        public void SendAll(string message)
        {
            try
            {
                if (_serverMode)
                {
                    //if (_tcp)
                    if (_connectionProtocol == Protocol.ConnectionProtocol.TCP)
                    {
                        for (int i = 0; i < _lstObjServerTCP.Count(); i++)
                        {
                            Send(message, i);
                        }
                    }
                    else
                    {
                        _objServerUDP.SendAll(message);
                    }
                }
                else
                {
                    for (int i =0;i<_lstObjClient.Count();i++)
                    {
                        Send(message, i);
                    }
                }
            }
            catch (Exception err)
            {
                GenerateErrorEvent(err);
            }

        }


        public void SendArray(byte[] memArray, int clusterSize, int connectionNumber)
        {
            try
            {

                SendArrayParamsContainer sendArrayParams = new SendArrayParamsContainer();
                sendArrayParams.memArray = memArray;
                sendArrayParams.clusterSize = clusterSize;
                sendArrayParams.connectionNumber = connectionNumber;

                Thread sendArrayThread = new Thread(new ParameterizedThreadStart(SendArrayThread));
                sendArrayThread.Name = "sendArrayThread_" + connectionNumber.ToString();
                sendArrayThread.Start(sendArrayParams);

            }
            catch (Exception err)
            {
                GenerateErrorEvent(err);
            }
        }

        public String GetClientHost(int ConnectionNumber)
        {
            return _lstObjClient[GetClientListIndex(ConnectionNumber)].GetHost;
        }
        public int GetClientPort(int ConnectionNumber)
        {
            return _lstObjClient[GetClientListIndex(ConnectionNumber)].GetPort;
        }

        public int GetClientListIndex(int connectionNumber)
        {
            for (int i =0; i<_lstObjClient.Count();i++)
            {
                if (connectionNumber == _lstObjClient[i].GetConnectionNumber)
                {
                    return i;
                }
            }

            return -1;
        }

        private void SendArrayThread(object sendArrayParams)
        {
            string data = "";
            int actualPositionNumber = 0;
            int nSize;
            int resultNumber = 0;
            //int nPosLectura = 0;
            int readingPositionNumber = 0;
            int conditionNumber;

            SendArrayParamsContainer aux = (SendArrayParamsContainer)sendArrayParams;
            SendArrayParamsContainer sendParams = new SendArrayParamsContainer();
            sendParams.clusterSize = aux.clusterSize;
            sendParams.connectionNumber = aux.connectionNumber;
            sendParams.memArray = aux.memArray;

            nSize = sendParams.memArray.Length; //memArray.Length;

            if (nSize <= sendParams.clusterSize) //TamCluster)
            {
                //TamCluster = nTam; //sí es mas chico lo que mando que el cluster
                sendParams.clusterSize = nSize;
            }

            try
            {

                while (actualPositionNumber < nSize - 1) //quizas aca me falte un byte (-1)
                {
                    conditionNumber = actualPositionNumber + sendParams.clusterSize;
                    for (int I = actualPositionNumber; I <= conditionNumber - 1; I++)
                    {
                        //meto todo al string para manadar
                        data = data + Convert.ToChar(sendParams.memArray[I]);
                        readingPositionNumber++;
                    }

                    //me re acomodo en el array
                    resultNumber = nSize - readingPositionNumber;
                    if (resultNumber <= sendParams.clusterSize)
                    {
                        sendParams.clusterSize = resultNumber; //ya estoy en el final y achico el cluster
                    }


                    actualPositionNumber = readingPositionNumber;
                    EventParameters ev = new EventParameters();
                    ev.SetPosition(actualPositionNumber).SetEvent(EventParameters.EventType.SEND_POSITION);
                    EventSocket(ev);

                    //ACÁ ENVIO LOS DATOS
                    //Send(data, sendParams.indexConection);
                    Send(sendParams.connectionNumber, data);

                    ev.SetEvent(EventParameters.EventType.SEND_COMPLETE).SetPosition(actualPositionNumber);
                    EventSocket(ev);

                    Thread.Sleep(5);

                    data = ""; //limpio la cadena
                }//fin while

                EventParameters evSendArrayComplete = new EventParameters();
                evSendArrayComplete.SetEvent(EventParameters.EventType.SEND_ARRAY_COMPLETE);
                EventSocket(evSendArrayComplete);
            }
            catch (Exception err)
            {
                GenerateErrorEvent(err);
            }

        }

        private int GetLastSpaceFree()
        {
            int res = 0;
            //if (_tcp)
            if (_connectionProtocol == Protocol.ConnectionProtocol.TCP)
            {
                res = _lstObjServerTCP.Count();
            }

            return res;
        }

        private void OrderClientList()
        {
            try
            {
                if (_connectionProtocol == Protocol.ConnectionProtocol.TCP)
                {
                    for (int i = 0; i < _lstObjServerTCP.Count(); i++)
                    {
                        _lstObjServerTCP[i].ListIndex = i;
                    }
                }

            }
            catch (Exception err)
            {
                GenerateErrorEvent(err);
            }
        }

        /// <summary>
        /// retorna el numero de indice de la lista
        /// </summary>
        /// <param name="indiceCliente">le paso el numero de indice de cliente</param>
        /// <returns></returns>
        private int GetListIndexConnectedClientToServer(int connectionNumber)
        {
            try
            {
                if (_connectionProtocol == Protocol.ConnectionProtocol.TCP)
                {
                    for (int i = 0; i < _lstObjServerTCP.Count(); i++)
                    {
                        if (_lstObjServerTCP[i].IndexConnection == connectionNumber)
                        {
                            return i;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                GenerateErrorEvent(err);
            }
            return -1;
        }

        private void GenerateErrorEvent(Exception err, string mensajeOpcional = "")
        {
            Utils utils = new Utils();
            EventParameters ev = new EventParameters();
            if (mensajeOpcional != "")
            {
                mensajeOpcional = " " + mensajeOpcional;
            }
            ev.SetData(err.Message + mensajeOpcional).
                SetEvent(EventParameters.EventType.ERROR).
                SetErrorCode(utils.GetErrorCode(err));
            Evsocket(ev);
        }

    }
}
