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

        //private static Socket _serverSocketUDP;
        //private IPEndPoint _serverUDP;

        private static IPEndPoint _remoteEP;

        //lo viejo
        private Thread _thrCliente;
        private UdpClient _udpClient;
        private List<UdpClient> _lstUDPClient;
        //private IPEndPoint _remoteEP;
        private int _indiceCon; //va a contener el indice de conexion
        private int _indiceLista; //va a conetener el indice de la lista de sockets
        ///si es el primer mensaje, es una conexión nueva y tengo que hacer saltar el evento de nueva conexión
        private bool _primerMensajeCliUDP;
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
            try
            {
                if (_remoteEP == null)
                {
                    _remoteEP = new IPEndPoint(IPAddress.Any, _puerto);
                }
                //_udpClient = new UdpClient(puerto);

                //if (_remoteEP.Port) //aca quede

                #region pruebas
                _udpClient = new UdpClient();
                _udpClient.ExclusiveAddressUse = false;

                _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpClient.Client.Bind(_remoteEP);
                #endregion

                while (true)
                {
                    var datos = _udpClient.Receive(ref _remoteEP);
                    ip_Conexion = _remoteEP.Address.ToString();

                    if (!_primerMensajeCliUDP)
                    {
                        _primerMensajeCliUDP = true;
                        Parametrosvento aceptarCon = new Parametrosvento();
                        aceptarCon.SetEvento(Parametrosvento.TipoEvento.ACEPTAR_CONEXION).SetIpOrigen(ip_Conexion);
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
                    ev.SetDatos(Encoding.ASCII.GetString(datos, 0, datos.Length)).SetIpOrigen(ip_Conexion).SetEvento(Parametrosvento.TipoEvento.DATOS_IN);
                    GenerarEvento(ev);
                }
            }
            catch (Exception e)
            {
                _conectado = false;
                _primerMensajeCliUDP = false;
                _udpClient.Close();
                GenerarEventoError(e);

            }
        }

        private void GenerarEvento(Parametrosvento ob)
        {
            ob.SetNumConexion(_indiceCon).SetIndiceLista(_indiceLista);
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
