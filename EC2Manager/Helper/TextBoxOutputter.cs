using System;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace EC2Manager
{
    public class TextBoxOutputter : TextWriter
    {
        private readonly TextBox textBox = null;

        public TextBoxOutputter(TextBox output)
        {
            textBox = output;
        }

        public override void Write(char value)
        {
            base.Write(value);
            textBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                textBox.AppendText(value.ToString());
                textBox.ScrollToEnd();
            }));
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}