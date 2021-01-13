using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Update
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            string logFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            if (!Directory.Exists(logFolderPath))
                Directory.CreateDirectory(logFolderPath);

            File.WriteAllText(Path.Combine(logFolderPath, "UpdateExceptions.log"), e.Exception.ToString());

        }
    }
}
