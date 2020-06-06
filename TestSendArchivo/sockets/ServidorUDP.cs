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
            internal IPEndPoint clienteEndPoint;
            internal bool primerMensaje;
        }
        private List<InfoCliente> _lstClientesUDP;
        //private static Socket _serverSocketUDP;
        //private IPEndPoint _serverUDP;

        //private static IPEndPoint _remoteEP;

        //lo viejo
        private Thread _thrCliente;
        private UdpClient _udpClient;
        //private List<UdpClient> _lstUDPClient;
        //private IPEndPoint _remoteEP;
        private int _indiceCon; //va a contener el indice de conexion
        private int _indiceLista; //va a conetener el indice de la lista de sockets
        ///si es el primer mensaje, es una conexión nueva y tengo que hacer saltar el evento de nueva conexión
        //private bool _primerMensajeCliUDP;
        private int _puerto;

        internal string ip_Conexion;
        //internal int puerto;
        internal bool EsperandoConexion;

        private bool _conectado;
                

        internal int IndiceConexion
        {
            get
            {
                return _indiceCon;
            }
            set
            {
                _indiceCon = value;
            }
        }


        internal int IndiceLista
        {
            get
            {
                return _indiceLista;
            }
            set
            {
                _indiceLista = value;
            }
        }

        internal delegate void Delegado_Servidor_Event(Parametrosvento servidorParametrosEvento);
        internal event Delegado_Servidor_Event evento_servidor;
        private void Evento_Servidor(Parametrosvento servidorParametrosEvento)
        {
            this.evento_servidor(servidorParametrosEvento);
        }

        internal ServidorUDP(int PuertoEscucha)
        {
            /*if (_serverSocketUDP == null)
            {
                _serverSocketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            }*/

            _puerto = PuertoEscucha;
            _lstClientesUDP = new List<InfoCliente>();
            
            
        }

        internal void Iniciar()
        {
            //_serverUDP = new IPEndPoint(IPAddress.Any, _puerto);
            //_serverSocketUDP.Bind(_serverUDP);
            //EscucharUDP();

            try
            {
                ThreadStart Cliente;
                Cliente = new ThreadStart(EscucharUDP);

                _thrCliente = new Thread(Cliente);
                _thrCliente.Name = "ThTCP";
                _thrCliente.IsBackground = true;
                _thrCliente.Start();
            }
            catch (Exception err)
            {
                EsperandoConexion = false;

                GenerarEventoError(err);
            }

        }

        internal void Enviar(string datos, ref string resultado)
        {
            int buf = 0;
            Byte[] sendBytes = Encoding.ASCII.GetBytes(datos);
            try
            {
                //_udpClient.Send(sendBytes, sendBytes.Length, _remoteEP);

                Parametrosvento ev = new Parametrosvento();
                ev.SetSize(buf).SetEvento(Parametrosvento.TipoEvento.ENVIO_COMPLETO);
                GenerarEvento(ev);

            }
            catch (Exception e)
            {
                
                GenerarEventoError(e);
            }


        }

        private void EscucharUDP()
        {
            int indiceMsg=0;
            UdpClient auxCli;
            
            try
            {
                
                bool clienteExistente = false;

                /*
                if (_remoteEP == null)
                {
                    _remoteEP = new IPEndPoint(IPAddress.Any, _puerto);
                }*/

                UdpClient clienteUDP = new UdpClient();
                clienteUDP.ExclusiveAddressUse = false;
                clienteUDP.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                //InfoCliente aux = new InfoCliente();
                //aux.puerto = _remoteEP.Port;


                //aux.udpClient = new UdpClient();
                //aux.udpClient.ExclusiveAddressUse = false;
                //aux.udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                //aux.udpClient.Client.Bind(_remoteEP);
                //aux.primerMensaje = false;

                bool bindeado = false;
                while (true)
                {
                    InfoCliente auxCliente = new InfoCliente();
                    auxCliente.clienteEndPoint = new IPEndPoint(IPAddress.Any, _puerto);
                    if (!bindeado)
                    {
                        clienteUDP.Client.Bind(auxCliente.clienteEndPoint);
                        bindeado = true;
                    }
                    var datosEntrada = clienteUDP.Receive(ref auxCliente.clienteEndPoint); //ACÁ LLEGA TODO SIEMPRE Y AHI ES DONDE TENGO QUE VER QUE HACER

                    if (auxCliente.clienteEndPoint.Port != _puerto)
                    {

                        for (int i = 0; i < _lstClientesUDP.Count(); i++)
                        {

                            if (_lstClientesUDP[i].clienteEndPoint.Port == auxCliente.clienteEndPoint.Port)
                            {
                                clienteExistente = true;
                                indiceMsg = i;
                                i = _lstClientesUDP.Count();
                            }
                        }
                        if (!clienteExistente)
                        {
                            _lstClientesUDP.Add(auxCliente);
                            indiceMsg = _lstClientesUDP.Count() - 1;
                            _lstClientesUDP[indiceMsg].primerMensaje = true;

                        }

                        //var datos = _udpClient.Receive(ref _remoteEP);
                        //var datos = _lstClientesUDP[indiceMsg].udpClient.Receive(ref _remoteEP);
                        ip_Conexion = _lstClientesUDP[indiceMsg].clienteEndPoint.Address.ToString();

                        if (_lstClientesUDP[indiceMsg].primerMensaje)
                        {
                            _lstClientesUDP[indiceMsg].primerMensaje = false;

                            Parametrosvento aceptarCon = new Parametrosvento();
                            aceptarCon.SetEvento(Parametrosvento.TipoEvento.ACEPTAR_CONEXION)
                                .SetIpOrigen(ip_Conexion)
                                .SetIndiceLista(indiceMsg);
                            
                            GenerarEvento(aceptarCon);

                            //mantengo esto para que sea el orden de eventos tal como es en tcp
                            //tendría que agregar algo que permita rechazar la conexion dsp de que se dispara ACEPTAR_CONEXION

                            //Parametrosvento nuevaCon = new Parametrosvento();
                            //nuevaCon.SetEvento(Parametrosvento.TipoEvento.NUEVA_CONEXION).SetIpOrigen(ip_Conexion);
                            //GenerarEvento(nuevaCon);

                            _conectado = true;
                            //_udpClient.Close(); //para pruebas

                        }

                        Parametrosvento ev = new Parametrosvento();
                        ev.SetDatos(Encoding.ASCII.GetString(datosEntrada, 0, datosEntrada.Length))
                            .SetIpOrigen(ip_Conexion)
                            .SetEvento(Parametrosvento.TipoEvento.DATOS_IN)
                            .SetIndiceLista(indiceMsg);
                        GenerarEvento(ev);
                    } //fin if (_remoteEP.Port == _puerto) 
                }
                
            }
            catch (Exception e)
            {
                _conectado = false;
                //_primerMensajeCliUDP = false;
                //_udpClient.Close();
                GenerarEventoError(e);

            }
        }

        private void GenerarEvento(Parametrosvento ob)
        {
            //ob.SetNumConexion(_indiceCon).SetIndiceLista(_indiceLista);
            Evento_Servidor(ob);
        }

        private void GenerarEventoError(Exception err, string mensajeOpcional = "")
        {
            Utils utils = new Utils();
            Parametrosvento ev = new Parametrosvento();
            if (mensajeOpcional != "")
            {
                mensajeOpcional = " " + mensajeOpcional;
            }
            ev.SetEscuchando(EsperandoConexion).
                SetDatos(err.Message + mensajeOpcional).
                SetEvento(Parametrosvento.TipoEvento.ERROR).
                SetCodError(utils.GetCodigoError(err));
            GenerarEvento(ev);
        }
        
    }
}
