using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.ComponentModel;

namespace Sockets
{
    internal class ServerTCP
    {
        #if DEBUG
            private /*static*/ bool debug_Mode = true;
        #else
            private /*static*/ bool debug_Mode = false;
        #endif

        private Thread thrClient;
        private Thread _thrClienteConexion;

        private TcpListener _tcpListen;
        private TcpClient _tcpClient;
        private Encoding _encoder;

        private int _indexConnection; //va a contener el indice de conexion
        private int _listIndex; //va a conetener el indice de la lista de sockets
        private bool _loopCommunicationClient;
        private bool _listening;

        private bool _connected;
        private int _receiveTimeout;

        internal bool waitConnection;
        //internal string ipConnection;
        internal int port;

        internal delegate void Delegate_Server_Event(EventParameters serverParametersEvent);
        internal event Delegate_Server_Event evento_servidor;
        private void Event_Server(EventParameters serverParametersEvent)
        {
            this.evento_servidor(serverParametersEvent);
        }
        
        internal bool Listen
        {
            get
            {
                return _listening;
            }
            set
            {
                _listening = false;
            }
        }

        internal int IndexConnection
        {
            get
            {
                return _indexConnection;
            }
            set
            {
                _indexConnection = value;
            }
        }

        internal int ListIndex
        {
            get
            {
                return _listIndex;
            }
            set
            {
                _listIndex = value;
            }
        }

        internal bool Connected
        {
            get
            {
                return _connected;
            }
        }

        internal int ReceiveTimeout
        {
            set
            {
                if (value != 0)
                {
                    _receiveTimeout = value * 1000;
                }
            }
        }

        /// <summary>
        /// setea el socket para la escucha
        /// </summary>
        /// <param name="portListening">port de escucha</param>
        /// <param name="Cod">codopage</param>
        /// <param name="Message">message que retorna en caso de error</param>
        /// <param name="tcp">define si la conexión es tpc o udp, vaor default true, si es falso, la conexion es udp</param>
        internal ServerTCP(int portListening, int Cod, ref string message)
        {
            _indexConnection = -1;
            try
            {
                CodePage(Cod, ref message);
                if (message != "")
                {
                    message = message + " No se pudo iniciar ConServer";
                    return;
                }

                port = portListening;
            }
            catch (Exception err)
            {
                message = err.Message;
                GenerateEventError(err);
            }
        }

        /// <summary>
        /// Inicia el sokect en escucha
        /// </summary>
        /// <param name="Message">Message que retorna en caso de error</param>
        internal void Start(ref string Message)
        {
            try
            {
                ThreadStart client;
                client = new ThreadStart(Listen_TCP);
                
                thrClient = new Thread(client);
                thrClient.Name = "ThTCP";
                thrClient.IsBackground = true;
                thrClient.Start();

            }
            catch (Exception err)
            {
                waitConnection = false;

                Message = err.Message;
                GenerateEventError(err);
            }
        }

        /*internal void Detener()
        {
            _loopCommunicationClient = false;
        }*/

        /// <summary>
        /// Detiene todas las conexiones
        /// </summary>
        /// <param name="Message">Message que retorna en caso de error</param>
        internal void DisconnectClient(bool serverStoping = false,bool lastClientConnected=false)
        {
            bool forceDisconnection;
            bool evServerStop = false;

            if (serverStoping)
            {
                forceDisconnection = true;
            }
            else
            {
                forceDisconnection = Connected;
            }


            try
            {
                if (thrClient != null)
                {
                    if (forceDisconnection)
                    {
                        _tcpListen.Stop();
                        if (_tcpClient != null)
                        {
                            _tcpClient.Close();


                            if ((serverStoping) && (lastClientConnected))
                            {
                                evServerStop = true;
                            }

                        }
                        else
                        {
                            evServerStop = true;
                        }


                        if (evServerStop)
                        {
                            EventParameters ev = new EventParameters();
                            ev.SetListening(waitConnection).SetEvent(EventParameters.EventType.SERVER_STOP);
                            GenerateEvent(ev);
                        }

                        thrClient.Abort();
                        _loopCommunicationClient = false;
                        //_thrClienteConexion.Abort();
                        Thread.EndCriticalRegion(); //esto cierra todo con o sin conexiones
                    }
                }
            }
            catch (Exception err)
            {
                if (debug_Mode == true)
                {
                    GenerateEventError(err);
                }
            }
        }

        private void Listen_TCP()
        {
            //bool _listening;

            TcpClient clientTCP = new TcpClient();

            _tcpListen = new TcpListener(IPAddress.Any, port);
            
            try
            {
                waitConnection = true;
                EventParameters ev = new EventParameters();
                ev.SetEvent(EventParameters.EventType.WAIT_CONNECTION).SetListening(true);
                GenerateEvent(ev);

                _tcpListen.Stop();
                _tcpListen.Start();
                _listening = true;
            }
            catch (Exception err)
            {
                //el error salta acá, porque ya abrí una nueva instancia que esta eschando acá.
                waitConnection = false;
                EventParameters evErr = new EventParameters();
                evErr.SetEvent(EventParameters.EventType.WAIT_CONNECTION);
                GenerateEvent(evErr); //ver porque puse dos eventos
                GenerateEventError(err);
                return;
            }

            do
            {
                try
                {
                    string sAux;

                    //Escuchando = true;
                    clientTCP = _tcpListen.AcceptTcpClient();
                    sAux = ((System.Net.IPEndPoint)(clientTCP.Client.RemoteEndPoint)).Address.ToString();

                    waitConnection = false;
                    EventParameters ev = new EventParameters();
                    ev.SetEvent(EventParameters.EventType.WAIT_CONNECTION);
                    GenerateEvent(ev);

                    try
                    {
                        _loopCommunicationClient = true;
                        _thrClienteConexion = new Thread(new ParameterizedThreadStart(ClientCommunication));
                        _thrClienteConexion.Name = "ThrCliente";
                        _thrClienteConexion.IsBackground = true;
                        _thrClienteConexion.Start(clientTCP);

                        ev.SetEvent(EventParameters.EventType.ACCEPT_CONNECTION).SetClientIp(sAux);
                        GenerateEvent(ev);

                        //ahora tendria que dejar de escuchar
                        _listening = false;
                        _tcpListen.Stop();
                    }
                    catch (Exception err)
                    {
                        _listening = false;
                        //ev.SetData(err.Message + " threadConexion").SetEvent(Parametrosvento.TipoEvento.ERROR);
                        //GenerateEvent(ev);
                        GenerateEventError(err, "threadConexion");

                    }
                }
                catch (Exception err)
                {
                    _listening = false;
                    _tcpListen.Stop();
                    GenerateEventError(err, "TcpListen.Accept()");
                    
                    //_thrClienteConexion.Abort();
                }

            } while (_listening == true);//fin do
        }

        private void ClientCommunication(object Cliente)
        {

            bool EveYaDisparado = false;

            try
            {
                _tcpClient = (TcpClient)Cliente;
                NetworkStream clientStream = _tcpClient.GetStream();
                string strData;

                _tcpClient = (TcpClient)Cliente;

                //levanto evento nueva conexion
                _connected = true;
                EventParameters ev = new EventParameters();
                ev.SetClientIp(_tcpClient.Client.RemoteEndPoint.ToString()).SetEvent(EventParameters.EventType.NEW_CONNECTION);
                GenerateEvent(ev);

                if (_receiveTimeout != 0)
                {
                    _tcpClient.ReceiveTimeout = _receiveTimeout;
                }
                

                byte[] message = new byte[65535];

                int bytesRead;

                while (_loopCommunicationClient)
                {
                    bytesRead = 0;

                    try
                    {
                        bytesRead = clientStream.Read(message, 0, 65535);
                    }
                    catch(Exception error)
                    {
                        if (error.HResult == -2146232800)
                        {
                            EventParameters evRecieveTimeOut = new EventParameters();
                            evRecieveTimeOut.SetEvent(EventParameters.EventType.RECIEVE_TIMEOUT).SetClientIp(_tcpClient.Client.RemoteEndPoint.ToString());
                            GenerateEvent(evRecieveTimeOut);
                        }

                        EveYaDisparado = true;
                        _connected = false;
                        _tcpClient.Close();
                        
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        //el cliente se desconecto!
                        _connected = false;
                        _tcpClient.Close();
                        ev.SetData("").SetEvent(EventParameters.EventType.END_CONNECTION);
                        //EveYaDisparado = true;
                        break;
                    }

                    //llegó el message
                    strData = _encoder.GetString(message, 0, bytesRead);
                    ev.SetData(strData)
                        .SetClientIp(_tcpClient.Client.RemoteEndPoint.ToString())
                        .SetEvent(EventParameters.EventType.DATA_IN)
                        .SetSize(strData.Length);
                    GenerateEvent(ev);
                }

                if (!EveYaDisparado)
                {
                    //el cliente cerro la conexion
                    _connected = false;
                    _tcpClient.Close();
                }

                ev.SetEvent(EventParameters.EventType.END_CONNECTION).SetData("");
                GenerateEvent(ev);
                
            }
            catch (Exception err)
            {
                GenerateEventError(err);
            }
        }

        /// <summary>
        /// Envia un message al cliente conectado
        /// </summary>
        /// <param name="Indice">Indice de conexion al que se el envia el message</param>
        /// <param name="Datos">el message a enviar</param>
        /// <param name="Resultado">Message que retorna en caso de error</param>
        //internal void Send(int Indice,string Datos, ref string Resultado)
        internal void Send(string data)
        {
            int buf=0;
            try
            {
                TcpClient TcpClienteDatos = _tcpClient;
                NetworkStream clienteStream = TcpClienteDatos.GetStream();

                byte[] buffer = _encoder.GetBytes(data);
                buf = buffer.Length;
                clienteStream.Write(buffer, 0, buf);
                clienteStream.Flush(); //envio los datos
                
                EventParameters ev = new EventParameters();
                ev.SetSize(buf).SetEvent(EventParameters.EventType.SEND_COMPLETE);
                GenerateEvent(ev);

            }
            catch (Exception err)
            {
                GenerateEventError(err);
            }
        }

        /// <summary>
        /// Code page para iniciar la comunicacion
        /// </summary>
        /// <param name="Codigo">codigo de codepage</param>
        /// <param name="Error">Message que retorna en caso de error</param>
        internal void CodePage(int CodePageCode, ref string error)
        {
            try
            {
                _encoder = Encoding.GetEncoding(CodePageCode);
            }
            catch (Exception err)
            {
                error = err.Message;
                GenerateEventError(err);
            }
        }

        private void GenerateEvent(EventParameters ob)
        {
            ob.SetConnectionNumber(_indexConnection).SetListIndex(_listIndex);
            Event_Server(ob);
        }

        private void GenerateEventError(Exception err,string optionalMessage="")
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
                ev.SetListening(waitConnection).
                    SetData(err.Message + optionalMessage).
                    SetEvent(EventParameters.EventType.ERROR).
                    SetErrorCode(utils.GetErrorCode(err));
                    //SetLineNumberError(utils.GetNumeroDeLineaError(err));
                GenerateEvent(ev);
            }
        }

    }
}
