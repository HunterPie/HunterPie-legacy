using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HunterPie {
    /// <summary>
    /// Interaction logic for Debugger.xaml
    /// </summary>
    public partial class Debugger : UserControl {
        // Colors
        private static object ERROR = Brushes.Red;
        private static object WARN = Brushes.Yellow;
        private static object NORMAL = Brushes.White;

        public static Debugger _Instance;
        public static Debugger Instance {
            get {
                if (_Instance == null) {
                    _Instance = new Debugger();
                }
                return _Instance;
            }
        }
        public Debugger() {
            InitializeComponent();
        }

        public static void Warn(string message) {
            PrintOnConsole(message, WARN);
        }

        public static void Error(string message) {
            PrintOnConsole($"[WARNING] {message}", ERROR);
        }

        public static void Log(string message) {
            PrintOnConsole(message, NORMAL);
        }

        private static void ScrollToEnd() {
            double ScrollableSize = _Instance.Console.ViewportHeight;
            double ScrollPosition = _Instance.Console.VerticalOffset;
            double ExtentHeight = _Instance.Console.ExtentHeight;
            if (ScrollableSize + ScrollPosition == ExtentHeight || ExtentHeight < ScrollableSize) {
                _Instance.Console.ScrollToEnd();
            }
        }

        private static void PrintOnConsole(string message, object color) {
            DateTime TimeStamp = DateTime.Now;
            message = $"{TimeStamp:%H:%m} [HunterPie] {message}\n";
            _Instance.Console.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action( () => {
                    TextRange msg = new TextRange(_Instance.Console.Document.ContentEnd, _Instance.Console.Document.ContentEnd);
                    msg.Text = message;
                    msg.ApplyPropertyValue(TextElement.ForegroundProperty, color);
                    ScrollToEnd();
                })
            );
        }

    }
}
