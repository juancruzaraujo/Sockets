using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;


namespace Sockets
{
    internal class Cliente
    {

        #if DEBUG
            private /*static*/ bool modo_Debug = true;
        #else
            private /*static*/ bool modo_Debug = false;
        #endif

        internal const int EVE_ERROR = 0;
        internal const int EVE_ENVIO_COMPLETO = 1;
        internal const int EVE_DATOS_IN = 2;
        internal const int EVE_CONEXION_OK = 3;
        internal const int EVE_CONEXION_FIN = 4;
        internal const int EVE_TIME_OUT = 5;
        

        private TcpClient _clienteSockTCP; //el socket en cuestion!
        private Thread _thrCliente; //hilo con el flujo de datos
        private Thread _thr_TimeOut; //hilo que setea cuando se inicia el timer de timeout de intento de conexion
        private bool _tcp;
        private UdpClient _clienteSockUDP;
        private IPEndPoint _epUDP;

        /// <summary>
        /// Verdadero estoy conectado, falso, no estoy conectado
        /// </summary>
        internal bool conectado; //me dice si estoy conectado o no

        /// <summary>
        /// Devuelve o establece el indice de conexion, necesario si se crea una lista o un vector de este objeto
        /// </summary>
        internal int indiceCon;

        /// <summary>
        /// Setea o devuelve el time out para poder conectarse en segundos.
        /// </summary>
        private int _val_TimeOut;
        internal int SetGetTimeOut
        {
            get
            {
                return _val_TimeOut;
            }
            set
            {
                _val_TimeOut = value;
                if (_val_TimeOut > 60)
                {
                    _val_TimeOut = 60;
                }
            }
        }

        private int _tipoCod;
        private Encoding _encoder;

        internal delegate void Delegado_Cliente_Event(Parametrosvento servidorParametrosEvento);
        internal event Delegado_Cliente_Event evento_cliente;
        private void Evento_Cliente(Parametrosvento servidorParametrosEvento)
        {
            this.evento_cliente(servidorParametrosEvento);
        }

        internal Cliente(bool tcp=true)
        {
            _tcp = tcp;
            /*if (modo_Debug == false)
            {
                Val_TimeOut = 30000;
            }
            else
            {
                Val_TimeOut = 5000;
            }*/
        }

        internal void Conectar(int indice,string host, int puerto, ref string err)
        {
            if (_tcp)
            {
                Conectar_TCP(indice, host, puerto, ref err);
            }
            else
            {
                Conectar_UPD(indice, host, puerto, ref err);
            }
        }

        private void Conectar_TCP(int indice, string host, int puerto, ref string err)
        {
            int nPuerto = 0;
            indiceCon = indice;
            
            try
            {
                conectado = false;

                _clienteSockTCP = new TcpClient();

                nPuerto = puerto;

                for (int i = 0; i < _val_TimeOut; ++i)
                {
                    try
                    {
                        _clienteSockTCP.Connect(host, nPuerto);
                        break; // salgo del for

                    }
                    catch (Exception e)
                    {
                        Thread.Sleep(1000); //espero un segundo y vuelvo a intentar
                    }

                }


                if (_clienteSockTCP.Connected)
                {

                    conectado = true;

                    //Eve_Conexion_OK(indiceCon);
                    Parametrosvento ev = new Parametrosvento();
                    ev.SetEvento(Parametrosvento.TipoEvento.CONEXION_OK);
                    GenerarEvento(ev);

                    ThreadStart thrclienteTCP = new ThreadStart(Flujo_Datos_tcp);
                    _thrCliente = new Thread(thrclienteTCP);
                    _thrCliente.Name = "ThrClienteTCP";
                    _thrCliente.Start();
                }
                else
                {
                    //Eve_TimeOut(indiceCon);
                    Parametrosvento evTime = new Parametrosvento();
                    evTime.SetEvento(Parametrosvento.TipoEvento.TIME_OUT);
                    GenerarEvento(evTime);
                    return;
                }


            }
            catch (Exception error)
            {
                ErrorConectar(error, ref err);
            }
        }

        private void Conectar_UPD(int indice, string host, int puerto, ref string err)
        {
            try
            {

                _clienteSockUDP = new UdpClient();
                _epUDP = new IPEndPoint(IPAddress.Parse(host), puerto);
                _clienteSockUDP.Connect(_epUDP);

                ThreadStart thrclienteUDP = new ThreadStart(Flujo_Datos_UDP);
                _thrCliente = new Thread(thrclienteUDP);
                _thrCliente.Name = "thrClienteUDP";
                _thrCliente.Start();
            }
            catch(Exception error)
            {
                ErrorConectar(error, ref err);
            }

        }

        private void ErrorConectar(Exception errorDescripcion,ref string err)
        {
            conectado = false;
            Error(errorDescripcion.Message);
            err = errorDescripcion.Message;
        }

        private void Flujo_Datos_tcp()
        {
            try
            {
                TcpClient tcpCliente = _clienteSockTCP;
                NetworkStream clientStream = tcpCliente.GetStream();

                byte[] message = new byte[4096];
                int bytesRead;
                string strDatos;

                while (true)
                {
                    bytesRead = 0;

                    try
                    {
                        //se bloquea hasta que llega un mensaje
                        bytesRead = clientStream.Read(message, 0, 4096);

                    }
                    catch (Exception error)
                    {
                        Error(error.Message);

                        Conexion_Fin();
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        //el cliente se desconecto!
                        Conexion_Fin();
                        break;
                    }

                    strDatos = _encoder.GetString(message, 0, bytesRead);
                    
                    Parametrosvento ev = new Parametrosvento();
                    ev.SetEvento(Parametrosvento.TipoEvento.DATOS_IN).SetDatos(strDatos);
                    GenerarEvento(ev);

                }
                //el cliente cerro la conexion
                Conexion_Fin();
                tcpCliente.Close(); //Sí se cierra el server.

                _thrCliente.Abort();
            }
            catch (Exception Err)
            {
                Error(Err.Message);
                Conexion_Fin();
            }
        } //fin Flujo_Datos_tcp

        private void Flujo_Datos_UDP()
        {
            try
            {
                var datosInUDP = _clienteSockUDP.Receive(ref _epUDP);
                Parametrosvento ev = new Parametrosvento();
                ev.SetEvento(Parametrosvento.TipoEvento.DATOS_IN).SetDatos(_epUDP.ToString()).SetIpOrigen(_clienteSockUDP.ToString());
                GenerarEvento(ev);

            }
            catch(Exception err)
            {
                Error(err.Message);
            }
        }

        private void Conexion_Fin()
        {
            Parametrosvento ev = new Parametrosvento();
            ev.SetEvento(Parametrosvento.TipoEvento.CONEXION_FIN);
            GenerarEvento(ev);
        }

        private void Error(string mensaje)
        {
            Parametrosvento evErr = new Parametrosvento();
            evErr.SetEvento(Parametrosvento.TipoEvento.ERROR).SetDatos(mensaje);
            GenerarEvento(evErr);
        }

        internal void Enviar(string datos, ref string error)
        {
            if (_tcp)
            {
                EnviarTCP(datos, ref error);
            }
            else
            {
                EnviarUDP(datos, ref error);
            }
        }

        private void EnviarTCP(string datos, ref string error)
        {
            try
            {
                if (conectado == true)
                {
                    TcpClient TcpClienteDatos = _clienteSockTCP;
                    NetworkStream clientStream = TcpClienteDatos.GetStream();

                    byte[] buffer = _encoder.GetBytes(datos);


                    clientStream.Write(buffer, 0, buffer.Length);
                    clientStream.Flush(); //envio lo datos

                    Parametrosvento ev = new Parametrosvento();
                    ev.SetEvento(Parametrosvento.TipoEvento.ENVIO_COMPLETO);
                    GenerarEvento(ev);
                }
                else
                {
                    error = "No esta conectado";
                    Error(error);
                }
            }
            catch (Exception err)
            {
                error = err.Message;
                Error(error);
            }
        }

        private void EnviarUDP(string datos, ref string error)
        {
            Byte[] bytesEnviar = Encoding.ASCII.GetBytes(datos);
            _clienteSockUDP.Send(bytesEnviar, bytesEnviar.Length);
            Parametrosvento ev = new Parametrosvento();
            ev.SetEvento(Parametrosvento.TipoEvento.ENVIO_COMPLETO).SetSize(bytesEnviar.Length).SetIpDestino(_epUDP.ToString());
        }

        internal void Cerrar_Conexion()
        {
            if (conectado == true)
            {
                System.Diagnostics.Debug.WriteLine("Cierro Ok"); //para pruebas
                _clienteSockTCP.Close();
                _thrCliente.Abort();

                Conexion_Fin();
            }
        }

        /// <summary>
        /// Tipo de Caracteres va a enviar y a recibir
        /// </summary>
        /// <param name="Tipo">0 UTF8: 1 UTF7:, 2 UTF32:, 3 Ascii:, 4 Unicode</param>
        internal void Codificacion(int Tipo)
        {

            _tipoCod = Tipo;
            switch (Tipo)
            {
                case 0:
                    _encoder = new UTF8Encoding();
                    break;

                case 1:
                    _encoder = new UTF7Encoding();
                    break;

                case 2:
                    _encoder = new UTF32Encoding();
                    break;

                case 3:
                    _encoder = new ASCIIEncoding();
                    break;

                case 4:
                    _encoder = new UnicodeEncoding();
                    break;

                default:
                    _encoder = new UTF8Encoding();
                    break;
            }
        }
        /// <summary>
        /// CodePage:
        ///     Setea el codigo de paginas de carectes para enviar y recibir
        /// </summary>
        /// <param name="Codigo">Código de pagina</param>
        /// <param name="Error">Sí hay un error se guarda en Error, de caso contrario queda vacio</param>
        internal void CodePage(int Codigo, ref string error)
        {
            try
            {
                _encoder = Encoding.GetEncoding(Codigo);
            }
            catch (Exception err)
            {
                error = err.Message;
                Error(error);
            }
        }

        internal void Enviar_ByteArray(byte[] memArray, int TamCluster, ref string error)
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

                TcpClient TcpClienteDatos = _clienteSockTCP;
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
                    Parametrosvento evPos = new Parametrosvento(); //levanto la posición en la que quede donde estoy enviando
                    evPos.SetEvento(Parametrosvento.TipoEvento.POSICION_ENVIO).SetPosicion(nPosActual);
                    GenerarEvento(evPos);

                    //envio los datos
                    byte[] buffer = _encoder.GetBytes(Datos);

                    clientStream.Write(buffer, 0, buffer.Length);
                    clientStream.Flush(); //envio lo datos

                    Parametrosvento ev = new Parametrosvento();
                    ev.SetEvento(Parametrosvento.TipoEvento.ENVIO_COMPLETO).SetPosicion(buffer.Length);
                    GenerarEvento(ev);
                    
                    //esperamos 5milisegunbdos para continuar
                    //si, asi se evita el solapamiento de paquetes.
                    Thread.Sleep(5); 

                    Datos = ""; //limpio la cadena
                }//fin while

            }
            catch (Exception err)
            {
                error = err.ToString();
                //Eve_Error(indiceCon,Err.Message);
                Error(err.Message);
            }

        }

        private void GenerarEvento(Parametrosvento ob)
        {
            ob.SetIndice(indiceCon);

            Evento_Cliente(ob);
        }
    }
}
