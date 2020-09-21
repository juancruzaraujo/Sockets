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
    internal class ServidorTCP
    {
        #if DEBUG
            private /*static*/ bool modo_Debug = true;
        #else
            private /*static*/ bool modo_Debug = false;
        #endif

        private Thread _thrCliente;
        private Thread _thrClienteConexion;

        private TcpListener _tcpListen;
        private TcpClient _tcpCliente;
        private Encoding _encoder;

        private int _indiceCon; //va a contener el indice de conexion
        private int _indiceLista; //va a conetener el indice de la lista de sockets
        private bool _bucleCliComunucacion;
        private bool _escuchar;

        private bool _conectado;
        internal bool EsperandoConexion;
        internal string ip_Conexion;
        internal int puerto;

        internal delegate void Delegado_Servidor_Event(Parametrosvento servidorParametrosEvento);
        internal event Delegado_Servidor_Event evento_servidor;
        private void Evento_Servidor(Parametrosvento servidorParametrosEvento)
        {
            this.evento_servidor(servidorParametrosEvento);
        }
        
        internal bool Escuchar
        {
            get
            {
                return _escuchar;
            }
            set
            {
                _escuchar = false;
            }
        }

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

        internal bool Conectado
        {
            get
            {
                return _conectado;
            }
        }

        /// <summary>
        /// setea el socket para la escucha
        /// </summary>
        /// <param name="PuertoEscucha">puerto de escucha</param>
        /// <param name="Cod">codopage</param>
        /// <param name="Mensaje">mensaje que retorna en caso de error</param>
        /// <param name="tcp">define si la conexión es tpc o udp, vaor default true, si es falso, la conexion es udp</param>
        internal ServidorTCP(int PuertoEscucha, int Cod, ref string mensaje)
        {
            _indiceCon = -1;
            try
            {
                CodePage(Cod, ref mensaje);
                if (mensaje != "")
                {
                    mensaje = mensaje + " No se pudo iniciar ConServer";
                    return;
                }

                puerto = PuertoEscucha;
            }
            catch (Exception err)
            {
                mensaje = err.Message;
                GenerarEventoError(err);
            }
        }

        /// <summary>
        /// Inicia el sokect en escucha
        /// </summary>
        /// <param name="Mensaje">Mensaje que retorna en caso de error</param>
        internal void Iniciar(ref string Mensaje)
        {
            try
            {
                ThreadStart cliente;
                cliente = new ThreadStart(EscucharTCP);
                
                _thrCliente = new Thread(cliente);
                _thrCliente.Name = "ThTCP";
                _thrCliente.IsBackground = true;
                _thrCliente.Start();

            }
            catch (Exception err)
            {
                EsperandoConexion = false;

                Mensaje = err.Message;
                GenerarEventoError(err);
            }
        }

        /*internal void Detener()
        {
            _bucleCliComunucacion = false;
        }*/

        /// <summary>
        /// Detiene todas las conexiones
        /// </summary>
        /// <param name="Mensaje">Mensaje que retorna en caso de error</param>
        internal void DesconectarCliente(bool deteniendoServer = false)
        {
            bool forzarDesconexion;

            if (deteniendoServer)
            {
                forzarDesconexion = true;
            }
            else
            {
                forzarDesconexion = Conectado;
            }

            
            /*if (!forzarDesconexion) //SI ESTÀ ESTO, NO DETIENE EL SERVER ni reinicia
            {
                EsperandoConexion = false;
                Parametrosvento ev = new Parametrosvento();
                ev.SetEscuchando(EsperandoConexion).SetEvento(Parametrosvento.TipoEvento.ESPERA_CONEXION);
                GenerarEvento(ev);
            }*/

            try
            {
                if (_thrCliente != null)
                {
                    if (forzarDesconexion)
                    {
                        _tcpListen.Stop();
                        if (_tcpCliente != null)
                        {
                            _tcpCliente.Close();
                        }
                        else
                        {
                            Parametrosvento ev = new Parametrosvento();
                            ev.SetEscuchando(EsperandoConexion).SetEvento(Parametrosvento.TipoEvento.SERVER_DETENIDO);
                            GenerarEvento(ev);
                        }
                        _thrCliente.Abort();
                        _bucleCliComunucacion = false;
                        //_thrClienteConexion.Abort();
                        Thread.EndCriticalRegion(); //esto cierra todo con o sin conexiones
                    }
                }
            }
            catch (Exception err)
            {
                if (modo_Debug == true)
                {
                    GenerarEventoError(err);
                }
            }
        }

        private void EscucharTCP()
        {
            //bool _escuchar;

            TcpClient Cliente = new TcpClient();

            _tcpListen = new TcpListener(IPAddress.Any, puerto);
            
            try
            {
                EsperandoConexion = true;
                Parametrosvento ev = new Parametrosvento();
                ev.SetEvento(Parametrosvento.TipoEvento.ESPERA_CONEXION).SetEscuchando(true);
                GenerarEvento(ev);

                _tcpListen.Stop();
                _tcpListen.Start();
                _escuchar = true;
            }
            catch (Exception err)
            {
                //el error salta acá, porque ya abrí una nueva instancia que esta eschando acá.
                EsperandoConexion = false;
                Parametrosvento evErr = new Parametrosvento();
                evErr.SetEvento(Parametrosvento.TipoEvento.ESPERA_CONEXION);
                GenerarEvento(evErr); //ver porque puse dos eventos
                GenerarEventoError(err);
                return;
            }

            do
            {
                try
                {
                    string sAux;

                    //Escuchando = true;
                    Cliente = _tcpListen.AcceptTcpClient();
                    sAux = ((System.Net.IPEndPoint)(Cliente.Client.RemoteEndPoint)).Address.ToString();

                    EsperandoConexion = false;
                    Parametrosvento ev = new Parametrosvento();
                    ev.SetEvento(Parametrosvento.TipoEvento.ESPERA_CONEXION);
                    GenerarEvento(ev);

                    try
                    {
                        _bucleCliComunucacion = true;
                        _thrClienteConexion = new Thread(new ParameterizedThreadStart(Cliente_Comunicacion));
                        _thrClienteConexion.Name = "ThrCliente";
                        _thrClienteConexion.IsBackground = true;
                        _thrClienteConexion.Start(Cliente);

                        ev.SetEvento(Parametrosvento.TipoEvento.ACEPTAR_CONEXION).SetIpOrigen(sAux);
                        GenerarEvento(ev);

                        //ahora tendria que dejar de escuchar
                        _escuchar = false;
                        _tcpListen.Stop();
                    }
                    catch (Exception err)
                    {
                        _escuchar = false;
                        //ev.SetDatos(err.Message + " threadConexion").SetEvento(Parametrosvento.TipoEvento.ERROR);
                        //GenerarEvento(ev);
                        GenerarEventoError(err, "threadConexion");

                    }
                }
                catch (Exception err)
                {
                    _escuchar = false;
                    _tcpListen.Stop();
                    GenerarEventoError(err, "TcpListen.Accept()");
                    
                    //_thrClienteConexion.Abort();
                }

            } while (_escuchar == true);//fin do
        }

        private void Cliente_Comunicacion(object Cliente)
        {

            bool EveYaDisparado = false;

            try
            {
                _tcpCliente = (TcpClient)Cliente;
                NetworkStream clientStream = _tcpCliente.GetStream();
                string strDatos;

                _tcpCliente = (TcpClient)Cliente;

                //levanto evento nueva conexion
                _conectado = true;
                Parametrosvento ev = new Parametrosvento();
                ev.SetIpOrigen(_tcpCliente.Client.RemoteEndPoint.ToString()).SetEvento(Parametrosvento.TipoEvento.NUEVA_CONEXION);
                GenerarEvento(ev);

                byte[] message = new byte[65535];

                int bytesRead;

                while (_bucleCliComunucacion)
                {
                    bytesRead = 0;

                    try
                    {
                        bytesRead = clientStream.Read(message, 0, 65535);
                    }
                    catch 
                    {
                        EveYaDisparado = true;
                        _conectado = false;
                        _tcpCliente.Close();
                        
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        //el cliente se desconecto!
                        _conectado = false;
                        _tcpCliente.Close();
                        ev.SetDatos("").SetEvento(Parametrosvento.TipoEvento.CONEXION_FIN);
                        //EveYaDisparado = true;
                        break;
                    }
                    
                    //llegó el mensaje
                    strDatos = _encoder.GetString(message, 0, bytesRead);
                    ev.SetDatos(strDatos)
                        .SetIpOrigen(_tcpCliente.Client.RemoteEndPoint.ToString())
                        .SetEvento(Parametrosvento.TipoEvento.DATOS_IN)
                        .SetSize(strDatos.Length);
                    GenerarEvento(ev);
                }

                if (!EveYaDisparado)
                {
                    //el cliente cerro la conexion
                    _conectado = false;
                    _tcpCliente.Close();
                }

                ev.SetEvento(Parametrosvento.TipoEvento.CONEXION_FIN).SetDatos("");
                GenerarEvento(ev);
                
            }
            catch (Exception err)
            {
                GenerarEventoError(err);
            }
        }

        /// <summary>
        /// Envia un mensaje al cliente conectado
        /// </summary>
        /// <param name="Indice">Indice de conexion al que se el envia el mensaje</param>
        /// <param name="Datos">el mensaje a enviar</param>
        /// <param name="Resultado">Mensaje que retorna en caso de error</param>
        //internal void Enviar(int Indice,string Datos, ref string Resultado)
        internal void Enviar(string datos)
        {
            int buf=0;
            try
            {
                TcpClient TcpClienteDatos = _tcpCliente;
                NetworkStream clienteStream = TcpClienteDatos.GetStream();

                byte[] buffer = _encoder.GetBytes(datos);
                buf = buffer.Length;
                clienteStream.Write(buffer, 0, buf);
                clienteStream.Flush(); //envio los datos
                
                Parametrosvento ev = new Parametrosvento();
                ev.SetSize(buf).SetEvento(Parametrosvento.TipoEvento.ENVIO_COMPLETO);
                GenerarEvento(ev);

            }
            catch (Exception err)
            {
                GenerarEventoError(err);
            }
        }

        /*
        internal void Enviar_ByteArray(byte[] memArray, int TamCluster)
        {
            string datos = "";
            int nPosActual = 0;
            int nTam;
            int nResultado = 0;
            int nPosLectura = 0;
            int nCondicion;

            nTam = memArray.Length;

            if (nTam <= TamCluster)
            {
                TamCluster = nTam; //sí es mas chico lo que mando que el cluster
            }

            try
            {

                //TcpClient TcpClienteDatos = _tcpCliente;
                //NetworkStream clientStream = TcpClienteDatos.GetStream();

                while (nPosActual < nTam - 1) //quizas aca me falte un byte (-1)
                {
                    nCondicion = nPosActual + TamCluster;
                    for (int I = nPosActual; I <= nCondicion - 1; I++)
                    {
                        //meto todo al string para manadar
                        datos = datos + Convert.ToChar(memArray[I]);
                        nPosLectura++;
                    }

                    //me re acomodo en el array
                    nResultado = nTam - nPosLectura;
                    if (nResultado <= TamCluster)
                    {
                        TamCluster = nResultado; //ya estoy en el final y achico el cluster
                    }
                    else
                    {
                        //por ahora no hago nada
                    }

                    nPosActual = nPosLectura;
                    Parametrosvento ev = new Parametrosvento();
                    ev.SetPosicion(nPosActual).SetEvento(Parametrosvento.TipoEvento.POSICION_ENVIO);
                    GenerarEvento(ev);
                    //ver que no me quede uno atras

                    //envio los datos
                    //byte[] buffer = _encoder.GetBytes(Datos);

                    //clientStream.Write(buffer, 0, buffer.Length);
                    //clientStream.Flush(); //envio lo datos
                    Enviar(datos);

                    ev.SetEvento(Parametrosvento.TipoEvento.ENVIO_COMPLETO).SetPosicion(datos.Length);
                    GenerarEvento(ev);

                    //string res = "";

                    Thread.Sleep(5);

                    datos = ""; //limpio la cadena
                }//fin while

            }
            catch (Exception err)
            {
                GenerarEventoError(err);
            }

        }
        */
        /// <summary>
        /// Code page para iniciar la comunicacion
        /// </summary>
        /// <param name="Codigo">codigo de codepage</param>
        /// <param name="Error">Mensaje que retorna en caso de error</param>
        internal void CodePage(int Codigo, ref string error)
        {
            try
            {
                _encoder = Encoding.GetEncoding(Codigo);
            }
            catch (Exception err)
            {
                error = err.Message;
                GenerarEventoError(err);
            }
        }

        private void GenerarEvento(Parametrosvento ob)
        {
            ob.SetNumConexion(_indiceCon).SetIndiceLista(_indiceLista);
            Evento_Servidor(ob);
        }

        private void GenerarEventoError(Exception err,string mensajeOpcional="")
        {
            if (err.HResult != -2146233040)
            {
                //-2146233040
                //-2146233040
                //-2146233040

                Utils utils = new Utils();
                Parametrosvento ev = new Parametrosvento();
                if (mensajeOpcional != "")
                {
                    mensajeOpcional = " " + mensajeOpcional;
                }
                ev.SetEscuchando(EsperandoConexion).
                    SetDatos(err.Message + mensajeOpcional).
                    SetEvento(Parametrosvento.TipoEvento.ERROR).
                    SetCodError(utils.GetCodigoError(err)).
                    SetLineNumberError(utils.GetNumeroDeLineaError(err));
                GenerarEvento(ev);
            }
        }

    }
}
