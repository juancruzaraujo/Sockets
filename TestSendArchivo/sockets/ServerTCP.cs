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
        private string _clientIp;

        internal bool waitConnection;
        internal int port;
        //internal bool serverDisconnecting;


        //ThreadStart client;


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

        internal string GetClientIp
        {
            get
            {
                return _clientIp;
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
                //ARREGLAR ESTO
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

                //en debug reemplaza este thread, en release crea uno nuevo y eso hace que falle

                //if (thrClient == null)
                {
                    ThreadStart client;
                    client = new ThreadStart(Listen_TCP);


                    thrClient = new Thread(client);
                    thrClient.Name = "ThTCP" + _listIndex;
                    thrClient.IsBackground = true;
                    thrClient.Start();
                }
                

            }
            catch (Exception err)
            {
                waitConnection = false;

                Message = err.Message;
                GenerateEventError(err);
            }
        }



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

                        

                        thrClient.Abort();
                        _loopCommunicationClient = false;
                        
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

            _tcpListen = new TcpListener(IPAddress.Any,port);

            //string test1;
            try
            {
                waitConnection = true;
                EventParameters ev = new EventParameters();
                ev.SetEvent(EventParameters.EventType.SERVER_WAIT_CONNECTION).SetListening(true);
                GenerateEvent(ev);


                /*
                 * en release este thread no se cierra a tiempo
                 * por eso y se levanta otro de este tipo y ahí falla
                */
                _tcpListen.Stop();                
                _tcpListen.Start();

                _listening = true;
            }
            catch (Exception err)
            {
                //el error salta acá, porque ya abrí una nueva instancia que esta eschando acá.
                waitConnection = false;
                EventParameters evErr = new EventParameters();
                //evErr.SetEvent(EventParameters.EventType.SERVER_WAIT_CONNECTION);
                //GenerateEvent(evErr); //ver porque puse dos eventos
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
                    _clientIp = ((System.Net.IPEndPoint)(clientTCP.Client.RemoteEndPoint)).Address.ToString();

                    waitConnection = false;
                    EventParameters ev = new EventParameters();
                    ev.SetEvent(EventParameters.EventType.SERVER_WAIT_CONNECTION);
                    GenerateEvent(ev);

                    try
                    {
                        _loopCommunicationClient = true;
                        _thrClienteConexion = new Thread(new ParameterizedThreadStart(ClientCommunication));
                        _thrClienteConexion.Name = "ThrConnectionTCP_" + _listIndex;
                        _thrClienteConexion.IsBackground = true;
                        _thrClienteConexion.Start(clientTCP);


                        _listening = false;
                        _tcpListen.Stop();

                        ev.SetEvent(EventParameters.EventType.SERVER_ACCEPT_CONNECTION).SetClientIp(_clientIp);
                        GenerateEvent(ev);

                    }
                    catch (Exception err)
                    {
                        _listening = false;
                        _tcpListen.Stop();
                        //ev.SetData(err.Message + " threadConexion").SetEvent(Parametrosvento.TipoEvento.ERROR);
                        //GenerateEvent(ev);
                        Thread thread = Thread.CurrentThread;
                        GenerateEventError(err, thread.Name);

                    }
                }
                catch (Exception err)
                {
                    _listening = false;
                    _tcpListen.Stop();
                    GenerateEventError(err, "TcpListen.Accept()");
                    
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
                ev.SetClientIp(_tcpClient.Client.RemoteEndPoint.ToString()).SetEvent(EventParameters.EventType.SERVER_NEW_CONNECTION);
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
                    catch
                    {

                        bool timeOut = false;
                        try
                        {
                            if (_tcpClient.Client.RemoteEndPoint != null)
                            {
                                if (_tcpClient.Client.ReceiveTimeout != 0)
                                {
                                    timeOut = true;
                                }
                            }
                        }
                        catch
                        {
                            timeOut = false;
                        }

                        EveYaDisparado = true;
                        _connected = false;
                        _tcpClient.Close();

                        EventParameters eventDisconnection = new EventParameters();
                        eventDisconnection.SetClientIp(_clientIp);
                        if (timeOut)
                        {
                            eventDisconnection.SetEvent(EventParameters.EventType.RECIEVE_TIMEOUT);
                        }
                        else
                        {
                            eventDisconnection.SetEvent(EventParameters.EventType.END_CONNECTION);
                        }
                        GenerateEvent(eventDisconnection);

                        break;
                    }

                    if (bytesRead == 0)
                    {
                        //el cliente se desconecto!
                        _connected = false;
                        _tcpClient.Close();
                        ev.SetData("").SetEvent(EventParameters.EventType.END_CONNECTION);

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
                    ev.SetEvent(EventParameters.EventType.END_CONNECTION).SetData("");
                    GenerateEvent(ev);
                }
                
            }
            catch (Exception err)
            {
                GenerateEventError(err);
            }
        }

        internal void Send(string data)
        {
            int buf=0;
            try
            {

                TcpClient tcpClientData = _tcpClient;
                bool force = true;
                while(force)
                {
                    if (tcpClientData != null)
                    {
                        force = false;
                    }
                    else
                    {
                        tcpClientData = _tcpClient;
                    }
                }


                NetworkStream clienteStream = tcpClientData.GetStream();

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
            ob.SetConnectionNumber(_indexConnection).SetListIndex(_listIndex).SetIsServerEvent(true);
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
