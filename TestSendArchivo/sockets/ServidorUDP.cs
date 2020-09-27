using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sockets
{
    internal class ServidorUDP
    {

        class InfoCliente
        {
            internal IPEndPoint clientEndPoint;
            internal bool firtsMessage;
            internal int connectionNumber;
            internal string dataIn;
        }
        private List<InfoCliente> lstClientsUDP;

        private Thread thrClient;
        private UdpClient _udpClient;

        private int _port;
        private int _maxNumberClientsUDP;
        private int _numberConnections = 0;
        private bool _maximunConnections;
        private bool _serverStarted;
        //private bool _maximunConnectionsReached;
        internal string _ipConnection;
        private Encoding _encoder;

        private bool _connected;
                
        internal int MaxClientUDP
        {
            get
            {
                return _maxNumberClientsUDP;
            }
            set
            {
                _maxNumberClientsUDP = value;
            }
        }

        internal delegate void Delegate_Server_Event(EventParameters serverParametersEvent);
        internal event Delegate_Server_Event evento_servidor;
        private void Event_Server(EventParameters serverParametersEvent)
        {
            this.evento_servidor(serverParametersEvent);
        }

        internal ServidorUDP(int portListening)
        {
            _port = portListening;
            lstClientsUDP = new List<InfoCliente>();
        }

        
        /// <summary>
        /// Code page para iniciar la comunicacion
        /// </summary>
        /// <param name="Codigo">codigo de codepage</param>
        /// <param name="Error">Message que retorna en caso de error</param>
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

        internal void Start()
        {
            try
            {
                _serverStarted = true;
                ThreadStart Cliente;
                Cliente = new ThreadStart(ListenUDP);

                thrClient = new Thread(Cliente);
                thrClient.Name = "ThUDP";
                thrClient.IsBackground = true;
                thrClient.Start();
            }
            catch (Exception err)
            {
                GenerateEventError(err);
            }

        }

        internal void Send(string data,int index)
        {
            int buf = 0;
            Byte[] sendBytes = _encoder.GetBytes(data);
            try
            {
                buf = sendBytes.Length;
                _udpClient.Send(sendBytes, buf, lstClientsUDP[index].clientEndPoint);
                
                EventParameters ev = new EventParameters();
                ev.SetSize(buf)
                .SetEvent(EventParameters.EventType.SEND_COMPLETE)
                .SetListIndex(index)
                .SetConnectionNumber(lstClientsUDP[index].connectionNumber);
                GenerateEvent(ev);

            }
            catch (Exception err)
            {
                
                GenerateEventError(err);
            }

        }

        internal void SendAll(string data)
        {
            for (int i =0;i<lstClientsUDP.Count();i++)
            {
                Send(data, i);
            }
        }

        private void ListenUDP()
        {
            int indexMsg=0;
            bool generateDataInEvent = false;

            try
            {
                bool existingClient = false;

                _udpClient = new UdpClient();

                _udpClient.ExclusiveAddressUse = false;
                _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                //bool bindeado = false;
                bool binded = false;
                while (_serverStarted)
                {
                    try
                    {
                        existingClient = false;
                        generateDataInEvent = false;
                        InfoCliente auxClient = new InfoCliente();
                        auxClient.clientEndPoint = new IPEndPoint(IPAddress.Any, _port);
                        if (!binded)
                        {
                            _udpClient.Client.Bind(auxClient.clientEndPoint);
                            binded = true;
                        }

                        //ACÁ LLEGA TODO SIEMPRE Y AHÍ ES DONDE TENGO QUE VER QUE HACER
                        var datosEntrada = _udpClient.Receive(ref auxClient.clientEndPoint);

                        if (auxClient.clientEndPoint.Port != _port) //me fijo no ser yo mismo
                        {

                            for (int i = 0; i < lstClientsUDP.Count(); i++)
                            {
                                if (lstClientsUDP[i].clientEndPoint.Port == auxClient.clientEndPoint.Port)
                                {
                                    existingClient = true;
                                    indexMsg = i;                                    
                                    lstClientsUDP[i].dataIn = _encoder.GetString(datosEntrada);

                                    generateDataInEvent = true;
                                    i = lstClientsUDP.Count();
                                }
                            }
                            if (!existingClient)
                            {
                                //SI _maxNumberClientsUDP es mayor a 0 hay un máximo de conexiones
                                bool evaluarMaximoClientesUDP = false; 
                                if (_maxNumberClientsUDP > 0)
                                {
                                    //evaluarMaximoClientesUDP = true;
                                    if (_numberConnections < _maxNumberClientsUDP)
                                    {
                                        evaluarMaximoClientesUDP = true;
                                    }
                                    else
                                    {
                                        evaluarMaximoClientesUDP = false;
                                    }
                                }
                                else
                                {
                                    evaluarMaximoClientesUDP = true; //no hay máximo de conexiones
                                }

                                //if ((_numberConnections < _maxNumberClientsUDP) && ) 
                                if (evaluarMaximoClientesUDP)
                                {
                                    lstClientsUDP.Add(auxClient);
                                    indexMsg = lstClientsUDP.Count() - 1;
                                    lstClientsUDP[indexMsg].firtsMessage = true;
                                    lstClientsUDP[indexMsg].dataIn = Encoding.ASCII.GetString(datosEntrada, 0, datosEntrada.Length);
                                    generateDataInEvent = true;
                                }
                                else
                                {
                                    //LIMITE DE CONEXIONES
                                    if (!_maximunConnections) //ya mostre el evento
                                    {
                                        _maximunConnections = true; //así evito disparar esto siempre
                                        EventParameters maxCons = new EventParameters();
                                        maxCons.SetEvent(EventParameters.EventType.CONNECTION_LIMIT);
                                        GenerateEvent(maxCons);
                                    }

                                }

                            }

                            if (generateDataInEvent) //lo que llego esta fuera del limite de conexiones
                            {

                                _ipConnection = lstClientsUDP[indexMsg].clientEndPoint.Address.ToString();

                                if (lstClientsUDP[indexMsg].firtsMessage)
                                {
                                    lstClientsUDP[indexMsg].firtsMessage = false;
                                    _numberConnections++;
                                    lstClientsUDP[indexMsg].connectionNumber = _numberConnections;

                                    EventParameters acceptCon = new EventParameters();
                                    acceptCon.SetEvent(EventParameters.EventType.ACCEPT_CONNECTION)
                                        .SetIpOrigen(_ipConnection)
                                        .SetListIndex(indexMsg)
                                        .SetConnectionNumber(lstClientsUDP[indexMsg].connectionNumber);

                                    GenerateEvent(acceptCon);

                                    //mantengo esto para que sea el orden de eventos tal como es en tcp
                                    //tendría que agregar algo que permita rechazar la conexion dsp de que se dispara ACCEPT_CONNECTION

                                    EventParameters newCon = new EventParameters();
                                    newCon.SetEvent(EventParameters.EventType.NEW_CONNECTION)
                                    .SetIpOrigen(_ipConnection)
                                    .SetListIndex(indexMsg)
                                    .SetConnectionNumber(lstClientsUDP[indexMsg].connectionNumber);

                                    GenerateEvent(newCon);

                                    _connected = true;

                                }

                                EventParameters ev = new EventParameters();
                                ev.SetData(lstClientsUDP[indexMsg].dataIn)
                                    .SetIpOrigen(_ipConnection)
                                    .SetEvent(EventParameters.EventType.DATA_IN)
                                    .SetListIndex(indexMsg)
                                    .SetConnectionNumber(lstClientsUDP[indexMsg].connectionNumber);
                                GenerateEvent(ev);

                            } //fin if (generarEventoDatosIn)
                        } //fin if (_remoteEP.Port == _port) 
                    }
                    catch(Exception err)
                    {
                        //-2146233040 error que se da cuando se detiene el server udp
                        //-2147467259 cuando llega un message udp simple
                        if (err.HResult != -2146233040 && err.HResult != -2147467259)
                        {
                            GenerateEventError(err);
                        }
                    }
                }
                
            }
            catch (Exception err)
            {
                _connected = false;
                
                _udpClient.Close();
                //-2146233040 error que se da cuando se detiene el server udp
                //-2147467259 cuando llega un message udp simple
                if (err.HResult != -2146233040 && err.HResult != -2147467259) //error que se da cuando se detiene el server udp
                {
                    GenerateEventError(err);
                }

            }
        }

        internal void Disconnect(int connectionNumber)
        {
            for (int i = 0;i<lstClientsUDP.Count();i++)
            {
                if (lstClientsUDP[i].connectionNumber == connectionNumber)
                {
                    lstClientsUDP.RemoveAt(i);
                    _numberConnections--;
                    if (_maximunConnections)
                    {
                        _maximunConnections = false;
                    }

                }
            }
        }

        internal void DisconnectAll()
        {
            lstClientsUDP.Clear();
            _numberConnections = 0;
            _maximunConnections = false;
        }

        internal void StopServer()
        {
            //thrClient.Abort();   
            _udpClient.Close();
            _serverStarted = false;
            thrClient.Abort();

            EventParameters ev = new EventParameters();
            ev.SetEvent(EventParameters.EventType.SERVER_STOP);
            GenerateEvent(ev);

        }

        private void GenerateEvent(EventParameters ob)
        {
            Event_Server(ob);
        }
        
        private void GenerateEventError(Exception err, string optionalMessage = "")
        {
            Utils utils = new Utils();
            EventParameters ev = new EventParameters();
            if (optionalMessage != "")
            {
                optionalMessage = " " + optionalMessage;
            }
            //ev.SetListening(waitConnection).
            ev.SetData(err.Message + optionalMessage).
            SetEvent(EventParameters.EventType.ERROR).
            SetErrorCode(utils.GetErrorCode(err));
                //SetLineNumberError(utils.GetNumeroDeLineaError(err));
            GenerateEvent(ev);
        }
        
    }
}
