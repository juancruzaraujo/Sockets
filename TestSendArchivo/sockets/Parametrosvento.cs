using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sockets
{
    public class Parametrosvento
    {
        private int _numConexion;
        private int _indiceLista;
        private TipoEvento _evento;
        private bool _escuchando;
        private long _size;
        private string _datos;
        private long _posicion;
        private string _ipOrigen;
        private string _ipDestino;
        private int _codigoError;
        private int _lineNumberError;

        /// <summary>
        /// Numero de evento que se dispara
        /// </summary>
        public enum TipoEvento
        {
            ERROR = 0,
            ENVIO_COMPLETO = 1,
            DATOS_IN = 2,
            /// <summary>
            /// la comunucación con el cliente ya esta establecida
            /// </summary>
            NUEVA_CONEXION = 3,
            CONEXION_FIN = 4,
            /// <summary>
            /// acepta conexión y se obtiene el ip del cliente, pero todavía no establece la comunucación
            /// </summary>
            ACEPTAR_CONEXION = 5,
            ESPERA_CONEXION = 6,
            POSICION_ENVIO = 7,
            CONEXION_OK = 8,
            TIME_OUT = 9,
            SERVER_INICIADO = 10,
            LIMITE_CONEXIONES = 11,
            SERVER_DETENIDO = 12,
            SEND_ARRAY_COMPLETE = 13
        };

        internal Parametrosvento SetNumConexion(int numConexion)
        {
            _numConexion = numConexion;
            return this;
        }

        internal Parametrosvento SetIndiceLista(int indiceLista)
        {
            _indiceLista = indiceLista;
            return this;
        }

        internal Parametrosvento SetEvento(TipoEvento evento)
        {
            _evento = evento;
            return this;
        }

        internal Parametrosvento SetEscuchando(bool escuchando)
        {
            _escuchando = escuchando;
            return this;
        }

        internal Parametrosvento SetSize(long size)
        {
            _size = size;
            return this;
        }

        internal Parametrosvento SetDatos(string datos)
        {
            _datos = datos;
            return this;
        }

        internal Parametrosvento SetPosicion(long posicion)
        {
            _posicion = posicion;
            return this;
        }

        internal Parametrosvento SetIpOrigen(string ipOrigen)
        {
            _ipOrigen = ipOrigen;
            return this;
        }

        internal Parametrosvento SetIpDestino(string ipDestino)
        {
            _ipDestino = ipDestino;
            return this;
        }

        internal Parametrosvento SetCodError(int codError)
        {
            _codigoError = codError;
            return this;
        }

        internal Parametrosvento SetLineNumberError(int lineNumber)
        {
            _lineNumberError = lineNumber;
            return this;
        }

        /// <summary>
        /// retorna el numero de conexión actual del cliente conectado
        /// </summary>
        public int GetNumConexion
        {
            get
            {
                return _numConexion;
            }
        }

        public int GetIndiceLista
        {
            get
            {
                return _indiceLista;
            }
        }

        public TipoEvento GetEvento
        {
            get
            {
                return _evento;
            }
        }

        public bool GetEscuchando
        {
            get
            {
                return _escuchando;
            }
        }

        public long GetSize
        {
            get
            {
                return _size;
            }
        }

        public string GetDatos
        {
            get
            {
                return _datos;
            }
        }

        public long GetPosicion
        {
            get
            {
                return _posicion;
            }
        }

        public string GetIpOrigen
        {
            get
            {
                return _ipOrigen;
            }
        }

        public string GetIpDestino
        {
            get
            {
                return _ipDestino;
            }
        }

        public int GetCodError
        {
            get
            {
                return _codigoError;
            }
        }

        public int GetLineNumberError
        {
            get
            {
                return _lineNumberError;
            }
        }

        public Parametrosvento LimpiarValores()
        {
            //ver si funciona y no rompe todo
            Parametrosvento aux = new Parametrosvento();
            return aux;
        }
    }
}
