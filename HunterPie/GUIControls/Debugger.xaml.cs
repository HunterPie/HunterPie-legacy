using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
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
        private static object MODULE = "#FFB0DB60";
        private static object NORMAL = "#FFFFFF";

        private static ObservableCollection<LogString> logs = new ObservableCollection<LogString>();

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
        public Debugger()
        {
            InitializeComponent();
            
        }

        public static Debugger InitializeDebugger() {
            return Instance;
       }

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

        public static void Module(object message, string modName = null) => PrintOnConsole($"[{modName?.ToUpper() ?? "MODULE"}] {message?.ToString()}", MODULE);

        public static void Benchmark(object message) => PrintOnConsole(message?.ToString(), WARN);

        public static void Debug(object message)
        {
            if (!UserSettings.PlayerConfig.HunterPie.Debug.ShowDebugMessages) return;
            PrintOnConsole($"[DEBUG] {message?.ToString()}", NORMAL, DispatcherPriority.ApplicationIdle);
        }

        private static void ScrollToEnd()
        {
            double ScrollableSize = Instance.scroll.ViewportHeight;
            double ScrollPosition = Instance.scroll.VerticalOffset;
            double ExtentHeight = Instance.scroll.ExtentHeight;
            if (ScrollableSize + ScrollPosition == ExtentHeight || ExtentHeight < ScrollableSize)
            {
                Instance.scroll.ScrollToEnd();
            }
        }

        private static void PrintOnConsole(string message, object color, DispatcherPriority priority = DispatcherPriority.Background)
        {
            DateTime TimeStamp = DateTime.Now;
            message = $"[{TimeStamp.ToLongTimeString()}] {message}";
            LogString msg = new LogString()
            {
                Text = message,
                Color = color
            };
            LastOperation = Instance.Dispatcher.BeginInvoke(priority, new Action(() =>
            {
                logs.Add(msg);
                ScrollToEnd();
            }));
        }

        public static void DumpLog()
        {
            
            LastOperation.Wait();
            
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
            File.WriteAllLines(Path.Combine(dir, $"{DateTime.Now:dd\\-M\\-yyyy}_{DateTime.Now.GetHashCode()}_DEBUG-HunterPie.log"), logs.Select(l => l.Text).ToArray());
            
        }

        private void OnClearConsoleButtonClick(object sender, RoutedEventArgs e)
        {
            logs.Clear();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _Instance.console.ItemsSource = logs;
        }
    }
    class LogString
    {
        public string Text { get; set; }
        public object Color { get; set; }
    }
}
