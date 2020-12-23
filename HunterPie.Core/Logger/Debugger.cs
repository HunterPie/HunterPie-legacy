using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace HunterPie.Logger
{
    public struct LogString
    {
        public string Message { get; set; }
        public object Color { get; set; }
    }

    public class Debugger
    {
        // Colors
        public static object ERROR = "#FF6459";
        public static object WARN = "#FFC13D";
        public static object DISCORD = "#52A0FF";
        public static object MODULE = "#FFB0DB60";
        public static object NORMAL = "#FFFFFF";

        public static bool IsDebugEnabled { get; set; }

        public static readonly ObservableCollection<LogString> Logs = new ObservableCollection<LogString>();

        public static void Log(object message)
        {
            Write($"[LOG] {message?.ToString()}", NORMAL);
        }

        public static void Warn(object message)
        {
            Write(message?.ToString(), WARN);
        }

        public static void Error(object message)
        {
            Write($"[ERROR] {message?.ToString()}", ERROR);
        }

        public static void Module(object message, string modName = null)
        {
            Write($"[{modName?.ToUpper() ?? "MODULE"}] {message}", MODULE);
        }

        public static void Debug(object message)
        {
            if (IsDebugEnabled)
            {
                Write($"[DEBUG] {message?.ToString()}", NORMAL);
            }
        }

        public static void Discord(object message)
        {
            Write($"[DISCORD] {message?.ToString()}", DISCORD);
        }

        public static void LogObject(object obj)
        {
            Write(JsonConvert.SerializeObject(obj, Formatting.Indented), NORMAL);
        }

        public static void Clear()
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                Logs.Clear();
            }));
        }

        public static void Write(string message, object color)
        {
            DateTime timestamp = DateTime.Now;
            message = $"[{timestamp.ToLongTimeString()}] {message}";
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                Logs.Add(new LogString { Message = message, Color = color });
            }));
        }
    }
}
