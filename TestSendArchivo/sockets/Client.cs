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
        //private Thread _thr_TimeOut; //hilo que setea cuando se inicia el timer de timeout de intento de conexion
        private bool _tcp;
        private UdpClient _clientSockUDP;
        private IPEndPoint _epUDP;
        private int _connectionNumber;
        private int _timeOutValue;
        private Encoding _encoder;
        private string _host;
        private int _port;
        private bool _closingConnection = false;

        /// <summary>
        /// Verdadero estoy conectado, falso, no estoy conectado
        /// </summary>
        internal bool conected; //me dice si estoy conectado o no

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

        internal Client(bool tcp=true)
        {
            _tcp = tcp;
            
        }

        internal void Connect(int connectionNumber,string host, int port)
        {
            _host = host;
            _port = port;
            _connectionNumber = connectionNumber;

            if (_tcp)
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
            ;
            
            try
            {
                conected = false;

                _clientSockTCP = new TcpClient();

                nPort = port;

                for (int i = 0; i < _timeOutValue; ++i)
                {
                    try
                    {
                        _clientSockTCP.Connect(host, nPort);
                        break; // salgo del for

                    }
                    catch
                    {
                        Thread.Sleep(1000); //espero un segundo y vuelvo a intentar
                    }

                }


                if (_clientSockTCP.Connected)
                {

                    conected = true;

                    EventParameters ev = new EventParameters();
                    ev.SetEvent(EventParameters.EventType.CONNECTION_OK);
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
                    evTime.SetEvent(EventParameters.EventType.TIME_OUT);
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
            Error(errorDescripcion.Message);
            
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
                        Error(error.Message);

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
                Error(err.Message);
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
                        //.SetData((Encoding.ASCII.GetString(datosInUDP, 0, datosInUDP.Length)))
                        .SetData(_encoder.GetString(dataInUDP))
                        .SetIpOrigen(_epUDP.ToString());
                    GenerateEvent(ev);
                }

            }
            catch(Exception err)
            {
                Error(err.Message);
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

        private void Error(string message)
        {
            EventParameters evErr = new EventParameters();
            evErr.SetEvent(EventParameters.EventType.ERROR).SetData(message);
            GenerateEvent(evErr);
        }

        internal void Send(string datos)
        {
            if (_tcp)
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
                    
                    Error("not connected");
                }
            }
            catch (Exception err)
            {
                Error(err.Message);
            }
        }

        private void Send_UDP(string datos)
        {
            
            byte[] bytesEnviar = _encoder.GetBytes(datos); //el ejemplo

            _clientSockUDP.Send(bytesEnviar, bytesEnviar.Length);
            EventParameters ev = new EventParameters();
            ev.SetEvent(EventParameters.EventType.SEND_COMPLETE).SetSize(bytesEnviar.Length).SetIpDestino(_epUDP.ToString());
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
        /// <param name="Codigo">Código de pagina</param>
        /// <param name="Error">Sí hay un error se guarda en Error, de caso contrario queda vacio</param>
        internal void CodePage(int Codigo)
        {
            try
            {
                _encoder = Encoding.GetEncoding(Codigo);
            }
            catch (Exception err)
            {
                Error(err.Message);
            }
        }

        
        
        private void GenerateEvent(EventParameters ob)
        {
            ob.SetConnectionNumber(_connectionNumber).SetTCP(_tcp);

            Client_Event(ob);
        }
    }
}
