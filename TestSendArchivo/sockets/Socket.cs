using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sockets
{
    public class Socket
    {
        //variables y objectos publicos
        //public Cliente cliente;
        //public Servidor servidor;


        //variables y objetos privados
        private Cliente _objCliente;
        private Servidor _objServidor;

        private string _mensaje;

        private bool _modoServidor;
        private bool _modoCliente;


        private int _puertoEscuchaServer;
        private int _codePage;
        private int _indice;
        private bool _escuchando;
        private int _puertoCliente;
        private long _size;
        private string _ipCliente;
        private string _datos;
        private string _host;
        private bool _tcp;

        //constantes priavdas
        private const string C_MENSAJE_ERROR_MODO_SOY_CLIENTE       = "modo cliente";
        private const string C_MENSAJE_ERROR_MODO_SOY_SERVER        = "modo server";


        public const int C_EVENTO_ERROR                           = -1;

        //constantes publicas
        public const int C_SERVER_EVENTO_ERROR                    = 0;
        public const int C_SERVER_EVENTO_ENVIO_COMPLETO           = 1;
        public const int C_SERVER_EVENTO_ENVIO_ARRAY              = 2;
        public const int C_SERVER_EVENTO_DATOS_IN                 = 3;
        public const int C_SERVER_EVENTO_NUEVA_CONEXION           = 4;
        public const int C_SERVER_EVENTO_CONEXION_FIN             = 5;
        public const int C_SERVER_EVENTO_TIME_OUT                 = 6;
        public const int C_SERVER_EVENTO_ACEPTAR_CONEXION         = 7;
        public const int C_SERVER_EVENTO_ESPERA_CONEXION          = 8;

        public const int C_CLIENTE_EVENTO_CONEXION_FIN            = 9;
        public const int C_CLIENTE_EVENTO_CONEXION_OK             = 10;
        public const int C_CLIENTE_EVENTO_DATOS_IN                = 11;
        public const int C_CLIENTE_EVENTO_ENVIO_COMPLETO          = 12;
        public const int C_CLIENTE_EVENTO_POS_ENVIO               = 13;
        public const int C_CLIENTE_EVENTO_TIME_OUT                = 14;
        public const int C_CLIENTE_EVENTO_ERROR                   = 15;

        /*
            Ev_Cliente_EnvioCompleto);
            Ev_Cliente_Error);
            Ev_Cliente_PosEnvio);
            Ev_Cliente_Timeout);
         */

        //private object ev_aceptar_conexion;

        /*
        public delegate void Delegado_Socket_Event(int Indice, int Evento, bool Escuchando, long Size, string Datos,long posicion, string IpOrigen);
        public event Delegado_Socket_Event Event_Socket;
        public void EventSocket(int indice, int evento, bool escuchando, long size, string datos, long posicion, string ipOrigen)
        {
            //this.Eve_Socket_Servidor(Indice, Evento, Escuchando, Size, Datos, IpOrigen);
            this.Event_Socket(Indice, evento, escuchando, size, datos, posicion, ipOrigen);
        }
        */
        public delegate void Delegado_Socket_Event(Parametrosvento parametros);
        public event Delegado_Socket_Event Event_Socket;
        public void EventSocket(Parametrosvento parametros)
        {
            //this.Eve_Socket_Servidor(Indice, Evento, Escuchando, Size, Datos, IpOrigen);
            this.Event_Socket(parametros);
        }


        public bool tcp
        {
            get
            {
                return _tcp;
            }
        }

        public string ipClienteConectado
        {
            get
            {
                string res = "";
                if (_modoServidor)
                {
                    res = _objServidor.ip_Conexion;
                }
                return res;
            }
        }

        public Socket()
        {
            
        }

        void Error(string mensajeError)
        {
            //EventSocket(_indice, C_EVENTO_ERROR, _escuchando, _size, mensajeError, 0, _ipCliente);
            Parametrosvento ev = new Parametrosvento();

            ev.SetDatos(mensajeError)
                .SetEvento(Parametrosvento.TipoEvento.ERROR)
                .SetIndice(_indice).SetEscuchando(_escuchando)
                .SetSize(_size)
                .SetIpOrigen(_ipCliente);

            EventSocket(ev);
        }

        public void SetCliente()
        {
            SetCliente(_puertoCliente, _codePage, _indice, _host);
        }


        /// <summary>
        /// setea el cliente 
        /// </summary>
        /// <param name="puerto"></param>
        /// <param name="codePage"></param>
        /// <param name="indice"></param>
        /// <param name="host"></param>
        /// <param name="timeOut"></param>
        public void SetCliente(int puerto, int codePage, int indice, string host,int timeOut = 30)
        {
            string res = "";
            _puertoCliente = puerto;
            _codePage = codePage;
            _indice = indice;
            _host = host;

            _objCliente = new Cliente();
            _objCliente.SetGetTimeOut = timeOut;
            _objCliente.CodePage(_codePage, ref res);
            if (res != "")
            {
                Error(res);
            }

            _objCliente.Eve_Conexion_Fin += new Cliente.DelegadoConexion_Fin(Ev_Cliente_conexion_Fin);
            _objCliente.Eve_Conexion_OK += new Cliente.DelegadoConexion_ok(Ev_Cliente_conexion_ok);
            _objCliente.Eve_DatosIn += new Cliente.DelegadoDatosIN(Ev_Cliente_DatosIn);
            _objCliente.Eve_EnvioCompleto += new Cliente.DelegadoEnvioCompleto(Ev_Cliente_EnvioCompleto);
            _objCliente.Eve_Error += new Cliente.DelegadoError(Ev_Cliente_Error);
            _objCliente.Eve_Posicion_Envio += new Cliente.Delegado_posicion_Envio(Ev_Cliente_PosEnvio);
            _objCliente.Eve_TimeOut += new Cliente.DelegadoTimeOut(Ev_Cliente_Timeout);

        }

        public void Conectar()
        {
            Conectar(_host, _puertoCliente);
        }
        public void Conectar(string host, int puerto)
        {
            string res ="";

            _host = host;
            _puertoCliente = puerto;
            
            _objCliente.Conectar(_indice, _host, _puertoCliente, ref res);
            if (res != "")
            {
                Error(res);
            }
        }

        public void SetServer()
        {
            SetServer(_puertoEscuchaServer, _codePage, _indice);
        }
        

        public void SetServer(int puerto,int codePage,int indice=0,bool tcp =true)
        {
            string res = "";
            _escuchando = false;
            _puertoEscuchaServer = puerto;
            _codePage = codePage;
            _indice = indice;
            _tcp = tcp;
            this.ModoServidor = true;
            //_modoServidor = true;


            if (!ModoServidor) //ya esta activado de antes el modo cliente
            {
                //EventSocket(indice, C_EVENTO_ERROR,false,0,"",0,"");
                return;
            }

            _objServidor = new Servidor(_puertoEscuchaServer, _codePage, _indice, ref res, _tcp);
            _objServidor.evento_servidor += new Servidor.Delegado_Servidor_Event(ev_server);

            //_objServidor.Eve_AceptarConexion += new Servidor.DelegadoAceptarConexion(Ev_Server_AceptarConexion);
            //_objServidor.Eve_DatosIn += new Servidor.DelegadoDatosIn(Ev_Server_DatosIn);
            //_objServidor.Eve_Envio_Completo += new Servidor.Delegado_Envio_Completo(Ev_Server_EnvioCompleto);
            //_objServidor.Eve_Error += new Servidor.DelegadoError(Ev_Server_Error);
            //_objServidor.Eve_Espera_Conexion += new Servidor.DelegadoEsperaConexion(Ev_Server_EsperaConexion);
            //_objServidor.Eve_FinConexion += new Servidor.DelegadoFinConexion(Ev_Server_FinConexion);
            //_objServidor.Eve_NuevaConexion += new Servidor.DelegadoNuevaConexion(Ev_Server_NuevaConexion);
            //_objServidor.Eve_Posicion_Envio += new Servidor.Delegado_posicion_Envio(Ev_Server_PosicionEnvio);
            
            if (res != "")
            {
                _mensaje = res;
                
                return;
            }
            _escuchando = true;

        }


        public void StartServer()
        {
            string res="";

            _objServidor.Iniciar(ref res);
            if (res != "")
            {
                Error(res);
                return;
            }
            
        }

        #region propiedades
        public bool ModoServidor
        {
            get
            {
                return _modoServidor;
            }
            set
            {
                /*
                if (value)
                {
                    if (_modoCliente)
                    {
                        
                        _modoServidor = false;
                        Error(C_MENSAJE_ERROR_MODO_SOY_CLIENTE);
                    }
                    else
                    {
                        _modoServidor = value;
                    }
                }
                else
                {
                    _modoServidor = value;
                }*/
                _modoServidor = value;
                 
            }
        }

        public bool ModoCliente
        {
            get
            {
                return _modoCliente;
            }
            set
            {
                /*
                if (value)
                {
                    if (_modoServidor)
                    {
                        _modoCliente = false;
                        Error(C_MENSAJE_ERROR_MODO_SOY_SERVER);
                    }
                }
                else
                {
                    _modoCliente = value;
                }*/
                _modoCliente = value;
            }
        }

        public int PuertoEscuchaServer
        {
            get
            {
                return _puertoEscuchaServer;
            }
            set
            {
                _puertoEscuchaServer = value;
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

        public int Indice
        {
            get
            {
                return _indice;
            }
            set
            {
                _indice = value;
            }
        }

        public string Mensaje
        {
            get
            {
                return _mensaje;
            }
        }

        public void Desconectar()
        {
            string res="";

            if (ModoServidor)
            {
                _objServidor.Detener(ref res);
            }
            if (ModoCliente)
            {
                _objCliente.Cerrar_Conexion();
            }

            if (res != "")
            {
                Error(res);
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

        public bool ClienteConectado
        {
            get
            {
                if (_objCliente != null)
                {
                    return _objCliente.conectado;
                }
                return false;
            }
        }


        #endregion

        #region eventos modo server
        private void ev_server(Parametrosvento ev)
        {
            if ((_ipCliente !=""))
            {
                if (_ipCliente != ev.GetIpOrigen)
                {
                    _ipCliente = ev.GetIpOrigen;
                }
            }
            EventSocket(ev);


        }
        /*
        private void Ev_Server_AceptarConexion(int Indice, string IpOrigen)
        {
            _ipCliente = IpOrigen;
            EventSocket(Indice, C_SERVER_EVENTO_ACEPTAR_CONEXION, _escuchando, _size, _datos, 0, _ipCliente);
        }

        private void Ev_Server_DatosIn(int indice, string Datos,string ipOrigen)
        {
            EventSocket(indice, C_SERVER_EVENTO_DATOS_IN, _escuchando, _size, Datos, 0, ipOrigen);
        }
        private void Ev_Server_PosicionEnvio(int Indice, long pos)
        {
            Event_Socket(Indice, C_SERVER_EVENTO_ENVIO_ARRAY, _escuchando, 0, _datos, pos, _ipCliente);
        }

        private void Ev_Server_NuevaConexion(int Indice,string ipOrigen)
        {
            Event_Socket(Indice, C_SERVER_EVENTO_NUEVA_CONEXION, _escuchando, 0,_datos, 0, ipOrigen);
        }

        private void Ev_Server_FinConexion(int Indice)
        {
            Event_Socket(Indice, C_SERVER_EVENTO_CONEXION_FIN, _escuchando, 0, _datos, 0, _ipCliente);
        }

        private void Ev_Server_EsperaConexion(int Indice, bool Escuchando)
        {
            Event_Socket(Indice, C_SERVER_EVENTO_ESPERA_CONEXION, _escuchando,0, _datos, 0, _ipCliente);
        }

        private void Ev_Server_Error(int indice, string ErrDescrip)
        {
            Event_Socket(indice, C_EVENTO_ERROR, _escuchando, 0, ErrDescrip, 0, _ipCliente);
        }

        private void Ev_Server_EnvioCompleto(int Indice, long Size)
        {
            Event_Socket(Indice, C_SERVER_EVENTO_ENVIO_COMPLETO, _escuchando, 0, _datos, 0, _ipCliente);
        }*/


        #endregion

        #region eventos modo cliente
        private void Ev_Cliente_Timeout(int Indice)
        {
            //EventSocket(Indice, C_CLIENTE_EVENTO_TIME_OUT, _escuchando, _size, _datos, 0, "");
        }

        private void Ev_Cliente_PosEnvio(int Indice, long pos)
        {
            //EventSocket(Indice, C_CLIENTE_EVENTO_POS_ENVIO, _escuchando, _size, "", pos, "");
        }

        private void Ev_Cliente_Error(int Indice, string Mensaje)
        {
            //EventSocket(Indice, C_CLIENTE_EVENTO_ERROR, _escuchando, 0, Mensaje, 0, "");
        }

        private void Ev_Cliente_EnvioCompleto(int Indice, long DatosSend)
        {
            //EventSocket(Indice, C_CLIENTE_EVENTO_ENVIO_COMPLETO, _escuchando, DatosSend,"",0, "");
        }

        private void Ev_Cliente_DatosIn(int Indice, string Mensaje)
        {
            //EventSocket(Indice, C_CLIENTE_EVENTO_DATOS_IN, _escuchando, 0, Mensaje, 0, "");
        }

        private void Ev_Cliente_conexion_ok(int Indice)
        {
            //EventSocket(Indice, C_CLIENTE_EVENTO_CONEXION_OK, _escuchando, 0, "", 0, "");
        }

        private void Ev_Cliente_conexion_Fin(int Indice)
        {
            //EventSocket(Indice, C_CLIENTE_EVENTO_CONEXION_FIN, _escuchando, 0, "", 0, "");
        }
        #endregion


        /// <summary>
        /// envía un mensaje al cliente o al servidor. si hay un error se dispara un evento de error
        /// </summary>
        /// <param name="mensaje">mensaje a enviar</param>
        public void Enviar(string mensaje)
        {

            string res = "";

            if (_modoServidor)
            {
                _objServidor.Enviar(mensaje, ref res);
            }

            if (_modoCliente)
            {
                _objCliente.Enviar(mensaje, ref res);
            }

            if (res != "")
            {
                Error(res);
            }
        }

        public void EnviarArray(byte[] memArray, int TamCluster)
        {
            string res = "";

            if (_modoServidor)
            {
                _objServidor.Enviar_ByteArray(memArray, TamCluster, ref res);
            }

            if (_modoCliente)
            {
                _objCliente.Enviar_ByteArray(memArray, TamCluster, ref res);
            }

            if (res != "")
            {
                Error(res);
            }

        }
    }
}
