using System;
using System.Windows.Controls;
using System.Windows.Documents;


namespace HunterPie.Logger {
    /// <summary>
    /// Interaction logic for Debugger.xaml
    /// </summary>
    public partial class Debugger : UserControl {
        // Colors
        private static string ERROR = "#FF6459";
        private static string WARN = "#FFC13D";
        private static string DISCORD = "#52A0FF";
        private static string NORMAL = "#FFFFFF";

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
            PrintOnConsole($"[ERROR] {message}", ERROR);
        }

        public static void Log(string message) {
            PrintOnConsole($"[LOG] {message}", NORMAL);
        }

        public static void Discord(string message) {
            PrintOnConsole($"[DISCORD] {message}", DISCORD);
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
            message = $"[{TimeStamp.ToLongTimeString()}] {message}\n";
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
