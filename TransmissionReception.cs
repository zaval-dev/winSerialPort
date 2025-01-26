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

        /**
         * Declaraciones para implementar cola de mensajes
         * **/
        private readonly Queue<byte[]> colaMensajes = new Queue<byte[]>();
        private readonly Queue<byte[]> colaArchivos = new Queue<byte[]>();
        private readonly object lockCola = new object();
        private Thread hiloEnvio;
        private bool hiloActivo = true;

        private SerialPort puerto;
        private string mensajeEnviar;
        private string mensajeRecibido;

        private Boolean BufferSalidaVacio;
        
        private byte[] TramaEnvio;
        private byte[] TramaCabeceraEnvio;
        private byte[] tramaRelleno;

        private byte[] TramaRecibida;

        private int archivosRecibidos;

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

            archivosRecibidos = 0;
            IniciarHiloEnvio();
        }

        public void IniciarHiloEnvio()
        {
            hiloEnvio = new Thread(ProcesarColaEnvios);
            hiloEnvio.Start();
        }

        private void ProcesarColaEnvios()
        {
            while (hiloActivo)
            {
                byte[] trama = null;

                lock (lockCola)
                {
                    if(colaMensajes.Count > 0)
                    {
                        trama = colaMensajes.Dequeue();
                    }
                    else if (colaArchivos.Count > 0)
                    {
                        trama = colaArchivos.Dequeue();
                    }
                }

                if(trama != null)
                {
                    //MessageBox.Show("AQUÍ");
                    puerto.Write(trama, 0, trama.Length);
                    if(trama.Length < 1024)
                    {
                        puerto.Write(tramaRelleno, 0, 1024 - trama.Length);
                    }
                    //MessageBox.Show($"Trama enviada: {trama.Length-5} bytes netos.");
                }
                else
                {
                    Thread.Sleep(10);
                }
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

        public void inicializaEnvio(string mensaje, string tipo = "M")
        {
            if(mensaje.Length > 1019)
            {
                MessageBox.Show("El mensaje es demasiado largo. \nEl máximo permitido: 1019 caracteres.");
                return;
            }

            mensajeEnviar = mensaje;
            
            byte[] bytesMensaje = Encoding.UTF8.GetBytes(mensajeEnviar);
            string longitudMensaje = tipo + bytesMensaje.Length.ToString("D4");
            
            TramaEnvio = bytesMensaje;
            TramaCabeceraEnvio = Encoding.UTF8.GetBytes(longitudMensaje);
            byte[] tramaMensaje = TramaCabeceraEnvio.Concat(TramaEnvio).ToArray();

            lock (lockCola)
            {
                colaMensajes.Enqueue(tramaMensaje);
            }
            //MessageBox.Show("Mensaje añadido a la cola de mensajes.");
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
                        procesoRecibirMensaje = new Thread(recibiendoMensaje);
                        procesoRecibirMensaje.Start();
                        break;
                    case "D":
                        /**
                         * D representaría los metadatos (tamaño) del archivo a enviar.
                         * **/
                        Array.Copy(TramaRecibida, BufferMensajes, TramaRecibida.Length);
                        string CabeceraRecibida = Encoding.UTF8.GetString(BufferMensajes, 1, 4);
                        int LongitudMensajeRecibido = Convert.ToInt16(CabeceraRecibida);
                        string metadatosRecibidos = Encoding.UTF8.GetString(TramaRecibida, 5, LongitudMensajeRecibido);
                        InicioConstruirArchivo(long.Parse(metadatosRecibidos));
                        break;
                    case "A":
                        Array.Copy(TramaRecibida, BufferArchivos, TramaRecibida.Length);
                        ConstruirArchivo();
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

        /**
         * Lógica de envío y recepción de mensajes
         * **/
        //private void enviandoMensaje()
        //{
        //    puerto.Write(TramaCabeceraEnvio, 0, 5);
        //    puerto.Write(TramaEnvio, 0, TramaEnvio.Length);
        //    puerto.Write(tramaRelleno, 0, 1019 - TramaEnvio.Length);
        //}

        private void recibiendoMensaje()
        {
            //Capturamos la longitud del mensaje EJ: "M0015", pero están almacenados en byte por lo que tenemos que convertir
            string CabeceraRecibida = Encoding.UTF8.GetString(BufferMensajes, 1, 4);
            //Una vez capturada la longitud la pasamos a entero
            int LongitudMensajeRecibido = Convert.ToInt16(CabeceraRecibida);

            //Decodificamos el mensaje(En bytes) y lo convertimos a string
            mensajeRecibido = Encoding.UTF8.GetString(TramaRecibida, 5, LongitudMensajeRecibido);

            OnLlegoMensaje(); //Disparamos el evento de llegada de mensaje
        }

        public void IniciaEnvioArchivo(string nombre, long bytes)
        {
            FlujoArchivoEnviar = new FileStream(nombre, FileMode.Open, FileAccess.Read);
            LectorArchivo = new BinaryReader(FlujoArchivoEnviar);

            archivoEnviar.Nombre = nombre;
            archivoEnviar.Tamaño = FlujoArchivoEnviar.Length;
            archivoEnviar.Avance = 0;
            archivoEnviar.Num = 1;

            string metadatos = bytes.ToString();
            inicializaEnvio(metadatos, "D");
            procesoEnvioArchivo = new Thread(EnviarArchivo);
            procesoEnvioArchivo.Start();
        }

        private void EnviarArchivo()
        {
            byte[] TramaEnvioArchivo;
            byte[] TramCabaceraEnvioArchivo;
            byte[] tramaCompleta;

            TramaEnvioArchivo = new byte[1019];
            TramCabaceraEnvioArchivo = new byte[5];

            //ENVIAR LA PRIMERA TRAMA CON EL NOMBRE ARCHIVO;
            TramCabaceraEnvioArchivo = ASCIIEncoding.UTF8.GetBytes("AC001");
            //ENVIAR LAS TRAMAS DE INFORMACION
            TramCabaceraEnvioArchivo = ASCIIEncoding.UTF8.GetBytes("AI001");

            while (archivoEnviar.Avance <= archivoEnviar.Tamaño - 1019)
            {
                LectorArchivo.Read(TramaEnvioArchivo, 0, 1019);
                archivoEnviar.Avance = archivoEnviar.Avance + 1019;
                //envio 1de una trama llena de 1019 bytes del archivo
                while (BufferSalidaVacio == false)
                {//esperamos
                }
                //MessageBox.Show("avance = " + arhivoEnviar.Avance.ToString());
                //puerto.Write(TramCabaceraEnvioArchivo, 0, 5);
                //puerto.Write(TramaEnvioArchivo, 0, 1019);
                // puerto.Write(tramaRelleno, 0, 1019 - TramaEnvio.Length);
                
                tramaCompleta = TramCabaceraEnvioArchivo.Concat(TramaEnvioArchivo).ToArray();

                lock (lockCola)
                {
                    colaArchivos.Enqueue(tramaCompleta);
                }
            }

            int tamanito = Convert.ToInt16(archivoEnviar.Tamaño - archivoEnviar.Avance);
            LectorArchivo.Read(TramaEnvioArchivo, 0, tamanito);
            //envio de la ultima trama + relleno
            while (BufferSalidaVacio == false)
            {//esperamos
            }
            //MessageBox.Show("avance = " + arhivoEnviar.Avance.ToString() + " t= " + tamanito.ToString());
            //puerto.Write(TramCabaceraEnvioArchivo, 0, 5);
            //puerto.Write(TramaEnvioArchivo, 0, tamanito);
            //puerto.Write(tramaRelleno, 0, 1019 - tamanito);

            tramaCompleta = TramCabaceraEnvioArchivo.Concat(TramaEnvioArchivo).ToArray();
            lock (lockCola)
            {
                colaArchivos.Enqueue(tramaCompleta);
            }
            LectorArchivo.Close();
            FlujoArchivoEnviar.Close();
        }


        /***
         * Al recibir los metadatos se inicia la construcción del archivo
         * **/
        public void InicioConstruirArchivo(long bytes)
        {
            FlujoArchivoRecibir = new FileStream("E:\\imagenes_prueba\\nuevo" + (archivosRecibidos + 1).ToString(), FileMode.Create, FileAccess.Write);
            EscritorArchivo = new BinaryWriter(FlujoArchivoRecibir);
            archivoRecibir.Nombre = "E:\\imagenes_prueba\\nuevo" + (archivosRecibidos + 1).ToString();
            archivoRecibir.Num = archivosRecibidos + 1;
            archivoRecibir.Tamaño = bytes;
            archivoRecibir.Avance = 0;
        }

        private void ConstruirArchivo()
        {
            // debe realizarse en funcion del tamaño 1019 y la ultima será tamanito
            //Console.Write(archivoRecibir.Avance);
            if (archivoRecibir.Avance <= archivoRecibir.Tamaño - 1019)
            {
                EscritorArchivo.Write(BufferArchivos, 5, 1019);
                archivoRecibir.Avance = archivoRecibir.Avance + 1019;
            }
            else
            {
                int tamanito = Convert.ToInt16(archivoRecibir.Tamaño - archivoRecibir.Avance);
                EscritorArchivo.Write(BufferArchivos, 5, tamanito);
                EscritorArchivo.Close();
                FlujoArchivoRecibir.Close();
            }
        }

        /**
         * Bucle infinito que verifica el buffer
         * **/
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
