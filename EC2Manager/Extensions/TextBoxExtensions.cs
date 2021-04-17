using System;
using System.Windows.Controls;

namespace EC2Manager.Extensions
{
    public static class TextBoxExtensions
    {
        public static void SetAsConsoleOutput(this TextBox textBox)
        {
            var outputter = new TextBoxOutputter(textBox);
            Console.SetOut(outputter);
        }
    }
}