namespace ChatApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            chatContainer = new Panel();
            messageTextBox = new TextBox();
            sendButton = new Button();
            panel1 = new Panel();
            byteCounter = new Label();
            btnEnviarArchivo = new Button();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // chatContainer
            // 
            chatContainer.AutoScroll = true;
            chatContainer.BackColor = Color.LightGray;
            chatContainer.Location = new Point(13, 25);
            chatContainer.Name = "chatContainer";
            chatContainer.Size = new Size(541, 378);
            chatContainer.TabIndex = 0;
            // 
            // messageTextBox
            // 
            messageTextBox.BackColor = Color.LightGray;
            messageTextBox.Location = new Point(13, 8);
            messageTextBox.Multiline = true;
            messageTextBox.Name = "messageTextBox";
            messageTextBox.Size = new Size(541, 84);
            messageTextBox.TabIndex = 1;
            messageTextBox.TextChanged += messageTextBox_TextChanged;
            messageTextBox.KeyPress += messageTextBox_KeyPress;
            // 
            // sendButton
            // 
            sendButton.Image = (Image)resources.GetObject("sendButton.Image");
            sendButton.Location = new Point(522, 98);
            sendButton.Name = "sendButton";
            sendButton.Size = new Size(32, 30);
            sendButton.TabIndex = 2;
            sendButton.UseVisualStyleBackColor = true;
            sendButton.Click += sendButton_Click;
            // 
            // panel1
            // 
            panel1.BackColor = Color.DarkSeaGreen;
            panel1.BackgroundImageLayout = ImageLayout.None;
            panel1.Controls.Add(btnEnviarArchivo);
            panel1.Controls.Add(byteCounter);
            panel1.Controls.Add(messageTextBox);
            panel1.Controls.Add(sendButton);
            panel1.Location = new Point(0, 422);
            panel1.Name = "panel1";
            panel1.Size = new Size(568, 136);
            panel1.TabIndex = 3;
            // 
            // byteCounter
            // 
            byteCounter.AutoSize = true;
            byteCounter.Location = new Point(13, 102);
            byteCounter.Name = "byteCounter";
            byteCounter.Size = new Size(42, 15);
            byteCounter.TabIndex = 3;
            byteCounter.Text = "0/1019";
            // 
            // btnEnviarArchivo
            // 
            btnEnviarArchivo.Image = (Image)resources.GetObject("btnEnviarArchivo.Image");
            btnEnviarArchivo.Location = new Point(481, 98);
            btnEnviarArchivo.Name = "btnEnviarArchivo";
            btnEnviarArchivo.Size = new Size(35, 30);
            btnEnviarArchivo.TabIndex = 4;
            btnEnviarArchivo.UseVisualStyleBackColor = true;
            btnEnviarArchivo.Click += btnEnviarArchivo_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.DarkSeaGreen;
            ClientSize = new Size(566, 558);
            Controls.Add(panel1);
            Controls.Add(chatContainer);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel chatContainer;
        private TextBox messageTextBox;
        private Button sendButton;
        private Panel panel1;
        private Label byteCounter;
        private Button btnEnviarArchivo;
    }
}
