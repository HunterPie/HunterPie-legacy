using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using HunterPie.Memory;


namespace HunterPie.GUI {
    public class WidgetSettings : Window {
        public bool IsClosed = false;

        public new void Close() {
            this.IsClosed = true;
            base.Close();
        }
    }
}
