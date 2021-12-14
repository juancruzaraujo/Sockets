using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;


namespace Sockets
{
    internal class Client
    {

        #if DEBUG
            private /*static*/ bool debug_Mode = true;
        #else
            private /*static*/ bool debug_Mode = false;
        #endif

        

        private TcpClient _clientSockTCP; //el socket en cuestion!
        private Thread _thrClient; //hilo con el flujo de datos
        Protocol.ConnectionProtocol _connectionProtocol;
        private UdpClient _clientSockUDP;
        private IPEndPoint _epUDP;
        private int _connectionNumber;
        private int _timeOutValue;
        private Encoding _encoder;
        private string _host;
        private int _port;
        private bool _closingConnection = false;
        private int _receiveTimeout;
        private string _clientTag;

        /// <summary>
        /// Verdadero estoy conectado, falso, no estoy conectado
        /// </summary>
        internal bool conected; //me dice si estoy conectado o no


        internal string ClientTag
        {
            get
            {
                return _clientTag;
            }
            set
            {
                _clientTag = value;
            }
        }

        /// <summary>
        /// Devuelve o establece el indice de conexion, necesario si se crea una lista o un vector de este objeto
        /// </summary>
        //internal int indexCon;
        internal int GetConnectionNumber
        {
           get
            {
                return _connectionNumber;
            }
        }
        
        internal string GetHost
        {
            get
            {
                return _host;
            }
        }

        internal int GetPort
        {
            get
            {
                return _port;
            }
        }

        /// <summary>
        /// reciebe timeout in secons. If the value is 0 there is no timeout
        /// </summary>
        internal int ReceiveTimeout
        {
            set
            {
                if (value !=0)
                {
                    _receiveTimeout = value * 1000;
                }
            }
        }

        /// <summary>
        /// Setea o devuelve el time out para poder conectarse en segundos.
        /// </summary>
        internal int SetGetTimeOut
        {
            get
            {
                return _timeOutValue;
            }
            set
            {
                _timeOutValue = value;
                if (_timeOutValue > 60)
                {
                    _timeOutValue = 60;
                }
            }
        }

        //private int _tipoCod;
        

        internal delegate void Delegated_Client_Event(EventParameters serverParametersEvent);
        internal event Delegated_Client_Event clientEvent;
        private void Client_Event(EventParameters serverParametersEvent)
        {
            this.clientEvent(serverParametersEvent);
        }

        internal Client(Protocol.ConnectionProtocol connectionProtocol = Protocol.ConnectionProtocol.TCP)
        {
            //_tcp = tcp;
            _connectionProtocol = connectionProtocol;
            
        }

        internal void Connect(int connectionNumber,string host, int port)
        {
            _host = host;
            _port = port;
            _connectionNumber = connectionNumber;

            if (_connectionProtocol == Protocol.ConnectionProtocol.TCP)
            {
                Connect_TCP(_host, _port);
            }
            else
            {
                Connect_UDP(host, _port);
            }
        }

        private void Connect_TCP(string host, int port)
        {
            int nPort = 0;


            try
            {
                conected = false;

                _clientSockTCP = new TcpClient();

                nPort = port;

                //https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient.receivetimeout?view=net-5.0
                if (_receiveTimeout != 0)
                {
                    _clientSockTCP.ReceiveTimeout = _receiveTimeout;
                }

                bool keep = true;
                DateTime startTime = DateTime.Now;
                while (keep)
                {
                    try
                    {
                        _clientSockTCP.Connect(host, nPort);
                    }
                    catch { } //ACA QUEDE por que hice esto?

                    if (_clientSockTCP.Connected)
                    {
                        keep = false;
                    }
                    DateTime endProcess = DateTime.Now;
                    TimeSpan elapsedTime = endProcess - startTime;
                    double segundosTotales = elapsedTime.TotalSeconds;
                    int seconds = elapsedTime.Seconds;

                    if (seconds > _timeOutValue)
                    {
                        keep = false;
                    }

                }


                if (_clientSockTCP.Connected)
                {

                    conected = true;

                    EventParameters ev = new EventParameters();
                    ev.SetEvent(EventParameters.EventType.CLIENT_CONNECTION_OK).SetServerIp(_host);
                    GenerateEvent(ev);

                    ThreadStart thrclienteTCP = new ThreadStart(DataFlow_TCP);
                    _thrClient = new Thread(thrclienteTCP);
                    _thrClient.Name = "ThrClientTCP";
                    _thrClient.Start();
                }
                else
                {
                    //Eve_TimeOut(indiceCon);
                    EventParameters evTime = new EventParameters();
                    evTime.SetEvent(EventParameters.EventType.CLIENT_TIME_OUT).SetServerIp(_host);
                    GenerateEvent(evTime);
                    return;
                }

            }
            catch (Exception error)
            {
                ErrorConnect(error);
            }
        }

        private void Connect_UDP(string host, int port)
        {
            try
            {
                _clientSockUDP = new UdpClient();
                _epUDP = new IPEndPoint(IPAddress.Parse(host), port);
                _clientSockUDP.Connect(_epUDP);

                ThreadStart thrclienteUDP = new ThreadStart(DataFlow_UDP);
                _thrClient = new Thread(thrclienteUDP);
                _thrClient.Name = "thrClientUDP";
                _thrClient.Start();
            }
            catch(Exception error)
            {
                ErrorConnect(error);
            }

        }

        private void ErrorConnect(Exception errorDescripcion)
        {
            conected = false;
            //Error(errorDescripcion.Message);
            GenerateEventError(errorDescripcion);


        }

        private void DataFlow_TCP()
        {
            try
            {
                TcpClient tcpCliente = _clientSockTCP;
                NetworkStream clientStream = tcpCliente.GetStream();

                byte[] message = new byte[65535];
                int bytesRead;
                string strDatos;

                while (true)
                {
                    bytesRead = 0;

                    try
                    {
                        //se bloquea hasta que llega un message
                        bytesRead = clientStream.Read(message, 0, 65535);

                    }
                    catch (Exception error)
                    {
                        if (error.HResult != -2146232800)
                        {
                            GenerateEventError(error);
                        }
                        else
                        {
                            EventParameters evRecieveTimeOut = new EventParameters();
                            evRecieveTimeOut.SetEvent(EventParameters.EventType.RECIEVE_TIMEOUT).SetServerIp(_host);
                            GenerateEvent(evRecieveTimeOut);
                        }

                        ConnectionEnd();
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        //el cliente se desconecto!
                        ConnectionEnd();
                        break;
                    }

                    strDatos = _encoder.GetString(message, 0, bytesRead);
                    
                    EventParameters ev = new EventParameters();
                    ev.SetEvent(EventParameters.EventType.DATA_IN).SetData(strDatos);
                    GenerateEvent(ev);

                }
                //el cliente cerro la conexion
                ConnectionEnd();
                tcpCliente.Close(); //Sí, se cierra el server.

                _thrClient.Abort();
            }
            catch (Exception err)
            {
                //Error(err.Message);
                GenerateEventError(err);
                ConnectionEnd();
            }
        } //fin DataFlow_TCP

        private void DataFlow_UDP()
        {
            try
            {
                while (true)
                {
                    var dataInUDP = _clientSockUDP.Receive(ref _epUDP);
                    //_encoder = Encoding.GetEncoding(code);

                    EventParameters ev = new EventParameters();
                    ev.SetEvent(EventParameters.EventType.DATA_IN)
                        .SetData(_encoder.GetString(dataInUDP))
                        .SetServerIp(_epUDP.ToString());
                    GenerateEvent(ev);
                }

            }
            catch(Exception err)
            {
                GenerateEventError(err);
            }
        }

        private void ConnectionEnd()
        {

            if (!_closingConnection)
            {
                _closingConnection = true;

                EventParameters ev = new EventParameters();
                ev.SetEvent(EventParameters.EventType.END_CONNECTION);
                GenerateEvent(ev);
            }

        }

        internal void Send(string datos)
        {
            //if (_tcp)
            if (_connectionProtocol == Protocol.ConnectionProtocol.TCP)
            {
                Send_TCP(datos);
            }
            else
            {
                Send_UDP(datos);
            }
        }

        private void Send_TCP(string datos)
        {
            try
            {
                if (conected == true)
                {
                    TcpClient TcpClienteDatos = _clientSockTCP;
                    NetworkStream clientStream = TcpClienteDatos.GetStream();

                    byte[] buffer = _encoder.GetBytes(datos);


                    clientStream.Write(buffer, 0, buffer.Length);
                    clientStream.Flush(); //envio lo datos

                    EventParameters ev = new EventParameters();
                    ev.SetEvent(EventParameters.EventType.SEND_COMPLETE);
                    GenerateEvent(ev);
                }
                else
                {
                    
                    //Error("not connected");
                }
            }
            catch (Exception err)
            {
                //Error(err.Message);
                GenerateEventError(err);
            }
        }

        private void Send_UDP(string datos)
        {
            
            byte[] bytesEnviar = _encoder.GetBytes(datos); //el ejemplo

            _clientSockUDP.Send(bytesEnviar, bytesEnviar.Length);
            EventParameters ev = new EventParameters();
            ev.SetEvent(EventParameters.EventType.SEND_COMPLETE).SetSize(bytesEnviar.Length).SetServerIp(_epUDP.ToString());
            GenerateEvent(ev);

        }

        internal void CloseConnection()
        {
            if (conected == true)
            {
                //System.Diagnostics.Debug.WriteLine("Cierro Ok"); //para pruebas
                _clientSockTCP.Close();
                _thrClient.Abort();

                ConnectionEnd();
            }
        }

        
        /// <summary>
        /// CodePage:
        ///     Setea el codigo de paginas de carectes para enviar y recibir
        /// </summary>
        /// <param name="code">Código de pagina</param>
        /// <param name="Error">Sí hay un error se guarda en Error, de caso contrario queda vacio</param>
        internal void CodePage(int code)
        {
            try
            {
                _encoder = Encoding.GetEncoding(code);
            }
            catch (Exception err)
            {
                GenerateEventError(err);
            }
        }

        
        
        private void GenerateEvent(EventParameters ob)
        {
            ob.SetConnectionNumber(_connectionNumber).SetTCP(_connectionProtocol).SetClientTag(_clientTag);

            Client_Event(ob);
        }


        private void GenerateEventError(Exception err, string optionalMessage = "")
        {
            if (err.HResult != -2146233040)
            {
                //-2146233040
                //-2146233040
                //-2146233040

                Utils utils = new Utils();
                EventParameters ev = new EventParameters();
                if (optionalMessage != "")
                {
                    optionalMessage = " " + optionalMessage;
                }
                ev.SetListening(false).
                    SetData(err.Message + optionalMessage).
                    SetEvent(EventParameters.EventType.ERROR).
                    SetErrorCode(utils.GetErrorCode(err));
                GenerateEvent(ev);
            }
        }
    }
}
