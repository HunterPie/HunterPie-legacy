using System;
using System.IO;
using System.Windows;
using HunterPie.Logger;

namespace HunterPie
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Debugger.Error(e.Exception);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(Path.Combine(dir, "stacktrace.log"), $"Application exit code: {e.ApplicationExitCode}\n{Environment.StackTrace}");
        }
    }
}
