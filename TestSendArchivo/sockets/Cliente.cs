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


        private TcpClient _clienteSock; //el socket en cuestion!
        private Thread _thrCliente; //hilo con el flujo de datos
        private Thread _thr_TimeOut; //hilo que setea cuando se inicia el timer de timeout de intento de conexion

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

        internal delegate void DelegadoDatosIN(int Indice,string Mensaje);
        internal event DelegadoDatosIN Eve_DatosIn;


        internal delegate void DelegadoEnvioCompleto(int Indice,long DatosSend);
        internal event DelegadoEnvioCompleto Eve_EnvioCompleto;
        private void Envio_Completo(int Indice,long DatosSend)
        {
            this.Eve_EnvioCompleto(Indice,DatosSend);
        }

        internal delegate void DelegadoConexion_ok(int Indice);
        internal event DelegadoConexion_ok Eve_Conexion_OK;
        private void Conexion_Ok(int Indice)
        {
            this.Eve_Conexion_OK(Indice);
        }

        internal delegate void DelegadoConexion_Fin(int Indice);
        internal event DelegadoConexion_Fin Eve_Conexion_Fin;
        private void Conexion_Fin(int Indice)
        {
            this.Eve_Conexion_Fin(Indice);
        }

        internal delegate void DelegadoTimeOut(int Indice);
        internal event DelegadoTimeOut Eve_TimeOut;
        private void TimeOut(int Indice)
        {
            this.Eve_TimeOut(Indice);
        }

        internal delegate void Delegado_posicion_Envio(int Indice, long pos);
        internal event Delegado_posicion_Envio Eve_Posicion_Envio;
        private void Posicion_Envio(int Indice, long pos)
        {
            this.Eve_Posicion_Envio(Indice, pos);
        }

        internal delegate void DelegadoError(int Indice,string Mensaje);
        internal event DelegadoError Eve_Error;
        private void Error(int Indice, string Mensaje)
        {
            this.Eve_Error(Indice,Mensaje);
        }


        internal Cliente()
        {
            /*if (modo_Debug == false)
            {
                Val_TimeOut = 30000;
            }
            else
            {
                Val_TimeOut = 5000;
            }*/
        }

        internal void Conectar(int Indice,string Host, int Puerto, ref string Err)
        {
            int nPuerto = 0;
            indiceCon = Indice;
            try
            {
                conectado = false;

                _clienteSock = new TcpClient();
                
                //nPuerto = Convert.ToInt32(Puerto);
                nPuerto = Puerto;
                //ClienteSock.Connect(Host, nPuerto);

                #region con time out
                //IAsyncResult result = ClienteSock.BeginConnect(Host, nPuerto, null, null);
                //bool success = result.AsyncWaitHandle.WaitOne(Val_TimeOut, true);               
                //if (!success)
                //{
                ////    // NOTE, MUST CLOSE THE SOCKET

                    
                //    ClienteSock.Close();
                //    //throw new ApplicationException("TimeOut");  //no va
                //    Eve_TimeOut(IndiceCon);
                //    return;
                //}
                #endregion

                for (int i = 0; i < _val_TimeOut; ++i)
                {
                    try
                    {
                        _clienteSock.Connect(Host, nPuerto);
                        //DoSomethingThatMightThrowAnException();

                        break; // salgo del for

                    }
                    catch (Exception e)
                    {
                        Thread.Sleep(1000); //espero un segundo y vuelvo a intentar
                    }

                }


                if (_clienteSock.Connected)
                {

                    conectado = true;
                    Eve_Conexion_OK(indiceCon);

                    ThreadStart Cliente = new ThreadStart(Flujo_Datos);
                    _thrCliente = new Thread(Cliente);
                    _thrCliente.Name = "ThrCliente";
                    _thrCliente.Start();
                }
                else
                {
                    Eve_TimeOut(indiceCon);
                    return;
                }
 

            }
            catch (Exception error)
            {
                conectado = false;
                Eve_Error(indiceCon,error.Message);
                Err = error.Message;
            }
        }//fin conectar

        private void Flujo_Datos()
        {
            try
            {
                TcpClient tcpCliente = _clienteSock;
                NetworkStream clientStream = tcpCliente.GetStream();

                byte[] message = new byte[4096];
                int bytesRead;
                string strDatos;

                while (true)
                {
                    bytesRead = 0;

                    try
                    {
                        //blocks until a client sends a message
                        bytesRead = clientStream.Read(message, 0, 4096);

                    }
                    catch (Exception Error)
                    {
                        //un error! =(
                        Eve_Error(indiceCon, Error.Message);
                        Conexion_Fin(indiceCon);
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        //el cliente se desconecto!
                        Conexion_Fin(indiceCon);
                        break;
                    }
                    //message has successfully been received
                    //System.Diagnostics.Debug.WriteLine(encoder.GetString(message, 0, bytesRead)); //para pruebas

                    strDatos = _encoder.GetString(message, 0, bytesRead);
                    System.Diagnostics.Debug.WriteLine(strDatos);
                    Eve_DatosIn(indiceCon, strDatos);


                }
                //el cliente cerro la conexion
                System.Diagnostics.Debug.WriteLine("Cierro de golpe"); //para pruebas
                Conexion_Fin(indiceCon);
                tcpCliente.Close(); //Sí se cierra el server.

                //Eve_FinConCLiente(indice);
                _thrCliente.Abort();
            }
            catch (Exception Err)
            {
                Eve_Error(indiceCon, Err.Message);
                Conexion_Fin(indiceCon);
            }
        } //fin Flujo_Datos


        internal void Enviar(string Datos, ref string Error)
        {
            try
            {
                if (conectado == true)
                {
                    TcpClient TcpClienteDatos = _clienteSock;
                    NetworkStream clientStream = TcpClienteDatos.GetStream();

                    //byte[] buffer = encoder.GetBytes(Datos);
                    byte[] buffer = _encoder.GetBytes(Datos);


                    clientStream.Write(buffer, 0, buffer.Length);
                    clientStream.Flush(); //envio lo datos

                    Envio_Completo(indiceCon,buffer.Length);  //evento envio completo
                }
                else
                {
                    Error = "No esta conectado";
                    Eve_Error(indiceCon,Error);
                }
            }
            catch (Exception Err)
            {
                Error = Err.Message;
                Eve_Error(indiceCon,Error);
            }
        }

        internal void Cerrar_Conexion()
        {
            if (conectado == true)
            {
                System.Diagnostics.Debug.WriteLine("Cierro Ok"); //para pruebas
                _clienteSock.Close();
                _thrCliente.Abort();

                Conexion_Fin(indiceCon);
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
        internal void CodePage(int Codigo, ref string Error)
        {
            try
            {
                _encoder = Encoding.GetEncoding(Codigo);
            }
            catch (Exception err)
            {
                Error = err.Message;
                Eve_Error(indiceCon,Error);
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

                TcpClient TcpClienteDatos = _clienteSock;
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
                    Posicion_Envio(indiceCon, nPosActual); //levanto la posición en la que quede donde estoy enviando
                    //envio los datos
                    byte[] buffer = _encoder.GetBytes(Datos);

                    clientStream.Write(buffer, 0, buffer.Length);
                    clientStream.Flush(); //envio lo datos
                    Envio_Completo(indiceCon,buffer.Length); //evento envio completo
                    
                    //esperamos 5milisegunbdos para continuar
                    //si, asi se evita el solapamiento de paquetes.
                    Thread.Sleep(5); 

                    Datos = ""; //limpio la cadena
                }//fin while

            }
            catch (Exception Err)
            {
                Error = Err.ToString();
                Eve_Error(indiceCon,Err.Message);
            }

        }
    }
}
