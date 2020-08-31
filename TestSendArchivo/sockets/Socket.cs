using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sockets
{
    public class Sockets
    {
        
        //variables y objetos privados
        private ClienteTCP _objCliente;
        private List<ServidorTCP> _lstObjServidorTCP;
        //private List<ServidorUDP> _lstObjServidorUDP;
        private ServidorUDP _objServidorUDP;

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
        private bool _deteniendoServer;
        private bool _serverIniciado;

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

        public int MaxServerConexiones
        {
            set
            {
                _maxServCon = value;
                if (_modoServidor)
                {
                    if (!_tcp)
                    {
                        _objServidorUDP.MaxClientesUDP = _maxServCon;
                    }
                }
            }
            get
            {
                return _maxServCon;
            }
        }

        public Sockets()
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

            _objCliente = new ClienteTCP(tcp);
            _objCliente.SetGetTimeOut = timeOut;
            _objCliente.CodePage(_codePage, ref res);
            if (res != "")
            {
                Error(res);
                return;
            }
            _objCliente.evento_cliente += new ClienteTCP.Delegado_Cliente_Event(Evsocket);
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
            _tcp = tcp;
            _maxServCon = maxCon;
            _cantConServidor = 0;
            _numCliConServidor = 1;
            this.ModoServidor = true;


            if (!ModoServidor) //ya esta activado de antes el modo cliente
            {
                Error(ModoServidor.ToString());
                return;
            }
            if (_tcp)
            {
                _lstObjServidorTCP = new List<ServidorTCP>();
            }
            else
            {
                
            }

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
            int indiceLista = GetUltimoEspacioLibre();

            if (_tcp)
            {
                ServidorTCP objServidor = new ServidorTCP(_puertoEscuchaServer, _codePage, ref mensaje);
                if (mensaje != "")
                {
                    Error(mensaje);
                    return;
                }
                objServidor.evento_servidor += new ServidorTCP.Delegado_Servidor_Event(Evsocket);
                _lstObjServidorTCP.Add(objServidor);
                
                _lstObjServidorTCP[indiceLista].IndiceConexion = _numCliConServidor;
                _lstObjServidorTCP[indiceLista].IndiceLista = indiceLista;
            }
            else
            {
                _objServidorUDP = new ServidorUDP(_puertoEscuchaServer);
                _objServidorUDP.MaxClientesUDP = _maxServCon;
                if (mensaje != "")
                {
                    Error(mensaje);
                    return;
                }
                _objServidorUDP.evento_servidor += new ServidorUDP.Delegado_Servidor_Event(Evsocket);

            }
            
        }

        
        public void StartServer()
        {
            string res="";
            if (_tcp)
            {
                _lstObjServidorTCP[_lstObjServidorTCP.Count() -1].Iniciar(ref res);

                if (res != "")
                {
                    Error(res);
                    return;
                }
                
                _serverEscuchando = true;
            }
            else
            {
                _objServidorUDP.Iniciar();
            }

            if (!_serverIniciado)
            {
                _serverIniciado = true;
                Parametrosvento ev = new Parametrosvento();
                ev.SetEvento(Parametrosvento.TipoEvento.SERVER_INICIADO);
                Evsocket(ev);
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

        public void Desconectarme()
        {
            if (ModoCliente)
            {
                _objCliente.Cerrar_Conexion();
            }
        }

        public void DesconectarCliente(int numConexion)
        {
            string res = "";
            if (ModoServidor)
            {
                if (_tcp)
                {
                    int indiceClienteDesconectar = GetIndiceListaClienteConectado(numConexion);
                    if (indiceClienteDesconectar >= 0)
                    {
                        _lstObjServidorTCP[indiceClienteDesconectar].DesconectarCliente();
                        ReacomodarListaClientes();
                        //_cantConServidor--;

                    }
                    if (res != "")
                    {
                        Error(res);
                    }
                }
                else
                {
                    _objServidorUDP.Desconectar(numConexion);
                }
            }
        }

        public void DesconectarTodosClientes()
        {
            if (_tcp)
            {

                //Console.WriteLine(_cantConServidor);

                //for (int i = 0; i < _lstObjServidorTCP.Count; i++)
                for (int i = _lstObjServidorTCP.Count() -1; i >= 0; i--)
                {
                    string res = "";

                    _lstObjServidorTCP[i].DesconectarCliente(_deteniendoServer);
                    ReacomodarListaClientes();
                    
                    if (res != "")
                    {
                        Error(res);
                    }
                }

                /*
                Parametrosvento ev = new Parametrosvento();
                ev.SetEvento(Parametrosvento.TipoEvento.SERVER_DETENIDO);
                EventSocket(ev);*/
            }
            else
            {
                _objServidorUDP.DesconectarTodos();
            }
        }

        public void StopServer()
        {
            _serverEscuchando = false;
            _deteniendoServer = true;
            if (tcp)
            {
                DesconectarTodosClientes();
                //_lstObjServidorTCP.Clear();
            }
            else
            {
                _objServidorUDP.DetenerServer();
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
            bool mostrarEvMaxConexiones = false;

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
                            //string aux = "";
                            //_objServidor.Iniciar(ref aux); //volvemos a iniciar el servidor udp
                        }
                        break;

                    case Parametrosvento.TipoEvento.NUEVA_CONEXION:
                        
                        if (!_tcp)
                        {
                            //
                        }
                        else
                        {
                            if (_cantConServidor >= (_maxServCon -1)) //muy cabeza, pero funciona
                            {
                                mostrarEvMaxConexiones = true; //genero el evento de limite de conexiones
                            }
                            _cantConServidor++;
                            _numCliConServidor++;
                            if (GetNumConexion() < _maxServCon)
                            {
                                CrearServidor(ref mensaje);
                                StartServer();
                            }
                        }
                        break;

                    case Parametrosvento.TipoEvento.CONEXION_FIN: //UDP no dispara este evento
                        _lstObjServidorTCP.RemoveAt(ev.GetIndiceLista);
                        ReacomodarListaClientes();
                        _cantConServidor--;

                        if (!_serverEscuchando && !_deteniendoServer)
                        {
                            CrearServidor(ref mensaje);
                            StartServer();
                        }

                        break;

                    case Parametrosvento.TipoEvento.SERVER_DETENIDO:
                        _deteniendoServer = false;
                        _serverIniciado = false;
                        _lstObjServidorTCP.Clear();
                        break;
                }
                
            }

            EventSocket(ev); //envío el evento a quien lo este consumiendo(?)

            //pongo esto acá ya que tengo que ser lo último que muestro
            if (mostrarEvMaxConexiones)
            {
                mostrarEvMaxConexiones = false;
                _serverEscuchando = false;
                Parametrosvento evMaxCon = new Parametrosvento();
                evMaxCon.SetEvento(Parametrosvento.TipoEvento.LIMITE_CONEXIONES);
                EventSocket(evMaxCon);
            }
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
                if (_tcp)
                {
                    if (_lstObjServidorTCP[indice].Conectado)
                    {
                        _lstObjServidorTCP[indice].Enviar(mensaje, ref res); //saczar ref res
                    }
                }
                else
                {
                    _objServidorUDP.Enviar(mensaje,indice);
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
                    if (_tcp)
                    {
                        for (int i = 0; i < _lstObjServidorTCP.Count(); i++)
                        {
                            Enviar(mensaje, i);
                        }
                    }
                    else
                    {
                        _objServidorUDP.EnviarATodos(mensaje);
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
                if (_tcp)
                {
                    for (int i = 0; i < _lstObjServidorTCP.Count(); i++)
                    {
                        EnviarArray(memArray, tamCluster, i);
                    }
                }
                else
                {
                    //UDP
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
                if (_tcp)
                {
                    _lstObjServidorTCP[indice].Enviar_ByteArray(memArray, tamCluster, ref res);
                }
                else
                {
                    //UDP
                }
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
            int res =0;
            if (_tcp)
            {
                res = _lstObjServidorTCP.Count();
            }
            else
            {
                //res = _lstObjServidorUDP.Count();
            }
            return res;
        }

        private void ReacomodarListaClientes()
        {
            try
            {
                if (_tcp)
                {
                    for (int i = 0; i < _lstObjServidorTCP.Count(); i++)
                    {
                        _lstObjServidorTCP[i].IndiceLista = i;
                    }
                }
                else
                {
                    //UDP
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
        private int GetIndiceListaClienteConectado(int numConexion)
        {
            try
            {
                if (_tcp)
                {
                    for (int i = 0; i < _lstObjServidorTCP.Count(); i++)
                    {
                        if (_lstObjServidorTCP[i].IndiceConexion == numConexion)
                        {
                            return i;
                        }
                    }
                }
                else
                {
                    //UDP
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
