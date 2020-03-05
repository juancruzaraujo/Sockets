using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sockets;

namespace TestSendArchivo
{

    internal class SocksCliente
    {
        /*
        string _Server;
        string _puerto;
        Cliente _obCliente;


        internal enum Eventos
        {
            Error,
            Envio_Completo,
            Datos_In,
            Nueva_Conexion,
            Conexion_Fin,
            Time_Out
        }

        internal delegate void Delegado_Socket_Cliente(int Indice, int Evento, bool Escuchando, long Size, string Datos);
        internal event Delegado_Socket_Cliente Eve_Socket_Cliente;
        private void Evento_Socket_Cliente(int Indice, int Evento, bool Escuchando, long Size, string Datos)
        {
            this.Eve_Socket_Cliente(Indice, Evento, Escuchando, Size, Datos);
        }

        internal SocksCliente()
        {
            string Mensaje="";

            _obCliente = new Cliente();
            _obCliente.CodePage(65001, ref Mensaje);
            
            _obCliente.Eve_Conexion_Fin += new Cliente.DelegadoConexion_Fin(Ev_ConexionFin);
            _obCliente.Eve_Conexion_OK += new Cliente.DelegadoConexion_ok(Ev_ConexionOK);
            _obCliente.Eve_DatosIn += new Cliente.DelegadoDatosIN(Ev_DatosIn);
            _obCliente.Eve_EnvioCompleto += new Cliente.DelegadoEnvioCompleto(Ev_EnvioCompleto);
            _obCliente.Eve_Error += new Cliente.DelegadoError(Ev_Error);
            _obCliente.Eve_TimeOut += new Cliente.DelegadoTimeOut(Ev_TimeOut);


        }

        private void Ev_TimeOut(int Indice)
        {
            
        }

        private void Ev_Error(int Indice, string Mensaje)
        {
            //throw new NotImplementedException();
            Console.WriteLine(Mensaje);
        }

        private void Ev_EnvioCompleto(int Indice, long DatosSend)
        {
            Eve_Socket_Cliente(Indice, Cliente.EVE_ENVIO_COMPLETO, false, 0, "");
            //return;
        }

        private void Ev_DatosIn(int Indice, string Mensaje)
        {
            //Console.WriteLine(Mensaje);
            //Eve_Socket_Servidor(indice, Servidor.EVE_DATOS_IN, false, 0, Datos, "");
            Eve_Socket_Cliente(Indice, Cliente.EVE_DATOS_IN, false, Mensaje.Length, Mensaje);
        }

        private void Ev_ConexionOK(int Indice)
        {
            Console.WriteLine("con ok");
        }

        private void Ev_ConexionFin(int Indice)
        {
            //throw new NotImplementedException();
        }

        internal void Conectar(string host,string puerto ,ref string Mensaje)
        {
            _Server = host;
            _puerto = puerto;
            _obCliente.Conectar(0, host, puerto, ref Mensaje);
            

            if (Mensaje != "")
            {
                Console.WriteLine(Mensaje);
            }
        }

        internal void EnviarDatos(string Datos)
        {
            string Mensaje = "";
            _obCliente.Enviar(Datos, ref Mensaje);
            if (Mensaje != "")
            {
                Console.WriteLine(Mensaje);
            }

        }

        internal void EnviarArray(byte[] memArray, int tamCluster, ref string Mensaje)
        {
            _obCliente.Enviar_ByteArray(memArray, tamCluster, ref Mensaje);
        }*/
    }
}
