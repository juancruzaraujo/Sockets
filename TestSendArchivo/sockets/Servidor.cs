using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace Sockets
{
    internal class Servidor
    {
        #if DEBUG
            private /*static*/ bool modo_Debug = true;
        #else
            private /*static*/ bool modo_Debug = false;
        #endif
        

        internal const int EVE_ERROR = 0;
        internal const int EVE_ENVIO_COMPLETO = 1;
        internal const int EVE_DATOS_IN = 2;
        internal const int EVE_NUEVA_CONEXION = 3;
        internal const int EVE_CONEXION_FIN = 4;
        internal const int EVE_ACEPTAR_CONEXION = 5;
        internal const int EVE_ESPERA_CONEXION = 6;

        private Thread _thrCliente;
        private Thread _thrClienteConexion;

        private TcpListener _tcpListen;
        private TcpClient _tcpCliente;
        private Encoding _encoder;
        UdpClient _newsock;
        IPEndPoint _sender;
        private bool _tcp;

        internal bool Conectado;
        internal bool EsperandoConexion;
        internal string ip_Conexion;
        internal int Puerto;
        internal int IndiceCon;

        internal delegate void DelegadoError(int indice, string ErrDescrip);
        internal event DelegadoError Eve_Error;
        private void Error(int Indice, string ErrDescrip)
        {
            this.Eve_Error(Indice, ErrDescrip);
        }

        internal delegate void DelegadoAceptarConexion(int Indice, string IpOrigen);
        internal event DelegadoAceptarConexion Eve_AceptarConexion;
        private void Aceptar_Conexion(int Indice, string IpOrigen)
        {
            //EsperandoConexion = false;
            this.Eve_AceptarConexion(Indice, IpOrigen);
        }

        internal delegate void DelegadoNuevaConexion(int Indice);
        internal event DelegadoNuevaConexion Eve_NuevaConexion;
        private void NuevaConexion(int Indice)
        {
            //EsperandoConexion = false;
            this.Eve_NuevaConexion(Indice);
        }

        internal delegate void DelegadoFinConexion(int Indice);
        internal event DelegadoFinConexion Eve_FinConexion;
        private void FinConexion(int Indice)
        {
            this.Eve_FinConexion(Indice);
        }

        internal delegate void DelegadoDatosIn(int indice, string Datos);
        internal event DelegadoDatosIn Eve_DatosIn;
        private void DatosIn(int Indice, string Datos)
        {
            this.Eve_DatosIn(Indice, Datos);
        }

        internal delegate void DelegadoEsperaConexion(int Indice, bool Escuchando);
        internal event DelegadoEsperaConexion Eve_Espera_Conexion;
        private void Espera_Conexion(int Indice, bool Escuchando)
        {
            this.Eve_Espera_Conexion(Indice, Escuchando);
        }

        internal delegate void Delegado_Envio_Completo(int Indice, long Size);
        internal event Delegado_Envio_Completo Eve_Envio_Completo;
        private void Envio_Completo(int Indice, long Size)
        {
            this.Eve_Envio_Completo(Indice, Size);
        }

        internal delegate void Delegado_posicion_Envio(int Indice, long pos);
        internal event Delegado_posicion_Envio Eve_Posicion_Envio;
        private void Posicion_Envio(int Indice, long pos)
        {
            this.Eve_Posicion_Envio(Indice, pos);
        }

        /// <summary>
        /// setea el socket para la escucha
        /// </summary>
        /// <param name="PuertoEscucha">puerto de escucha</param>
        /// <param name="Cod">codopage</param>
        /// <param name="IndiceConexion">inidce de conexion</param>
        /// <param name="Mensaje">mensaje que retorna en caso de error</param>
        /// <param name="tcp">define si la conexión es tpc o udp, vaor default true, si es falso, la conexion es udp</param>
        internal Servidor(int PuertoEscucha, int Cod, int IndiceConexion, ref string Mensaje, bool tcp=true)
        {

            IndiceCon = IndiceConexion;

            try
            {
                CodePage(Cod, ref Mensaje);
                if (Mensaje != "")
                {
                    Mensaje = Mensaje + " No se pudo iniciar ConServer";
                    return;
                }

                Puerto = PuertoEscucha;
                _tcp = tcp;
            }
            catch (Exception Err)
            {
                Mensaje = Err.Message;
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
                ThreadStart Cliente;
                if (_tcp)
                {
                    Cliente = new ThreadStart(EscucharTCP);
                }
                else
                {
                    Cliente = new ThreadStart(EscucharUDP);
                }
                _thrCliente = new Thread(Cliente);
                _thrCliente.Name = "ThEscucha";
                _thrCliente.IsBackground = true;
                _thrCliente.Start();
            }
            catch (Exception Err)
            {
                EsperandoConexion = false;
                Espera_Conexion(IndiceCon, false);
                Mensaje = Err.Message;
            }
        }

        /// <summary>
        /// Detiene todas las conexiones
        /// </summary>
        /// <param name="Mensaje">Mensaje que retorna en caso de error</param>
        internal void Detener(ref string Mensaje)
        {

            EsperandoConexion = false;
            Espera_Conexion(IndiceCon, false);
            try
            {
                if (_thrCliente != null)
                {

                    _tcpListen.Stop();
                    _tcpCliente.Close();

                    _thrCliente.Abort();

                    _thrClienteConexion.Abort();

                    Thread.EndCriticalRegion(); //esto cierra todo con o sin conexiones
                }
            }
            catch (Exception Err)
            {
                if (modo_Debug == true)
                {
                    this.Eve_Error(IndiceCon, Err.Message + " TcpListen.Start()");
                    Mensaje = Err.Message;
                }
                //this.Eve_FinConexion(IndiceCon);
            }
        }

        private void EscucharUDP()
        {
            try
            {
                byte[] data = new byte[1024];
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, Puerto);

                _newsock = new UdpClient(ipep);

                _sender = new IPEndPoint(IPAddress.Any, 0);
                ip_Conexion = _sender.Address.ToString();

                data = _newsock.Receive(ref _sender);
                this.Eve_DatosIn(IndiceCon, Encoding.ASCII.GetString(data, 0, data.Length));

                while (true)
                {
                    data = _newsock.Receive(ref _sender);
                    this.Eve_DatosIn(IndiceCon, Encoding.ASCII.GetString(data, 0, data.Length));
                }
            }
            catch(Exception e)
            {
                this.Eve_Error(IndiceCon, e.Message);
            }
        }

        private void EscucharTCP()
        {
            bool Escuchar;

            TcpClient Cliente = new TcpClient();

            _tcpListen = new TcpListener(IPAddress.Any, Puerto);
            
            try
            {
                EsperandoConexion = true;
                Espera_Conexion(IndiceCon, true);

                _tcpListen.Stop();
                _tcpListen.Start();
                Escuchar = true;
            }
            catch (Exception Err)
            {
                //el error salta aca, por que ya abri una nueva instancia que esta eschando aca.
                EsperandoConexion = false;
                Espera_Conexion(IndiceCon, EsperandoConexion);
                Escuchar = false;
                this.Eve_Error(IndiceCon, Err.Message + " TcpListen.Start()");
                this.Eve_FinConexion(IndiceCon);
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
                    Espera_Conexion(IndiceCon, EsperandoConexion);

                    Aceptar_Conexion(IndiceCon, sAux);
                    try
                    {
                        _thrClienteConexion = new Thread(new ParameterizedThreadStart(Cliente_Comunicacion));
                        _thrClienteConexion.Name = "ThrCliente";
                        _thrClienteConexion.IsBackground = true;
                        _thrClienteConexion.Start(Cliente);
                        //ahora tendria que dejar de escuchar
                        Escuchar = false;
                        _tcpListen.Stop();
                        //Cliente.Close();
                        //thrCliente.Abort();
                    }
                    catch (Exception Err)
                    {
                        Escuchar = false;
                        this.Eve_Error(IndiceCon, Err.Message + " threadConexion");
                    }
                }
                catch (Exception Err)
                {
                    if (modo_Debug == true)
                    {
                        this.Eve_Error(IndiceCon, Err.Message + " TcpListen.Accept()");
                    }
                    Escuchar = false;
                    _tcpListen.Stop();
                    //Cliente.Close();
                    //thrCliente.Abort();
                }

            } while (Escuchar == true);//fin do
        }

        private void Cliente_Comunicacion(object Cliente)
        {

            bool EveYaDisparado = false;

            try
            {
                //TcpClient tcpCliente = (TcpClient)Cliente;
                _tcpCliente = (TcpClient)Cliente;
                NetworkStream clientStream = _tcpCliente.GetStream();
                string strDatos;

                _tcpCliente = (TcpClient)Cliente;

                //levanto evento nueva conexion
                Conectado = true;
                this.Eve_NuevaConexion(IndiceCon);


                byte[] message = new byte[4096];

                int bytesRead;

                while (true)
                {
                    bytesRead = 0;

                    try
                    {
                        //blocks until a client sends a message

                        bytesRead = clientStream.Read(message, 0, 4096);

                    }
                    catch (Exception Err)
                    {
                        Conectado = false;
                        _tcpCliente.Close();
                        if (modo_Debug == true)
                        {
                            this.Eve_Error(IndiceCon, "Cliente Comunicacion; Servidor>\r\n" + Err.Message + "\r\n");
                        }
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        //el cliente se desconecto!
                        Conectado = false;
                        _tcpCliente.Close();
                        this.Eve_FinConexion(IndiceCon);
                        EveYaDisparado = true;
                        break;
                    }
                    
                    //llegó el mensaje
                    strDatos = _encoder.GetString(message, 0, bytesRead);

                    Eve_DatosIn(IndiceCon, strDatos + " " + _tcpCliente.Client.RemoteEndPoint.ToString());

                }
                //el cliente cerro la conexion
                Conectado = false;
                _tcpCliente.Close();

                if (EveYaDisparado == false)
                {
                    this.FinConexion(IndiceCon);
                }

            }
            catch (SocketException Err)//(Exception Err)
            {
                this.Eve_Error(IndiceCon, " Cliente_Comunicacion>\r\n" + Err.Message + "\r\n");
            }
        }

        /// <summary>
        /// Envia un mensaje al cliente conectado
        /// </summary>
        /// <param name="Indice">Indice de conexion al que se el envia el mensaje</param>
        /// <param name="Datos">el mensaje a enviar</param>
        /// <param name="Resultado">Mensaje que retorna en caso de error</param>
        //internal void Enviar(int Indice,string Datos, ref string Resultado)
        internal void Enviar(string datos, ref string resultado)
        {
            int buf=0;
            try
            {
                
                if (_tcp)
                {
                    TcpClient TcpClienteDatos = _tcpCliente;
                    NetworkStream clienteStream = TcpClienteDatos.GetStream();

                    byte[] buffer = _encoder.GetBytes(datos);
                    buf = buffer.Length;
                    clienteStream.Write(buffer, 0, buf);
                    clienteStream.Flush(); //envio los datos
                }
                else
                {
                    
                    UdpClient udpClient = new UdpClient();

                    Byte[] sendBytes = Encoding.ASCII.GetBytes(datos);
                    try
                    {
                        udpClient.Send(sendBytes, sendBytes.Length, "192.168.0.6", 1492);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }

                }
                Envio_Completo(IndiceCon, buf);

            }
            catch (Exception Err)
            {
                resultado = Err.Message;
                Error(IndiceCon, resultado);
            }
        }

        internal void Enviar_ByteArray(byte[] memArray, int TamCluster, ref string Error)
        {
            string Datos = "";
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

                TcpClient TcpClienteDatos = _tcpCliente;
                NetworkStream clientStream = TcpClienteDatos.GetStream();

                while (nPosActual < nTam - 1) //quizas aca me falte un byte (-1)
                {
                    nCondicion = nPosActual + TamCluster;
                    for (int I = nPosActual; I <= nCondicion - 1; I++)
                    {
                        //meto todo al string para manadar
                        Datos = Datos + Convert.ToChar(memArray[I]);
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

                    nPosActual = nPosLectura; //ver que no me quede uno atras
                    Posicion_Envio(IndiceCon, nPosActual); //levanto la posición en la que quede donde estoy enviando

                    //envio los datos
                    byte[] buffer = _encoder.GetBytes(Datos);

                    clientStream.Write(buffer, 0, buffer.Length);
                    clientStream.Flush(); //envio lo datos
                    Envio_Completo(IndiceCon, buffer.Length); //evento envio completo

                    Thread.Sleep(5);

                    Datos = ""; //limpio la cadena
                }//fin while

            }
            catch (Exception Err)
            {
                Error = Err.ToString();
                Eve_Error(IndiceCon, Err.Message);
            }

        }

        /// <summary>
        /// Code page para iniciar la comunicacion
        /// </summary>
        /// <param name="Codigo">codigo de codepage</param>
        /// <param name="Error">Mensaje que retorna en caso de error</param>
        internal void CodePage(int Codigo, ref string Error)
        {
            try
            {
                _encoder = Encoding.GetEncoding(Codigo);
            }
            catch (Exception err)
            {
                Error = err.Message;
            }
        }



    }
}
