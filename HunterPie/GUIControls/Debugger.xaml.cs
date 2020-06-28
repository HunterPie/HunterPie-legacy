using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using DispatcherOperation = System.Windows.Threading.DispatcherOperation;
using UserSettings = HunterPie.Core.UserSettings;


namespace HunterPie.Logger
{
    /// <summary>
    /// Interaction logic for Debugger.xaml
    /// </summary>
    public partial class Debugger : UserControl
    {
        // Colors
        private static object ERROR = "#FF6459";
        private static object WARN = "#FFC13D";
        private static object DISCORD = "#52A0FF";
        private static object NORMAL = "#FFFFFF";
        private static DispatcherOperation LastOperation;
        private static Debugger _Instance;
        public static Debugger Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new Debugger();
                }
                return _Instance;
            }
        }
        public Debugger() => InitializeComponent();

        public static Debugger InitializeDebugger() => Instance;

        public static void LoadNewColors()
        {
            ERROR = Application.Current.FindResource("DEBUGGER_ERROR");
            WARN = Application.Current.FindResource("DEBUGGER_WARN");
            DISCORD = Application.Current.FindResource("DEBUGGER_DISCORD");
            NORMAL = Application.Current.FindResource("DEBUGGER_LOG");
        }

        public static void Warn(object message) => PrintOnConsole(message?.ToString(), WARN);

        public static void Error(object message) => PrintOnConsole($"[ERROR] {message?.ToString()}", ERROR);

        public static void Log(object message) => PrintOnConsole($"[LOG] {message?.ToString()}", NORMAL);

        public static void Discord(object message) => PrintOnConsole($"[DISCORD] {message?.ToString()}", DISCORD);

        public static void Update(object message) => PrintOnConsole($"[UPDATE] {message?.ToString()}", NORMAL);

        public static void Debug(object message)
        {
            if (!UserSettings.PlayerConfig.HunterPie.Debug.ShowDebugMessages) return;
            PrintOnConsole($"[DEBUG] {message?.ToString()}", NORMAL, DispatcherPriority.ApplicationIdle);
        }

        private static void ScrollToEnd()
        {
            double ScrollableSize = _Instance.Console.ViewportHeight;
            double ScrollPosition = _Instance.Console.VerticalOffset;
            double ExtentHeight = _Instance.Console.ExtentHeight;
            if (ScrollableSize + ScrollPosition == ExtentHeight || ExtentHeight < ScrollableSize)
            {
                _Instance.Console.ScrollToEnd();
            }
        }

        private static void PrintOnConsole(string message, object color, DispatcherPriority priority = DispatcherPriority.Background)
        {
            DateTime TimeStamp = DateTime.Now;
            message = $"[{TimeStamp.ToLongTimeString()}] {message}\n";
            LastOperation = _Instance.Dispatcher.BeginInvoke(
                priority,
                new Action(() =>
                {
                    TextRange msg = new TextRange(_Instance.Console.Document.ContentEnd, _Instance.Console.Document.ContentEnd)
                    {
                        Text = message
                    };
                    msg.ApplyPropertyValue(TextElement.ForegroundProperty, color);
                    ScrollToEnd();
                })
            );
        }

        public static void DumpLog()
        {
            LastOperation.Wait();
            TextRange tr = new TextRange(_Instance.Console.Document.ContentStart, _Instance.Console.Document.ContentEnd);
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            IEnumerable<string> logFiles = Directory.EnumerateFiles(dir);
            if (logFiles.Count() >= 10)
            {
                foreach (string file in logFiles)
                {
                    File.Delete(Path.Combine(dir, file));
                }
            }
            File.WriteAllText(Path.Combine(dir, $"{DateTime.Now:dd\\-M\\-yyyy}_{DateTime.Now.GetHashCode()}_DEBUG-HunterPie.log"), tr.Text);
        }

    }
}
