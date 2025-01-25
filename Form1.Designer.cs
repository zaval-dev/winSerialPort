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
            chatContainer = new Panel();
            messageTextBox = new TextBox();
            sendButton = new Button();
            panel1 = new Panel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // chatContainer
            // 
            chatContainer.AutoScroll = true;
            chatContainer.BackColor = SystemColors.ButtonFace;
            chatContainer.Location = new Point(0, 0);
            chatContainer.Name = "chatContainer";
            chatContainer.Size = new Size(317, 297);
            chatContainer.TabIndex = 0;
            // 
            // messageTextBox
            // 
            messageTextBox.Location = new Point(13, 8);
            messageTextBox.Multiline = true;
            messageTextBox.Name = "messageTextBox";
            messageTextBox.Size = new Size(287, 84);
            messageTextBox.TabIndex = 1;
            // 
            // sendButton
            // 
            sendButton.Location = new Point(225, 98);
            sendButton.Name = "sendButton";
            sendButton.Size = new Size(75, 23);
            sendButton.TabIndex = 2;
            sendButton.Text = "Enviar";
            sendButton.UseVisualStyleBackColor = true;
            sendButton.Click += sendButton_Click;
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ButtonFace;
            panel1.Controls.Add(messageTextBox);
            panel1.Controls.Add(sendButton);
            panel1.Location = new Point(0, 295);
            panel1.Name = "panel1";
            panel1.Size = new Size(317, 131);
            panel1.TabIndex = 3;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ButtonHighlight;
            ClientSize = new Size(317, 427);
            Controls.Add(panel1);
            Controls.Add(chatContainer);
            Name = "Form1";
            Text = "Form1";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel chatContainer;
        private TextBox messageTextBox;
        private Button sendButton;
        private Panel panel1;
    }
}
