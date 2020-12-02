using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HunterPie.Logger;

namespace HunterPie.GUIControls
{
    /// <summary>
    /// Interaction logic for Debugger.xaml
    /// </summary>
    public partial class DebuggerControl : UserControl
    {
        private static DebuggerControl _Instance;
        public static DebuggerControl Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new DebuggerControl();
                }
                return _Instance;
            }
        }
        ScrollViewer scroll { get; set; }
        public DebuggerControl()
        {
            InitializeComponent();
            
        }

        public static DebuggerControl InitializeDebugger() {
            return Instance;
        }

        public static void LoadNewColors()
        {
            Debugger.ERROR = Application.Current.FindResource("DEBUGGER_ERROR");
            Debugger.WARN = Application.Current.FindResource("DEBUGGER_WARN");
            Debugger.DISCORD = Application.Current.FindResource("DEBUGGER_DISCORD");
            Debugger.NORMAL = Application.Current.FindResource("DEBUGGER_LOG");
        }

        private static void ScrollToEnd()
        {

            if (Instance.scroll is null)
            {
                return;
            }

            double ScrollableSize = Instance.scroll.ViewportHeight;
            double ScrollPosition = Instance.scroll.VerticalOffset;
            double ExtentHeight = Instance.scroll.ExtentHeight;
            if (ScrollableSize + ScrollPosition == ExtentHeight || ExtentHeight < ScrollableSize)
            {
                Instance.scroll.ScrollToEnd();
            }
        }

        public static void DumpLog()
        {
            
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            IEnumerable<string> logFiles = Directory.EnumerateFiles(dir);
            if (logFiles.Count() >= 10)
            {
                File.Delete(Path.Combine(dir, logFiles.First()));
            }
            File.WriteAllLines(Path.Combine(dir, $"{DateTime.Now:dd\\-M\\-yyyy}_{DateTime.Now.GetHashCode()}_DEBUG-HunterPie.log"), Debugger.Logs.Select(l => l.Message).ToArray());
            
        }

        private void OnClearConsoleButtonClick(object sender, RoutedEventArgs e)
        {
            Debugger.Clear();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _Instance.console.ItemsSource = Debugger.Logs;
            scroll = Instance.console.Template.FindName("scroll", Instance.console) as ScrollViewer;
        }

        public static void WriteStacktrace()
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(Path.Combine(dir, "stacktrace.log"), $"Application exit code: {Environment.ExitCode}\n{Environment.StackTrace}");
        }

        private void scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollToEnd();

        }
    }
}
