using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp
{
    internal class TransmissionReception
    {
        public delegate void HandlerTxRx(Object o, string mensajeRecibido);
        public event HandlerTxRx LlegoMensaje;
        
        private FileSent archivoEnviar;
        private FileStream FlujoArchivoEnviar;
        private BinaryReader LectorArchivo;

        private FileSent archivoRecibir;
        private FileStream FlujoArchivoRecibir;
        private BinaryWriter EscritorArchivo;

        Thread procesoEnvio;
        Thread procesoVerificaSalida;
        Thread procesoRecibirMensaje;

        Thread procesoEnvioArchivo;
        Thread procesoConstruyeArchivo;

        private SerialPort puerto;
        private string mensajeEnviar;
        private string mensajeRecibido;

        private Boolean BufferSalidaVacio;
        
        private byte[] TramaEnvio;
        private byte[] TramaCabeceraEnvio;
        private byte[] tramaRelleno;

        private byte[] TramaRecibida;

        /**
         * Buffers que almacenarán los valores de Trama recibida de forma independiente 
         * para mensajes y archivos
         * **/
        private byte[] BufferMensajes;
        private byte[] BufferArchivos;

        /**
         * Constructor
         * **/
        public TransmissionReception()
        {
            TramaEnvio = new byte[1024];
            TramaCabeceraEnvio = new byte[5];
            tramaRelleno = new byte[1024];

            TramaRecibida = new byte[1024];
            BufferArchivos = new byte[1024];
            BufferMensajes = new byte[1024];

            for(int i = 0; i <= 1023; i++)
            {
                tramaRelleno[i] = 64;
            }
        }

        public void Inicialize(string portName)
        {
            try
            {
                puerto = new SerialPort(portName, 57600, Parity.Even, 8, StopBits.Two);
                /**
                 * Se requiere de 1024 bits disponibles para disparar el evento DataReceived
                 * **/
                puerto.ReceivedBytesThreshold = 1024;
                puerto.DataReceived += new SerialDataReceivedEventHandler(DataReceivedEvent);
                puerto.Open();

                BufferSalidaVacio = true;
                procesoVerificaSalida = new Thread(verificaBufferSalida);
                procesoVerificaSalida.Start();

                archivoEnviar = new FileSent();
                archivoRecibir = new FileSent();

                MessageBox.Show("Puerto: " + puerto.PortName + " abierto correctamente.");
            }
            catch (Exception e)
            {
                MessageBox.Show("Error al intentar abrir el puerto." + "\n" + e.Message);
            }
        }

        private void DataReceivedEvent(object o, SerialDataReceivedEventArgs sd)
        {
            if(puerto.BytesToRead >= 1024)
            {
                /**
                 * Se almacena todo el buffer de entrada del puerto 
                 * y se almacena en TramaRecibida
                 * **/
                puerto.Read(TramaRecibida, 0, 1024);

                /**
                 * Capturamos el primer digito de la cabecera
                 * Determinamos si es un archivo(A) o mensaje(M)
                 * **/
                string TAREA = ASCIIEncoding.UTF8.GetString(TramaRecibida, 0, 1);

                switch(TAREA)
                {
                    case "M":
                        Array.Copy(TramaRecibida, BufferMensajes, TramaRecibida.Length);
                        procesoRecibirMensaje = new Thread(RecibiendoMensaje);
                        procesoRecibirMensaje.Start();
                        break;
                    case "A":
                        Array.Copy(TramaRecibida, BufferArchivos, TramaRecibida.Length);
                        //procesoConstruyeArchivo = new Thread();
                        //procesoConstruyeArchivo.Start();
                        break;
                    default:
                        MessageBox.Show("Trama no reconocida.");
                        break;
                }
            }
        }

        protected virtual void OnLlegoMensaje()
        {
            if(LlegoMensaje != null)
            {
                LlegoMensaje(this, mensajeRecibido);
            }
        }

        private void RecibiendoMensaje()
        {
            //Capturamos la longitud del mensaje EJ: "M0015", pero están almacenados en byte por lo que tenemos que convertir
            string CabeceraRecibida = ASCIIEncoding.UTF8.GetString(BufferMensajes, 1, 4);
            //Una vez capturada la longitud la pasamos a entero
            int LongitudMensajeRecibido = Convert.ToInt16(CabeceraRecibida);

            //Decodificamos el mensaje(En bytes) y lo convertimos a string
            mensajeRecibido = ASCIIEncoding.UTF8.GetString(TramaRecibida, 5, LongitudMensajeRecibido);

            OnLlegoMensaje(); //Disparamos el evento de llegada de mensaje
        }

        public void verificaBufferSalida()
        {
            while (true)
            {
                if (puerto.BytesToWrite > 0)
                {
                    BufferSalidaVacio = false;
                }
                else
                {
                    BufferSalidaVacio = true;
                }
            }
        }
    }
}
