using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Sockets;


namespace TestSendArchivo
{
    class Program
    {
        static bool _modoServer;

        //static SockServer _obServer;
        //static SocksCliente _obCliente;

        static FileStream _ArchStream;
        static BinaryWriter _ArchWriter;

        static Socket _obSocket;

        static int _tam;

        static bool _enviarArchivo = false;
        static bool _recibirArchivo = false;

        const string C_ARCHIVO_ENVIAR = @"C:\Users\Usuario\Desktop\Programacion\putty.exe";
        //const string C_ARCHIVO_ENVIAR = @"C:\Users\Usuario\Desktop\Programacion\test.txt";

        const string C_ARCHIVO_RECIBIR_SERVER = @"C:\prueba\putty.exe";
        //const string C_ARCHIVO_RECIBIR_SERVER = @"C:\prueba\test.txt";

        const string C_ARCHIVO_RECIBIR_CLIENTE = @"C:\prueba\putty2.exe";
        //const string C_ARCHIVO_RECIBIR_CLIENTE = @"C:\prueba\test2.txt";

        const string C_ENVACRH = "ARCH>";
        const string C_FINARCH = "FINARCH>";
        //const string C_CRLF = Convert.ToChar(13).ToString() + Convert.ToChar(10).ToString();
        //delegate void Delegado_SocketServidor(int tipo, int Indice, string Datos, int Tipo_Dato);
        //delegate void Delegado_SocketCliente();

        const int C_TAM_CLUSTER = 1400;


        //[DllImport("User32.dll")]
        //public static extern int MessageBox(int h, string m, string c, int type);

        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        static void Main(string[] args)
        {
            /* //con esto no se escriben los caracteres, como los passwords de linux
            System.Console.Write("password: ");
            string password = null;
            while (true)
            {
                var key = System.Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                    break;
                password += key.KeyChar;
            }
            Console.WriteLine(password);
            */

            //MessageBox(0, "hola mundo", "mensajin", 2); muestra un mensaje y se para todo hasta que se toque un boton

            //para poder mostrar colores usando los comandos de vt100 que si tiene telnet
            var handle = GetStdHandle(STD_OUTPUT_HANDLE);
            uint mode;
            GetConsoleMode(handle, out mode);
            mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            SetConsoleMode(handle, mode);


            Console.WriteLine("\x1b[93m TEST SERVER.\r\n");

            _obSocket = new Socket();
            _obSocket.Event_Socket += new Socket.Delegado_Socket_Event(EvSockets); 

            if (args.Length > 0)
            {
                _modoServer = true;
                Server();

            }
            else
            {
                Console.WriteLine("1 MODO SERVER, 2 MODO CLIENTE, 3 MODO SERVER UDP, 4 MODO CLIENTE UDP");
                while (true)
                {
                    var input = Console.ReadLine();
                    if ((_obSocket.ModoCliente) || (_obSocket.ModoServidor))
                    {
                        if (input.Equals("fin", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                        else if (input.Equals("send", StringComparison.OrdinalIgnoreCase))
                        {
                            //mando el archivo
                            EnviarArchivo(false);
                            _enviarArchivo = true;
                        }
                        else
                        {
                            _obSocket.Enviar(input + "\r\n");
                        }
                    }
                    else
                    {
                        if (input.Equals("1", StringComparison.OrdinalIgnoreCase))
                        {
                            _modoServer = true;
                            Server();
                            _obSocket.ModoServidor = _modoServer;
                            //break;
                        }
                        if (input.Equals("2", StringComparison.OrdinalIgnoreCase))
                        {
                            _modoServer = false;
                            Cliente();
                            _obSocket.ModoCliente = true;
                            //break;
                        }
                        if (input.Equals("3", StringComparison.OrdinalIgnoreCase))
                        {
                            _modoServer = true;
                            Server(false);
                            //break;
                        }
                        if (input.Equals("4", StringComparison.OrdinalIgnoreCase))
                        {
                            _modoServer = false;
                            ClienteUDP();
                            //break;
                        }
                    }

                }
            }

            //Console.WriteLine("tecla cualquiera para salir");
            //Console.ReadLine();
        }

        static void EvSockets(Parametrosvento ev)
        {
            switch (ev.GetEvento)
            {

                //case Socket.C_SERVER_EVENTO_ACEPTAR_CONEXION:
                case Parametrosvento.TipoEvento.ACEPTAR_CONEXION:
                    Console.WriteLine("conectado desde " + ev.GetIpOrigen);
                    break;

                case Parametrosvento.TipoEvento.DATOS_IN:
                    //DatosIn(indice, datos, true, ipOrigen);
                    DatosIn(ev.GetIndice, ev.GetDatos, true, ev.GetIpOrigen);
                    break;

                case Parametrosvento.TipoEvento.ERROR:
                    Console.WriteLine("error cliente");
                    Console.WriteLine(ev.GetDatos);
                    break;
                /*
                case Socket.C_EVENTO_ERROR:
                    Console.WriteLine("error cliente");
                    Console.WriteLine(datos);
                    break;

                case Socket.C_SERVER_EVENTO_ERROR:
                    Console.WriteLine("error cliente");
                    Console.WriteLine(datos);
                    break;

                case Socket.C_CLIENTE_EVENTO_TIME_OUT:
                    Console.WriteLine("error cliente time out =(");
                    Console.WriteLine(datos);
                    break;

                case Socket.C_CLIENTE_EVENTO_DATOS_IN:
                    //Console.WriteLine(datos);
                    DatosIn(indice, datos, false, ipOrigen);
                    break;

                case Socket.C_CLIENTE_EVENTO_CONEXION_OK:
                    Console.WriteLine("Conectado OK");
                    break;
                */
            }
        }

        static void Server(bool tcp =true)
        {
            string Mensaje = "";

            if (tcp)
            {
                Console.WriteLine("modo server");
                Console.Title = "MODO SERVER"; 
            }
            else
            {
                Console.WriteLine("modo server udp");
                Console.Title = "MODO SERVER UDP";
            }
                      
            _obSocket.ModoServidor = true;
            _obSocket.SetServer(1492, 65001, 0,tcp);
            _obSocket.StartServer();

            if (Mensaje != "")
            {
                Console.WriteLine(Mensaje);
                return;
            }
        }

        static void DatosIn(int indice,string datos,bool server,string ipOrigen)
        {
            //Console.WriteLine(datos);
            //NO BORRAR COMENTARIOS
            /*
            if (datos=="enviar")
            {
                //mando el archivo
                Console.WriteLine("envio");
            }

            if (datos=="recibir")
            {
                //le pido que me mande el archivo
                Console.WriteLine("recibo");
            }
            */
            if (_obSocket.tcp)
            {
                if ((datos.Contains(C_ENVACRH + "\r\n")) || (datos.Contains(C_FINARCH + "\r\n")))
                {
                    if (!_recibirArchivo)
                    {
                        _recibirArchivo = true;
                    }
                    else
                    {
                        _recibirArchivo = false;
                    }
                    ArmarArchivo(datos); //lega el archivo
                    return;
                }
                else
                {
                    if (!_recibirArchivo)
                    {
                        Console.WriteLine("[" + ipOrigen + "] " + datos);
                    }
                    else
                    {
                        ArmarArchivo(datos);
                    }
                }
            }
            else
            {
                Console.WriteLine("[" + ipOrigen + "] " + datos);
                _obSocket.Enviar("funciona! ameo");
            }
        }

        static void Cliente()
        {
            string Mensaje = "";

            Console.WriteLine("modo cliente");
            Console.Title = "MODO CLIENTE";

            _obSocket.ModoCliente = true;
            _obSocket.SetCliente(1492, 65001, 0, "127.0.0.1",5);
            
            _obSocket.Conectar();


            if (Mensaje != "")
            {
                Console.WriteLine(Mensaje);
            }

        }

        static void ClienteUDP()
        {
            string Mensaje = "";

            Console.WriteLine("modo cliente UDP");
            Console.Title = "MODO CLIENTE UDP";

            _obSocket.ModoCliente = true;
            

        }

        static void setArchivos()
        {
            string sPath;

            if (_modoServer)
            {
                sPath = C_ARCHIVO_RECIBIR_SERVER;
            }
            else
            {
                sPath = C_ARCHIVO_RECIBIR_CLIENTE;
            }

            //muy cabeza, pero para la prueba tiene que servir
            _ArchStream = new FileStream(sPath, FileMode.OpenOrCreate, FileAccess.Write);
            _ArchWriter = new BinaryWriter(_ArchStream);
        }

        static void ArmarArchivo(string Datos)
        {
            try
            {
                string sAux = "";
                int nPos = 0;

                if (Datos.Contains(C_FINARCH + "\r\n"))
                //if (Datos.Contains(C_FINARCH))
                {
                    Console.WriteLine(Datos + "esto llego");

                    nPos = Datos.IndexOf(C_FINARCH + "\r\n");
                    sAux = Datos.Substring(nPos); //me quedo con el comando
                    Datos = Datos.Substring(0, nPos); //me quedo con la parte final del archivo

                    if (Datos != "")
                    {
                        _ArchWriter.Write(Encoding.GetEncoding(28591).GetBytes(Datos));
                    }

                    _ArchWriter.Close();
                    _ArchStream.Close();

                    Console.WriteLine("llegó... ponele ");
                    Datos = "";
                    return;
                }                

                /*
                sAux = Datos.Substring(0, Datos.Length - 2); //saco salto de linea y retorno de carro
                if (sAux == C_ENVACRH)
                {
                    setArchivos();
                    Datos = "";
                    return;
                }
                 */
                if (Datos.Contains(C_ENVACRH + "\r\n"))
                {
                    nPos = Datos.IndexOf(C_ENVACRH + "\r\n");
                    sAux = Datos.Substring(nPos); //me quedo con el comando
                    Datos = Datos.Substring(0, nPos); //me quedo con la parte final del archivo
                    setArchivos();
                }
                
                _ArchWriter.Write(Encoding.GetEncoding(28591).GetBytes(Datos));
                Datos = "";
                
            }
            catch (Exception err)
            {
                Console.WriteLine("Error> " + err.Message);
            }
        }

        static void EnviarArchivo(bool modoServer)
        {
            byte[] memArrayArchivo;
            FileStream Archivo;
            string Err = "";

            //string vbcrlf = Convert.ToChar(10).ToString() + Convert.ToChar(13).ToString();

            try
            {
                Archivo = new FileStream(C_ARCHIVO_ENVIAR, FileMode.Open);
                memArrayArchivo = new byte[Archivo.Length];

                Archivo.Read(memArrayArchivo, 0, (int)Archivo.Length);
                Archivo.Close();
                Console.WriteLine(memArrayArchivo.Length);

                _obSocket.Enviar(C_ENVACRH + "\r\n");
                _obSocket.EnviarArray(memArrayArchivo, C_TAM_CLUSTER);


                #region forma vieja
                /*
                if (modoServer)
                {
                    //_obServer.EnviarDatos(C_ENVACRH + "\r\n"); 
                    //_obServer.EnviarArray(memArrayArchivo, C_TAM_CLUSTER, ref Err);

                }
                else
                {
                    //_obCliente.EnviarDatos(C_ENVACRH + "\r\n"); 
                    //_obCliente.EnviarArray(memArrayArchivo, C_TAM_CLUSTER, ref Err);
                }*/
                #endregion

                if (Err != "")
                {
                    Console.WriteLine("ERROR " + Err);
                }
                else
                {
                    #region forma vieja
                    /*
                    if (modoServer)
                    {
                        //_obServer.EnviarDatos(C_FINARCH + "\r\n");
                    }
                    else
                    {
                        //_obCliente.EnviarDatos(C_FINARCH + "\r\n");
                    }*/
                    #endregion

                    _obSocket.Enviar(C_FINARCH + "\r\n");
                    Console.WriteLine("envio ok");
                }
            }
            catch (Exception Error)
            {
                Console.WriteLine(Error.Message);
            }
        }


    }
}
