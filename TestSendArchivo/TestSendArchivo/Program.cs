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

        static Sockets.Sockets _obSocket;

        //static int _tam;

        static bool _enviarArchivo = false;
        static bool _recibirArchivo = false;

        const string C_ARCHIVO_ENVIAR = @"C:\Users\Usuario\Desktop\Programacion\putty.exe";
        //const string C_ARCHIVO_ENVIAR = @"C:\Users\Usuario\Desktop\origen\prueba.json";

        const string C_ARCHIVO_RECIBIR_SERVER = @"C:\prueba\putty.exe";
        //const string C_ARCHIVO_RECIBIR_SERVER = @"C:\prueba\resultado.json";

        const string C_ARCHIVO_RECIBIR_CLIENTE = @"C:\prueba\putty2.exe";
        //const string C_ARCHIVO_RECIBIR_CLIENTE = @"C:\prueba\resultado2.json";

        const string C_ENVACRH = "ARCH>";
        const string C_FINARCH = "FINARCH>";
        
        const int C_TAM_CLUSTER = 1400;

        const int C_MAX_CONEXIONES_SERVER = 2; //en udp, si pasas esto en 0 acepta n conexiones

        //[DllImport("User32.dll")]
        //public static extern int MessageBox(int h, string m, string c, int type);

        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;
        /*
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        */
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

            //MessageBox(0, "hola mundo", "mensajin", 2); muestra un message y se para todo hasta que se toque un boton

            /*
            //para poder mostrar colores usando los comandos de vt100 que si tiene telnet
            var handle = GetStdHandle(STD_OUTPUT_HANDLE);
            uint mode;
            GetConsoleMode(handle, out mode);
            mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            SetConsoleMode(handle, mode);
            */

            Console.WriteLine("\x1b[93m TEST SERVER.\r\n");

            _obSocket = new Sockets.Sockets();
            _obSocket.Event_Socket += new Sockets.Sockets.Delegate_Socket_Event(EvSockets); 

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
                    if (_obSocket != null)
                    {
                        if ((_obSocket.ClientMode) || (_obSocket.ServerMode))
                        {
                            if (input.Equals("fin", StringComparison.OrdinalIgnoreCase))
                            {
                                _obSocket.DisconnectAll();
                                //break;
                            }
                            else if (input.Equals("send", StringComparison.OrdinalIgnoreCase))
                            {
                                //mando el archivo
                                EnviarArchivo(1);
                                _enviarArchivo = true;
                            }
                            else if (input.Equals("stop", StringComparison.OrdinalIgnoreCase))
                            {
                                _obSocket.KillServer();
                            }
                            else if (input.Equals("starttcp", StringComparison.OrdinalIgnoreCase))
                            {
                                Server();
                            }
                            else if(input.Equals("startudp", StringComparison.OrdinalIgnoreCase))
                            {
                                Server(false);
                            }
                            else if(input.Equals("newtcp", StringComparison.OrdinalIgnoreCase))
                            {
                                Cliente();
                            }
                            else if(input.Equals("newudp", StringComparison.OrdinalIgnoreCase))
                            {
                                ClienteUDP();
                            }
                            else
                            {
                                //<<<<<<<<<<<<le envio a todos un texto cualquiera>>>>>>>>>>>>>>>>
                                _obSocket.SendAll(input + "\r\n");
                            }
                        }
                        else
                        {
                            if (input.Equals("1", StringComparison.OrdinalIgnoreCase))
                            {
                                _modoServer = true;
                                Server();
                                _obSocket.ServerMode = _modoServer;
                                //break;
                            }
                            if (input.Equals("2", StringComparison.OrdinalIgnoreCase))
                            {
                                _modoServer = false;
                                Cliente();
                                _obSocket.ClientMode = true;
                                //break;
                            }
                            if (input.Equals("3", StringComparison.OrdinalIgnoreCase))
                            {
                                _modoServer = true;
                                Server(false);
                            }
                            if (input.Equals("4", StringComparison.OrdinalIgnoreCase))
                            {
                                _modoServer = false;
                                ClienteUDP();
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("no hay ni server ni cliente iniciado");
                    }

                }
            }

        }

        static void EvSockets(EventParameters ev)
        {
            
            switch (ev.GetEventType)
            {
                case EventParameters.EventType.NEW_CONNECTION:
                    Console.WriteLine (corchete(ev.GetConnectionNumber.ToString()) +  " conectado desde " + ev.GetIpOrigen);
                    _obSocket.Send("<SOS> " + ev.GetConnectionNumber.ToString(), ev.GetListIndex);
                    //_obSocket.Send("<SOS> " + ev.GetConnectionNumber.ToString(), 0);
                    break;

                case EventParameters.EventType.DATA_IN:
                    //DatosIn(indice, datos, true, ipOrigen);
                    DatosIn(ev.GetListIndex,ev.GetConnectionNumber, ev.GetData, true, ev.GetIpOrigen);
                    break;

                case EventParameters.EventType.ERROR:
                    //Console.WriteLine("error cliente");
                    Console.WriteLine(corchete(ev.GetConnectionNumber.ToString()) + " cod error " + ev.GetErrorCode + 
                        " en linea " +ev.GetLineNumberError.ToString()  + 
                        " descripcion " + ev.GetData);
                    break;

                case EventParameters.EventType.CONNECTION_LIMIT:
                    Console.WriteLine("<<LIMITE CONEXIONES>>");
                    break;

                case EventParameters.EventType.SERVER_STOP:
                    Console.WriteLine("<<server detenido>>");
                    break;

                case EventParameters.EventType.END_CONNECTION:
                    Console.WriteLine("<<conexión fin>> " + corchete(ev.GetConnectionNumber.ToString()) + " >>");
                    break;

                case EventParameters.EventType.SERVER_START:
                    Console.WriteLine("<<server iniciado>>");
                    break;

                case EventParameters.EventType.SEND_ARRAY_COMPLETE:
                    _obSocket.Send(C_FINARCH + "\r\n",ev.GetListIndex);
                    Console.WriteLine("envio ok");
                    break;

                case EventParameters.EventType.TIME_OUT:
                    Console.WriteLine("TIME OUT");
                    break;

                default:
                    //Console.WriteLine(corchete("Evento " + ev.GetEventType) + " " +ev.GetData);
                    break;
            }
        }

        static void Server(bool tcp =true)
        {
            string Message = "";

            if (tcp)
            {
                Console.WriteLine("modo server");
                Console.Title = "MODO SERVER TCP"; 
            }
            else
            {
                Console.WriteLine("modo server udp");
                Console.Title = "MODO SERVER UDP";
            }
                      
            _obSocket.ServerMode = true;
            _obSocket.SetServer(1492,Sockets.Sockets.C_DEFALT_CODEPAGE,tcp, C_MAX_CONEXIONES_SERVER);
            _obSocket.StartServer();

            if (Message != "")
            {
                Console.WriteLine(Message);
                return;
            }
        }

        static void DatosIn(int indice,int nConexion,string datos,bool server,string ipOrigen)
        {

            //if (_obSocket.tcp)
            //{
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
                        Console.WriteLine(corchete(nConexion.ToString()) + " " + datos);
                        
                        if (datos.Contains("kill\r\n"))
                        {
                            _obSocket.DisconnectConnectedClientToMe(nConexion);
                        }

                        if (datos.Contains("killall\r\n"))
                        {
                            _obSocket.DisconnectAllConnectedClientsToMe(); 
                        }

                        if (datos.Contains("detener\r\n"))
                        {
                            _obSocket.KillServer();
                            Console.WriteLine("SERVER DETENIDO");
                            _obSocket.StartServer();
                        }

                        if (datos.Contains("iniciar\r\n"))
                        {
                            _obSocket.StartServer();
                        }



                }
                    else
                    {
                        ArmarArchivo(datos);
                    }
                }
            //}
            //else
            //{
                //Console.WriteLine("[" + ipOrigen + "] " + datos);
                //if (_obSocket.ServerMode)
                //{
                    //EnviarRespuesta("UDP",indice);
                //}
            //}
        }

        static void EnviarRespuesta(string msg,int indice)
        {
            _obSocket.Send("server " + msg + " envia ok",indice);
        }

        static void Cliente()
        {
            string message = "";

            Console.WriteLine("modo cliente");
            Console.Title = "MODO CLIENTE";

            _obSocket.ClientMode = true;
            //_obSocket.SetCliente(1492, "127.0.0.1",5);
            //_obSocket.Connect();
            _obSocket.ConnectClient(1492, "127.0.0.1", 5);


            if (message != "")
            {
                Console.WriteLine(message);
            }

        }

        static void ClienteUDP()
        {
            //string Message = "";

            Console.WriteLine("modo cliente UDP");
            Console.Title = "MODO CLIENTE UDP";

            _obSocket.ClientMode = true;
            _obSocket.ConnectClient(1492, "127.0.0.1", 5,false);
            //_obSocket.SetCliente(1492, 0, "127.0.0.1",5, false);
            //_obSocket.Connect();


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

        static void EnviarArchivo(int conectionIndex)
        {
            byte[] memArrayFile;
            FileStream file;
            //string Err = "";

            //string vbcrlf = Convert.ToChar(10).ToString() + Convert.ToChar(13).ToString();

            try
            {
                file = new FileStream(C_ARCHIVO_ENVIAR, FileMode.Open);
                memArrayFile = new byte[file.Length];

                file.Read(memArrayFile, 0, (int)file.Length);
                file.Close();
                Console.WriteLine(memArrayFile.Length);

                _obSocket.Send(C_ENVACRH + "\r\n");
                _obSocket.SendArray(memArrayFile, C_TAM_CLUSTER, conectionIndex);


                file.Close();
                
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }

        static string corchete(string message)
        {
            return "[" + message + "]";
        }


    }
}
