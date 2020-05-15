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
        //private Servidor _objServidor;
        private List<Servidor> _lstObjServidor;


        private string _mensaje;

        private bool _modoServidor;
        private bool _modoCliente;


        private int _puertoEscuchaServer;
        private int _codePage;
        private int _numConexion;
        private bool _escuchando;
        private int _puertoCliente;
        private long _size;
        private string _ipCliente;
        private string _datos;
        private string _host;
        private bool _tcp;
        private int _maxServCon;
        private int _cantConServidor;
        private int _numCliConServidor;
        private bool _serverEscuchando;

        //constantes priavdas
        private const string C_MENSAJE_ERROR_MODO_SOY_CLIENTE       = "modo cliente";
        private const string C_MENSAJE_ERROR_MODO_SOY_SERVER        = "modo server";


        
        public delegate void Delegado_Socket_Event(Parametrosvento parametros);
        public event Delegado_Socket_Event Event_Socket;
        public void EventSocket(Parametrosvento parametros)
        {
            this.Event_Socket(parametros);
        }

        
        public int GetMaxServerConexiones
        {
            get
            {
                return _maxServCon;
            }
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
                    //res = _objServidor.ip_Conexion;
                }
                return res;
            }
        }

        public Socket()
        {
            
        }

        void Error(string mensajeError)
        {
            Parametrosvento ev = new Parametrosvento();

            ev.SetDatos(mensajeError)
                .SetEvento(Parametrosvento.TipoEvento.ERROR)
                .SetNumConexion(_numConexion).SetEscuchando(_escuchando)
                .SetSize(_size)
                .SetIpOrigen(_ipCliente);

            EventSocket(ev);
        }

        public void SetCliente()
        {
            SetCliente(_puertoCliente, _numConexion, _host);
        }


        /// <summary>
        /// setea el cliente 
        /// </summary>
        /// <param name="puerto"></param>
        /// <param name="codePage"></param>
        /// <param name="indice"></param>
        /// <param name="host"></param>
        /// <param name="timeOut"></param>
        public void SetCliente(int puerto, int numConexion, string host,int timeOut = 30,bool tcp = true, int codePage = 65001)
        {
            string res = "";
            _puertoCliente = puerto;
            _codePage = codePage;
            _numConexion = numConexion;
            _host = host;

            _objCliente = new Cliente(tcp);
            _objCliente.SetGetTimeOut = timeOut;
            _objCliente.CodePage(_codePage, ref res);
            if (res != "")
            {
                Error(res);
                return;
            }
            _objCliente.evento_cliente += new Cliente.Delegado_Cliente_Event(Evsocket);
            _tcp = tcp;
            _modoCliente = true;

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

            _objCliente.Conectar(_numConexion, _host, _puertoCliente, ref res);
            if (res != "")
            {
                Error(res);
            }
        }

        public void SetServer(int puerto, int codePage = 65001, bool tcp = true, int maxCon = 0)
        {
            string res = "";
            _escuchando = false;
            _puertoEscuchaServer = puerto;
            _codePage = codePage;
            //_indice = indice;
            _tcp = tcp;
            _maxServCon = maxCon;
            _cantConServidor = 1;
            _numCliConServidor = 1;
            this.ModoServidor = true;
            //_modoServidor = true;


            if (!ModoServidor) //ya esta activado de antes el modo cliente
            {
                Error(ModoServidor.ToString());
                return;
            }

            _lstObjServidor = new List<Servidor>();
            CrearServidor(ref res);
            
            if (res != "")
            {
                _mensaje = res;
                Error(res);
                return;
            }
            _escuchando = true;
            _modoServidor = true;
        }

        private void CrearServidor(ref string mensaje)
        {
            
            Servidor objServidor = new Servidor(_puertoEscuchaServer, _codePage, ref mensaje, _tcp);
            if(mensaje!="")
            {
                Error(mensaje);
                return;
            }
            objServidor.evento_servidor += new Servidor.Delegado_Servidor_Event(Evsocket);
            _lstObjServidor.Add(objServidor);

            int indiceLista = GetUltimoEspacioLibre();

            _lstObjServidor[indiceLista].IndiceConexion = _numCliConServidor;
            _lstObjServidor[indiceLista].IndiceLista = indiceLista;
            //_cantConServidor++;
            
        }

        public void StartServer()
        {
            string res="";

            //_objServidor.Iniciar(ref res);
            _lstObjServidor[GetUltimoEspacioLibre()].Iniciar(ref res);
            
            if (res != "")
            {
                Error(res);
                return;
            }
            Parametrosvento ev = new Parametrosvento();
            ev.SetEvento(Parametrosvento.TipoEvento.SERVER_INICIADO);
            Evsocket(ev);
            _serverEscuchando = true;
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

        public int NumConexion
        {
            get
            {
                return _numConexion;
            }
            set
            {
                _numConexion = value;
            }
        }

        public string Mensaje
        {
            get
            {
                return _mensaje;
            }
        }

        public int GetNumConexion()
        {
            return _cantConServidor;
        }

        public int GetUltimoNumeroClienteConectado
        {
            get
            {
                return _numCliConServidor;
            }
        }

        public void Desconectarme()
        {
            if (ModoCliente)
            {
                _objCliente.Cerrar_Conexion();
            }
        }

        public void DesconectarCliente(int indice)
        {
            string res = "";
            if (ModoServidor)
            {
                int indiceClienteDesconectar = GetIndiceListaClienteConectado(indice); 
                if (indiceClienteDesconectar >=0)
                {
                    _lstObjServidor[indiceClienteDesconectar].Detener(ref res);
                    ReacomodarListaClientes();
                    _cantConServidor--;
                    
                }

                if (res != "")
                {
                    Error(res);
                }
            }
        }

        public void DesconectarTodosClientes()
        {
            for (int i=0;i<_lstObjServidor.Count;i++)
            {
                string res = "";
                _lstObjServidor[i].Detener(ref res);
                _cantConServidor--;
                if (res!="")
                {
                    Error(res);
                }
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

        //todos los eventos del cliente y el servidor caen acá
        private void Evsocket(Parametrosvento ev)
        {
            //REVISAR ESTO
            string mensaje = "";

            if ((_ipCliente !=""))
            {
                if (_ipCliente != ev.GetIpOrigen)
                {
                    _ipCliente = ev.GetIpOrigen;
                }
            }

            if (_modoServidor)
            {
                switch(ev.GetEvento)
                {
                    case Parametrosvento.TipoEvento.ERROR:
                        if (!_tcp)
                        {
                            string aux = "";
                            //_objServidor.Iniciar(ref aux); //volvemos a iniciar el servidor udp
                        }
                        break;

                    case Parametrosvento.TipoEvento.NUEVA_CONEXION:
                        
                        //if (!_tcp)
                        {

                        //}
                        //else
                        //{
                            
                            if (_cantConServidor >= _maxServCon)
                            {
                                _serverEscuchando = false;
                                Parametrosvento evMaxCon = new Parametrosvento();
                                evMaxCon.SetEvento(Parametrosvento.TipoEvento.LIMITE_CONEXIONES);
                                EventSocket(evMaxCon);
                                
                            }

                            _cantConServidor++;
                            _numCliConServidor++;
                            if (GetNumConexion() <= _maxServCon)
                            {
                                CrearServidor(ref mensaje);
                                StartServer();
                            }
                        }
                        break;

                    case Parametrosvento.TipoEvento.CONEXION_FIN:
                        _lstObjServidor.RemoveAt(ev.GetIndiceLista);
                        ReacomodarListaClientes();
                        _cantConServidor--;
                        if (!_serverEscuchando)
                        {
                            CrearServidor(ref mensaje);
                            StartServer();
                        }


                        break;
                }
                
            }

            EventSocket(ev); //envío el evento a quien lo este consumiendo(?)
        }

        /// <summary>
        /// envía un mensaje al cliente o al servidor. si hay un error se dispara un evento de error
        /// </summary>
        /// <param name="mensaje">mensaje a enviar</param>
        public void Enviar(string mensaje,int indice=0)
        {

            string res = "";

            if (_modoServidor)
            {
                if (_lstObjServidor[indice].Conectado)
                {
                    _lstObjServidor[indice].Enviar(mensaje, ref res);
                }
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

        public void EnviarATodos(string mensaje)
        {
            try
            {
                if (_modoServidor)
                {
                    for (int i = 0; i < _lstObjServidor.Count(); i++)
                    {
                        Enviar(mensaje, i);
                    }
                }
                else
                {
                    Enviar(mensaje);
                }
            }
            catch(Exception err)
            {
                GenerarEventoError(err);
            }

        }

        public void EnviarArrayATodos(byte[] memArray,int tamCluster)
        {
            try
            {
                for (int i = 0; i < _lstObjServidor.Count(); i++)
                {
                    EnviarArray(memArray, tamCluster, i);
                }
            }
            catch(Exception err)
            {
                GenerarEventoError(err);
            }
        }

        public void EnviarArray(byte[] memArray, int tamCluster,int indice)
        {
            string res = "";

            if (_modoServidor)
            {
                _lstObjServidor[indice].Enviar_ByteArray(memArray, tamCluster, ref res);
            }

            if (_modoCliente)
            {
                _objCliente.Enviar_ByteArray(memArray, tamCluster, ref res);
            }

            if (res != "")
            {
                Error(res);
            }

        }

        private int GetUltimoEspacioLibre()
        {
            return _lstObjServidor.Count()-1;
        }

        private void ReacomodarListaClientes()
        {
            try
            {
                for (int i = 0; i < _lstObjServidor.Count(); i++)
                {
                    _lstObjServidor[i].IndiceLista = i;
                }
            }
            catch(Exception err)
            {
                GenerarEventoError(err);
            }

        }

        /// <summary>
        /// retorna el numero de indice de la lista
        /// </summary>
        /// <param name="indiceCliente">le paso el numero de indice de cliente</param>
        /// <returns></returns>
        private int GetIndiceListaClienteConectado(int indiceCliente)
        {
            try
            {
                for (int i = 0; i < _lstObjServidor.Count(); i++)
                {
                    if (_lstObjServidor[i].IndiceConexion == indiceCliente)
                    {
                        return i;
                    }
                }
            }
            catch(Exception err)
            {
                GenerarEventoError(err);
            }
            return -1;
        }

        private void GenerarEventoError(Exception err, string mensajeOpcional = "")
        {
            Utils utils = new Utils();
            Parametrosvento ev = new Parametrosvento();
            if (mensajeOpcional != "")
            {
                mensajeOpcional = " " + mensajeOpcional;
            }
            ev.SetDatos(err.Message + mensajeOpcional).
                SetEvento(Parametrosvento.TipoEvento.ERROR).
                SetCodError(utils.GetCodigoError(err));
            Evsocket(ev);
        }

    }
}
