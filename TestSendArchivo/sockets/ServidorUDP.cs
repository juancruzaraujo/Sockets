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
            internal string datosIn;
        }
        private List<InfoCliente> _lstClientesUDP;

        private Thread _thrCliente;
        private UdpClient _udpClient;

        private int _puerto;
        private int _maxClientesUDP;
        private int _cantidadConexiones = 0;
        private bool _maximoConexiones;
        private bool _serverIniciado;
        private bool _hayMaxConexiones;
        internal string _ip_Conexion;
        
        private bool _conectado;
                
        internal int MaxClientesUDP
        {
            get
            {
                return _maxClientesUDP;
            }
            set
            {
                _maxClientesUDP = value;
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
            _puerto = PuertoEscucha;
            _lstClientesUDP = new List<InfoCliente>();   
        }

        internal void Iniciar()
        {
            try
            {
                
                _serverIniciado = true;
                ThreadStart Cliente;
                Cliente = new ThreadStart(EscucharUDP);

                _thrCliente = new Thread(Cliente);
                _thrCliente.Name = "ThUDP";
                _thrCliente.IsBackground = true;
                _thrCliente.Start();
            }
            catch (Exception err)
            {
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
                buf = sendBytes.Length;

                Parametrosvento ev = new Parametrosvento();
                ev.SetSize(buf)
                .SetEvento(Parametrosvento.TipoEvento.ENVIO_COMPLETO)
                .SetIndiceLista(indice)
                .SetNumConexion(_lstClientesUDP[indice].numConexion);
                GenerarEvento(ev);

            }
            catch (Exception err)
            {
                
                GenerarEventoError(err);
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
            bool generarEventoDatosIn=false;

            try
            {
                bool clienteExistente = false;

                _udpClient = new UdpClient();

                _udpClient.ExclusiveAddressUse = false;
                _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                bool bindeado = false;
                while (_serverIniciado)
                {
                    try
                    {
                        clienteExistente = false;
                        generarEventoDatosIn = false;
                        InfoCliente auxCliente = new InfoCliente();
                        auxCliente.clienteEndPoint = new IPEndPoint(IPAddress.Any, _puerto);
                        if (!bindeado)
                        {
                            _udpClient.Client.Bind(auxCliente.clienteEndPoint);
                            bindeado = true;
                        }

                        //ACÁ LLEGA TODO SIEMPRE Y AHÍ ES DONDE TENGO QUE VER QUE HACER
                        var datosEntrada = _udpClient.Receive(ref auxCliente.clienteEndPoint);

                        if (auxCliente.clienteEndPoint.Port != _puerto) //me fijo no ser yo mismo
                        {

                            for (int i = 0; i < _lstClientesUDP.Count(); i++)
                            {
                                if (_lstClientesUDP[i].clienteEndPoint.Port == auxCliente.clienteEndPoint.Port)
                                {
                                    clienteExistente = true;
                                    indiceMsg = i;
                                    _lstClientesUDP[i].datosIn = Encoding.ASCII.GetString(datosEntrada, 0, datosEntrada.Length);
                                    generarEventoDatosIn = true;
                                    i = _lstClientesUDP.Count();
                                }
                            }
                            if (!clienteExistente)
                            {
                                //SI _maxClientesUDP es mayor a 0 hay un máximo de conexiones
                                bool evaluarMaximoClientesUDP = false; 
                                if (_maxClientesUDP > 0)
                                {
                                    //evaluarMaximoClientesUDP = true;
                                    if (_cantidadConexiones < _maxClientesUDP)
                                    {
                                        evaluarMaximoClientesUDP = true;
                                    }
                                    else
                                    {
                                        evaluarMaximoClientesUDP = false;
                                    }
                                }
                                else
                                {
                                    evaluarMaximoClientesUDP = true; //no hay máximo de conexiones
                                }

                                //if ((_cantidadConexiones < _maxClientesUDP) && ) 
                                if (evaluarMaximoClientesUDP)
                                {
                                    _lstClientesUDP.Add(auxCliente);
                                    indiceMsg = _lstClientesUDP.Count() - 1;
                                    _lstClientesUDP[indiceMsg].primerMensaje = true;
                                    _lstClientesUDP[indiceMsg].datosIn = Encoding.ASCII.GetString(datosEntrada, 0, datosEntrada.Length);
                                    generarEventoDatosIn = true;
                                }
                                else
                                {
                                    //LIMITE DE CONEXIONES
                                    if (!_maximoConexiones) //ya mostre el evento
                                    {
                                        _maximoConexiones = true; //así evito disparar esto siempre
                                        Parametrosvento maxCons = new Parametrosvento();
                                        maxCons.SetEvento(Parametrosvento.TipoEvento.LIMITE_CONEXIONES);
                                        GenerarEvento(maxCons);
                                    }

                                }

                            }

                            if (generarEventoDatosIn) //lo que llego esta fuera del limite de conexiones
                            {

                                _ip_Conexion = _lstClientesUDP[indiceMsg].clienteEndPoint.Address.ToString();

                                if (_lstClientesUDP[indiceMsg].primerMensaje)
                                {
                                    _lstClientesUDP[indiceMsg].primerMensaje = false;
                                    _cantidadConexiones++;
                                    _lstClientesUDP[indiceMsg].numConexion = _cantidadConexiones;

                                    Parametrosvento aceptarCon = new Parametrosvento();
                                    aceptarCon.SetEvento(Parametrosvento.TipoEvento.ACEPTAR_CONEXION)
                                        .SetIpOrigen(_ip_Conexion)
                                        .SetIndiceLista(indiceMsg)
                                        .SetNumConexion(_lstClientesUDP[indiceMsg].numConexion);

                                    GenerarEvento(aceptarCon);

                                    //mantengo esto para que sea el orden de eventos tal como es en tcp
                                    //tendría que agregar algo que permita rechazar la conexion dsp de que se dispara ACEPTAR_CONEXION

                                    Parametrosvento nuevaCon = new Parametrosvento();
                                    nuevaCon.SetEvento(Parametrosvento.TipoEvento.NUEVA_CONEXION)
                                    .SetIpOrigen(_ip_Conexion)
                                    .SetIndiceLista(indiceMsg)
                                    .SetNumConexion(_lstClientesUDP[indiceMsg].numConexion);

                                    GenerarEvento(nuevaCon);

                                    _conectado = true;

                                }

                                Parametrosvento ev = new Parametrosvento();
                                ev.SetDatos(_lstClientesUDP[indiceMsg].datosIn)
                                    .SetIpOrigen(_ip_Conexion)
                                    .SetEvento(Parametrosvento.TipoEvento.DATOS_IN)
                                    .SetIndiceLista(indiceMsg)
                                    .SetNumConexion(_lstClientesUDP[indiceMsg].numConexion);
                                GenerarEvento(ev);

                            } //fin if (generarEventoDatosIn)
                        } //fin if (_remoteEP.Port == _puerto) 
                    }
                    catch(Exception err)
                    {
                        //-2146233040 error que se da cuando se detiene el server udp
                        //-2147467259 cuando llega un mensaje udp simple
                        if (err.HResult != -2146233040 && err.HResult != -2147467259)
                        {
                            GenerarEventoError(err);
                        }
                    }
                }
                
            }
            catch (Exception err)
            {
                _conectado = false;
                
                _udpClient.Close();
                //-2146233040 error que se da cuando se detiene el server udp
                //-2147467259 cuando llega un mensaje udp simple
                if (err.HResult != -2146233040 && err.HResult != -2147467259) //error que se da cuando se detiene el server udp
                {
                    GenerarEventoError(err);
                }

            }
        }

        internal void Enviar_ByteArray(byte[] memArray, int TamCluster, int indice)
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

                    ev.SetEvento(Parametrosvento.TipoEvento.ENVIO_COMPLETO).SetPosicion(datos.Length);
                    GenerarEvento(ev);

                    
                    Enviar(datos,indice);

                    Thread.Sleep(5);

                    datos = ""; //limpio la cadena
                }//fin while

            }
            catch (Exception err)
            {
                GenerarEventoError(err);
            }

        }

        internal void Desconectar(int numConexion)
        {
            for (int i = 0;i<_lstClientesUDP.Count();i++)
            {
                if (_lstClientesUDP[i].numConexion == numConexion)
                {
                    _lstClientesUDP.RemoveAt(i);
                    _cantidadConexiones--;
                    if (_maximoConexiones)
                    {
                        _maximoConexiones = false;
                    }

                }
            }
        }

        internal void DesconectarTodos()
        {
            _lstClientesUDP.Clear();
            _cantidadConexiones = 0;
            _maximoConexiones = false;
        }

        internal void DetenerServer()
        {
            //_thrCliente.Abort();   
            _udpClient.Close();
            _serverIniciado = false;
            _thrCliente.Abort();

            Parametrosvento ev = new Parametrosvento();
            ev.SetEvento(Parametrosvento.TipoEvento.SERVER_DETENIDO);
            GenerarEvento(ev);

        }

        private void GenerarEvento(Parametrosvento ob)
        {
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
            //ev.SetEscuchando(EsperandoConexion).
                ev.SetDatos(err.Message + mensajeOpcional).
                SetEvento(Parametrosvento.TipoEvento.ERROR).
                SetCodError(utils.GetCodigoError(err)).
                SetLineNumberError(utils.GetNumeroDeLineaError(err));
            GenerarEvento(ev);
        }
        
    }
}
