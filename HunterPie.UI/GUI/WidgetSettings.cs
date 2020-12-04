using System.Windows;

namespace HunterPie.GUI
{
    public class WidgetSettings : Window
    {
        public bool IsClosed = false;

        public new void Close()
        {
            IsClosed = true;
            base.Close();
        }
    }
}
