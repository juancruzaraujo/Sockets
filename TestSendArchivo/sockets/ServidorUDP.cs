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
            internal int numConexion;
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

        internal void Enviar(string datos,int indice)
        {
            int buf = 0;
            Byte[] sendBytes = Encoding.ASCII.GetBytes(datos);
            try
            {
                _udpClient.Send(sendBytes, sendBytes.Length, _lstClientesUDP[indice].clienteEndPoint);
                

                Parametrosvento ev = new Parametrosvento();
                ev.SetSize(buf)
                .SetEvento(Parametrosvento.TipoEvento.ENVIO_COMPLETO)
                .SetIndiceLista(indice)
                .SetNumConexion(_lstClientesUDP[indice].numConexion);
                GenerarEvento(ev);

            }
            catch (Exception e)
            {
                
                GenerarEventoError(e);
            }


        }

        internal void EnviarATodos(string datos)
        {
            for (int i =0;i<_lstClientesUDP.Count();i++)
            {
                Enviar(datos, i);
            }
        }

        private void EscucharUDP()
        {
            int indiceMsg=0;
            UdpClient auxCli;
            int cantidadConexiones = 0;            

            try
            {
                
                bool clienteExistente = false;

                /*
                if (_remoteEP == null)
                {
                    _remoteEP = new IPEndPoint(IPAddress.Any, _puerto);
                }*/

                //UdpClient clienteUDP = new UdpClient();
                _udpClient = new UdpClient();

                _udpClient.ExclusiveAddressUse = false;
                _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                bool bindeado = false;
                while (true)
                {
                    InfoCliente auxCliente = new InfoCliente();
                    auxCliente.clienteEndPoint = new IPEndPoint(IPAddress.Any, _puerto);
                    if (!bindeado)
                    {
                        _udpClient.Client.Bind(auxCliente.clienteEndPoint);
                        bindeado = true;
                    }

                    //ACÁ LLEGA TODO SIEMPRE Y AHI ES DONDE TENGO QUE VER QUE HACER
                    var datosEntrada = _udpClient.Receive(ref auxCliente.clienteEndPoint); 

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

                        
                        ip_Conexion = _lstClientesUDP[indiceMsg].clienteEndPoint.Address.ToString();

                        if (_lstClientesUDP[indiceMsg].primerMensaje)
                        {
                            _lstClientesUDP[indiceMsg].primerMensaje = false;
                            cantidadConexiones++;
                            _lstClientesUDP[indiceMsg].numConexion = cantidadConexiones;

                            Parametrosvento aceptarCon = new Parametrosvento();
                            aceptarCon.SetEvento(Parametrosvento.TipoEvento.ACEPTAR_CONEXION)
                                .SetIpOrigen(ip_Conexion)
                                .SetIndiceLista(indiceMsg)
                                .SetNumConexion(_lstClientesUDP[indiceMsg].numConexion);

                            GenerarEvento(aceptarCon);

                            //mantengo esto para que sea el orden de eventos tal como es en tcp
                            //tendría que agregar algo que permita rechazar la conexion dsp de que se dispara ACEPTAR_CONEXION

                            Parametrosvento nuevaCon = new Parametrosvento();
                            nuevaCon.SetEvento(Parametrosvento.TipoEvento.NUEVA_CONEXION)
                            .SetIpOrigen(ip_Conexion)
                            .SetIndiceLista(indiceMsg)
                            .SetNumConexion(_lstClientesUDP[indiceMsg].numConexion);

                            GenerarEvento(nuevaCon);

                            _conectado = true;
                            //_udpClient.Close(); //para pruebas

                        }

                        Parametrosvento ev = new Parametrosvento();
                        ev.SetDatos(Encoding.ASCII.GetString(datosEntrada, 0, datosEntrada.Length))
                            .SetIpOrigen(ip_Conexion)
                            .SetEvento(Parametrosvento.TipoEvento.DATOS_IN)
                            .SetIndiceLista(indiceMsg)
                            .SetNumConexion(_lstClientesUDP[indiceMsg].numConexion);
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
