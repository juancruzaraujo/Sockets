using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sockets;

namespace TestSendArchivo
{
    internal class SockServer
    {
        /*
        int _puerto;
        Servidor _obServer;

        
        //internal const int EVE_ERROR = 0;
        //internal const int EVE_ENVIO_COMPLETO = 1;
        //internal const int EVE_DATOS_IN = 2;
        //internal const int EVE_NUEVA_CONEXION = 3;
        //internal const int EVE_CONEXION_FIN = 4;
        //internal const int EVE_ACEPTAR_CONEXION = 5;
        //internal const int EVE_ESPERA_CONEXION = 6;
        
        internal enum Eventos
        {
            Error,
            Envio_Completo,
            Datos_In,
            Nueva_Conexion,
            Conexion_Fin,
            Aceptar_Conexion,
            Espera_Conexion
        }

        internal delegate void Delegado_Sockt_Servidor(int Indice, int Evento, bool Escuchando, long Size, string Datos, string IpOrigen);
        internal event Delegado_Sockt_Servidor Eve_Socket_Servidor;
        private void Evento_Sokct_Servidor(int Indice, int Evento, bool Escuchando, long Size, string Datos, string IpOrigen)
        {
            this.Eve_Socket_Servidor(Indice, Evento, Escuchando, Size, Datos, IpOrigen);
        }


        internal SockServer(int puerto)
        {
            _puerto = puerto;
            
            
        }

        internal void IniciarServer(ref string Mensaje)
        {
            _obServer = new Servidor(_puerto, 65001, 0, ref Mensaje);
            
            _obServer.Eve_AceptarConexion += new Servidor.DelegadoAceptarConexion(Ev_AceptarConexion);
            _obServer.Eve_DatosIn += new Servidor.DelegadoDatosIn(Ev_DatosIn);
            _obServer.Eve_Error += new Servidor.DelegadoError(Ev_Error);
            _obServer.Eve_Espera_Conexion += new Servidor.DelegadoEsperaConexion(Ev_EsperaConexion);
            _obServer.Eve_FinConexion += new Servidor.DelegadoFinConexion(Ev_FinConexion);
            _obServer.Eve_NuevaConexion += new Servidor.DelegadoNuevaConexion(Ev_NuevaConexion);
            _obServer.Eve_Envio_Completo += new Servidor.Delegado_Envio_Completo(Ev_EnvioCompleto);

            if (Mensaje == "")
            {
                _obServer.Iniciar(ref Mensaje);
                if (Mensaje != "")
                {
                    return; //algo malio sal
                }
            }

        }

        private void Ev_EnvioCompleto(int Indice, long Size)
        {
            return;
        }

        private void Ev_NuevaConexion(int Indice)
        {
            Eve_Socket_Servidor(Indice, Servidor.EVE_NUEVA_CONEXION, false, 0, "", "");
        }

        private void Ev_FinConexion(int Indice)
        {
            return;
        }

        private void Ev_EsperaConexion(int Indice, bool Escuchando)
        {
            return;
        }

        private void Ev_Error(int indice, string ErrDescrip)
        {
            return;
        }

        private void Ev_DatosIn(int indice, string Datos)
        {
            Eve_Socket_Servidor(indice, Servidor.EVE_DATOS_IN, false, 0, Datos, "");
        }

        private void Ev_AceptarConexion(int Indice, string IpOrigen)
        {
            Eve_Socket_Servidor(Indice, Servidor.EVE_ACEPTAR_CONEXION, false, 0, "", IpOrigen);
        }

        internal void EnviarDatos(string Datos)
        {
            string Mensaje="";
            _obServer.Enviar(Datos, ref Mensaje);
            if (Mensaje != "")
            {
                Console.WriteLine(Mensaje);
            }

        }

        internal void EnviarArray(byte[] memArray,int tamCluster,ref string Mensaje)
        {
            _obServer.Enviar_ByteArray(memArray, tamCluster, ref Mensaje);
        }*/
    }
}
