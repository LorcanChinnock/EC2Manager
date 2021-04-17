using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace EC2Manager.Extensions
{
    public static class ComboBoxExtensions
    {
        public static void AddItems(this ComboBox comboBox, IEnumerable<object> items)
        {
            for (var i = 0; i < items.Count(); i++)
            {
                comboBox.Items.Add(items.ElementAt(i));
            }
        }
    }
}