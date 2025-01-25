namespace ChatApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(messageTextBox.Text))
            {
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
    }
}
