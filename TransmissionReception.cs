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
        private string rutaDescarga = "E:\\PRUEBA\\1\\";

        private Boolean BufferSalidaVacio;
        
        private byte[] TramaEnvio;
        private byte[] TramaCabeceraEnvio;
        private byte[] tramaRelleno;

        private byte[] TramaRecibida;

        /**
         * Constructor
         * **/
        public TransmissionReception()
        {
            TramaEnvio = new byte[1024];
            TramaCabeceraEnvio = new byte[5];
            tramaRelleno = new byte[1024];

            TramaRecibida = new byte[1024];

            for(int i = 0; i <= 1023; i++)
            {
                tramaRelleno[i] = 64;
            }
        }
        public void modificarRutaDescarga(string nuevaRuta)
        {
            rutaDescarga = nuevaRuta;
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
            procesoEnvio = new Thread(enviandoMensaje);
            procesoEnvio.Start();
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
                        procesoRecibirMensaje = new Thread(recibiendoMensaje);
                        procesoRecibirMensaje.Start();
                        break;
                    /* caso "AC" = "F" 
                     * instanciar los flujo y binary de escritura = inicioConstruirArchivo
                     * */
                    case "D":
                        string CabeceraRecibida = Encoding.UTF8.GetString(TramaRecibida, 1, 4);
                        int LongitudMensajeRecibido = Convert.ToInt16(CabeceraRecibida);
                        string metadatosRecibidos = Encoding.UTF8.GetString(TramaRecibida, 5, LongitudMensajeRecibido);
                        InicioConstruirArchivo(metadatosRecibidos);
                        break;
                    case "A":
                        //procesoConstruyeArchivo = new Thread(ConstruirArchivo);
                        //.Start();
                        ConstruirArchivo();
                        break;
                    default:
                        MessageBox.Show("trama no reconocida");
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
        private void enviandoMensaje()
        {
            puerto.Write(TramaCabeceraEnvio, 0, 5);
            puerto.Write(TramaEnvio, 0, TramaEnvio.Length);
            puerto.Write(tramaRelleno, 0, 1019 - TramaEnvio.Length);
        }
        private void recibiendoMensaje()
        {
            //Capturamos la longitud del mensaje EJ: "M0015", pero están almacenados en byte por lo que tenemos que convertir
            string CabeceraRecibida = Encoding.UTF8.GetString(TramaRecibida, 1, 4);
            //Una vez capturada la longitud la pasamos a entero
            int LongitudMensajeRecibido = Convert.ToInt16(CabeceraRecibida);
            //Decodificamos el mensaje(En bytes) y lo convertimos a string
            mensajeRecibido = Encoding.UTF8.GetString(TramaRecibida, 5, LongitudMensajeRecibido);

            OnLlegoMensaje(); //Disparamos el evento de llegada de mensaje
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

        public void IniciaEnvioArchivo(string nombre)
        {
            // abrirlo , manejar las excepciones
            // leerlo en stream
            // iniciar una hebra de envio
            FlujoArchivoEnviar = new FileStream(nombre, FileMode.Open, FileAccess.Read);
            LectorArchivo = new BinaryReader(FlujoArchivoEnviar);

            archivoEnviar.Nombre = nombre;
            archivoEnviar.Tamaño = FlujoArchivoEnviar.Length;
            archivoEnviar.Avance = 0;
            archivoEnviar.Num = 1;

            int indiceUltimaBarra = nombre.LastIndexOf('\\');
            string nombreArchivo = nombre.Substring(indiceUltimaBarra + 1);

            string metadatos = nombreArchivo + "-" + FlujoArchivoEnviar.Length;
            MessageBox.Show(metadatos);
            inicializaEnvio(metadatos, "D");
            procesoEnvioArchivo = new Thread(EnviandoArchivo);
            procesoEnvioArchivo.Start();

        }

        private void EnviandoArchivo()
        {
            byte[] TramaEnvioArchivo;
            byte[] TramCabaceraEnvioArchivo;

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
                //envio de una trama llena de 1019 bytes del archivo
                while (BufferSalidaVacio == false)
                {//esperamos
                }
                //MessageBox.Show("avance = " + archivoEnviar.Avance.ToString());
                puerto.Write(TramCabaceraEnvioArchivo, 0, 5);
                puerto.Write(TramaEnvioArchivo, 0, 1019);
                // puerto.Write(tramaRelleno, 0, 1019 - TramaEnvio.Length);
            }
            int tamanito = Convert.ToInt16(archivoEnviar.Tamaño - archivoEnviar.Avance);
            LectorArchivo.Read(TramaEnvioArchivo, 0, tamanito);
            //envio de la ultima trama + relleno
            while (BufferSalidaVacio == false)
            {//esperamos
            }
            MessageBox.Show("avance = " + archivoEnviar.Avance.ToString() + " t= " + tamanito.ToString());
            puerto.Write(TramCabaceraEnvioArchivo, 0, 5);
            puerto.Write(TramaEnvioArchivo, 0, tamanito);
            puerto.Write(tramaRelleno, 0, 1019 - tamanito);

            LectorArchivo.Close();
            FlujoArchivoEnviar.Close();
        }

        //debe ser privado, y debe llamarse en una hebra al recibir la primera trama FILE
        // tamaño, nombre, numero identificacion....
        public void InicioConstruirArchivo(string metadatos)
        {
            string[] partes = metadatos.Split('-');
            string nombre = partes[0];
            string bytes = partes[1];
            FlujoArchivoRecibir = new FileStream(rutaDescarga + nombre, FileMode.Create, FileAccess.Write);
            EscritorArchivo = new BinaryWriter(FlujoArchivoRecibir);
            archivoRecibir.Nombre = nombre;
            archivoRecibir.Num = 1;
            archivoRecibir.Tamaño = long.Parse(bytes);
            archivoRecibir.Avance = 0;
        }

        private void ConstruirArchivo()
        {
            // debe realizarse en funcion del tamaño 1019 y la ultima será tamanito

            if (archivoRecibir.Avance <= archivoRecibir.Tamaño - 1019)
            {
                EscritorArchivo.Write(TramaRecibida, 5, 1019);
                archivoRecibir.Avance = archivoRecibir.Avance + 1019;
            }

            else
            {
                int tamanito = Convert.ToInt16(archivoRecibir.Tamaño - archivoRecibir.Avance);
                EscritorArchivo.Write(TramaRecibida, 5, tamanito);
                EscritorArchivo.Close();
                FlujoArchivoRecibir.Close();
            }

        }
    }
}
