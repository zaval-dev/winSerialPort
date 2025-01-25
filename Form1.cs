using System.Text;

namespace ChatApp
{
    public partial class Form1 : Form
    {
        TransmissionReception controlTxRx;
        delegate void mostrarOtroProceso(string mensaje);
        mostrarOtroProceso delegadoMostrar;

        public string portName = "COM1";

        public Form1()
        {
            InitializeComponent();
        }

        /**
         * Manejará el evento de llegada de mensaje
         * 
         * Invoke asegura que se realice desde el hilo principal (Interfaz gráfica) empleando al
         * delegado
         * **/
        private void controlTxRx_LlegoMensaje(object o, string mensajeRecibido)
        {
            Invoke(delegadoMostrar, mensajeRecibido);
        }

        private void mostrarMensaje(string mensajeRecibido)
        {
            Label messageLabel = new Label
            {
                Text = mensajeRecibido,
                AutoSize = true,
                MaximumSize = new Size(chatContainer.DisplayRectangle.Width - 20, 0),
                BackColor = Color.LightGreen,
                Padding = new Padding(5),
                Margin = new Padding(5),
                BorderStyle = BorderStyle.None,
            };

            int yOffset = chatContainer.Controls.Count > 0
                ? chatContainer.Controls[chatContainer.Controls.Count - 1].Bottom + 5
                : 10;

            messageLabel.Location = new Point(10, yOffset);
            chatContainer.Controls.Add(messageLabel);
            chatContainer.ScrollControlIntoView(messageLabel);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            controlTxRx = new TransmissionReception();
            controlTxRx.Inicialize(portName);
            controlTxRx.LlegoMensaje += new TransmissionReception.HandlerTxRx(controlTxRx_LlegoMensaje);
            delegadoMostrar = new mostrarOtroProceso(mostrarMensaje);
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            byte[] bytesMensajeEnviar = Encoding.UTF8.GetBytes(messageTextBox.Text);
            if (bytesMensajeEnviar.Length > 1019)
            {
                MessageBox.Show("El mensaje es demasiado largo. \nEl máximo permitido: 1019 caracteres.");
                return;
            }
            if (!string.IsNullOrWhiteSpace(messageTextBox.Text))
            {
                controlTxRx.inicializaEnvio(messageTextBox.Text);

                Label messageLabel = new Label
                {
                    Text = messageTextBox.Text,
                    AutoSize = true,
                    MaximumSize = new Size(chatContainer.DisplayRectangle.Width - 20, 0),
                    BackColor = Color.LightGreen,
                    Padding = new Padding(5),
                    Margin = new Padding(5),
                    BorderStyle = BorderStyle.None,
                };

                int yOffset = chatContainer.Controls.Count > 0
                ? chatContainer.Controls[chatContainer.Controls.Count - 1].Bottom + 5
                : 10;

                int xOffset = chatContainer.DisplayRectangle.Width - messageLabel.PreferredWidth - 10;

                messageLabel.Location = new Point(xOffset, yOffset);
                chatContainer.Controls.Add(messageLabel);
                chatContainer.ScrollControlIntoView(messageLabel);

                messageTextBox.Clear();
            }
        }

        private void messageTextBox_TextChanged(object sender, EventArgs e)
        {
            byte[] bytesToWrite = Encoding.UTF8.GetBytes(messageTextBox.Text);
            byteCounter.Text = $"{bytesToWrite.Length}/1019";
        }

        private void messageTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            byte[] bytesToWrite = Encoding.UTF8.GetBytes(messageTextBox.Text);
            if(bytesToWrite.Length >= 1019 && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }
    }
}
