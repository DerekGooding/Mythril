using Myra.Graphics2D.UI;

namespace Mythril.UI
{
    public class LogWindow : Window
    {
        public TextBox LogTextBox { get; }

        public LogWindow()
        {
            Title = "Log";
            Width = 600;
            Height = 400;

            LogTextBox = new TextBox
            {
                Multiline = true,
                Enabled = false,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            Content = LogTextBox;
        }

        public void AddLog(string message)
        {
            LogTextBox.Text += message + "\n";
            // Optional: Scroll to the bottom
            // LogTextBox.SetVerticalScroll(int.MaxValue);
        }
    }
}
