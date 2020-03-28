using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using UserSettings = HunterPie.Core.UserSettings;


namespace HunterPie.Logger {
    /// <summary>
    /// Interaction logic for Debugger.xaml
    /// </summary>
    public partial class Debugger : UserControl {
        // Colors
        private static object ERROR = "#FF6459";
        private static object WARN = "#FFC13D";
        private static object DISCORD = "#52A0FF";
        private static object NORMAL = "#FFFFFF";

        private static Debugger _Instance;
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

        public static Debugger InitializeDebugger() {
            return Instance;
        }
        
        public static void LoadNewColors() {
            ERROR = Application.Current.FindResource("DEBUGGER_ERROR");
            WARN = Application.Current.FindResource("DEBUGGER_WARN");
            DISCORD = Application.Current.FindResource("DEBUGGER_DISCORD");
            NORMAL = Application.Current.FindResource("DEBUGGER_LOG");
        }

        public static void Warn(object message) {
            PrintOnConsole(message?.ToString(), WARN);
        }

        public static void Error(object message) {
            PrintOnConsole($"[ERROR] {message?.ToString()}", ERROR);
        }

        public static void Log(object message) {
            PrintOnConsole($"[LOG] {message?.ToString()}", NORMAL);
        }

        public static void Discord(object message) {
            PrintOnConsole($"[DISCORD] {message?.ToString()}", DISCORD);
        }

        public static void Update(object message) {
            PrintOnConsole($"[UPDATE] {message?.ToString()}", NORMAL);
        }

        public static void Debug(object message) {
            if (!UserSettings.PlayerConfig.HunterPie.Debug.ShowDebugMessages) return;
            PrintOnConsole($"[DEBUG] {message?.ToString()}", NORMAL);
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
            _Instance.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                new Action(() => {
                    TextRange msg = new TextRange(_Instance.Console.Document.ContentEnd, _Instance.Console.Document.ContentEnd) {
                        Text = message
                    };
                    msg.ApplyPropertyValue(TextElement.ForegroundProperty, color);
                    ScrollToEnd();
                })
            );
        }

    }
}
