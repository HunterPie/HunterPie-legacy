using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Update
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string BackgroundUrl
        {
            get { return (string)GetValue(BackgroundUrlProperty); }
            set { SetValue(BackgroundUrlProperty, value); }
        }
        public static readonly DependencyProperty BackgroundUrlProperty =
            DependencyProperty.Register("BackgroundUrl", typeof(string), typeof(MainWindow));

        public string Version
        {
            get { return (string)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }
        public static readonly DependencyProperty VersionProperty =
            DependencyProperty.Register("Version", typeof(string), typeof(MainWindow));

        public string UpdateMessage
        {
            get { return (string)GetValue(UpdateMessageProperty); }
            set { SetValue(UpdateMessageProperty, value); }
        }
        public static readonly DependencyProperty UpdateMessageProperty =
            DependencyProperty.Register("UpdateMessage", typeof(string), typeof(MainWindow));

        private Dictionary<string, string> arguments = new Dictionary<string, string>();

        public string FilesUpdated
        {
            get { return (string)GetValue(FilesUpdatedProperty); }
            set { SetValue(FilesUpdatedProperty, value); }
        }
        public static readonly DependencyProperty FilesUpdatedProperty =
            DependencyProperty.Register("FilesUpdated", typeof(string), typeof(MainWindow));

        private bool IsUpdating = false;

        public MainWindow()
        {
            GetRandomBackgroundImage();
            WindowBlur.SetIsEnabled(this, true);
            if (!GetAndParseArgs())
            {
                Close();
                return;
            }
            if (arguments.ContainsKey("version"))
            {
                Version = $"v{(arguments["version"])}";
            }
            InitializeComponent();
        }

        private async void UpdateFiles()
        {
            if (IsUpdating)
            {
                return;
            }
            arguments.TryGetValue("branch", out string branch);
            AsyncUpdate updater = new AsyncUpdate(branch);
            IsUpdating = true;
            updater.OnNewFileUpdate += (src, message) =>
            {
                FilesUpdated = $"Files: {updater.FilesUpdatedCounter}/{updater.DifferentFilesCounter}";
                UpdateMessage = message;
            };
            updater.OnUpdateFail += (_, __) =>
            {
                UpdateMessage = "Failed to update HunterPie";
                LaunchHunterPie(__);
            };
            updater.OnUpdateSuccess += (_, __) =>
            {
                LaunchHunterPie(__);
            };
            updater.OnDownloadProgressChanged += (object _, DownloadProgressChangedEventArgs args) =>
            {
                long kBytesReceived = args.BytesReceived / 1024;
                long kBytesToReceive = args.TotalBytesToReceive / 1024;

                UpdateProgressBar.Visibility = Visibility.Visible;
                ProgressBarText.Visibility = Visibility.Visible;

                if (kBytesReceived > UpdateProgressBar.Value && kBytesToReceive == UpdateProgressBar.Maximum)
                {
                    DoubleAnimation anim = new DoubleAnimation(UpdateProgressBar.Value, kBytesReceived, new Duration(TimeSpan.FromMilliseconds(200)));
                    UpdateProgressBar.BeginAnimation(ProgressBar.ValueProperty, anim);
                }
                else if (kBytesToReceive != UpdateProgressBar.Maximum)
                {
                    UpdateProgressBar.Value = 0;
                    UpdateProgressBar.Maximum = kBytesToReceive;
                    DoubleAnimation anim = new DoubleAnimation(0, kBytesReceived, new Duration(TimeSpan.FromMilliseconds(200)));
                    UpdateProgressBar.BeginAnimation(ProgressBar.ValueProperty, anim);
                }
            };

            await updater.Start();
        }

        private void LaunchHunterPie(UpdateFinished args)
        {
            Process HunterPie = new Process();
            HunterPie.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HunterPie.exe");
            HunterPie.StartInfo.Arguments = $"justUpdated={args.JustUpdated} latestVersion={args.IsLatestVersion}";
            HunterPie.Start();
            Close();
        }

        private bool GetAndParseArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            foreach (string argument in args)
            {
                string[] argv = ParseArgValue(argument);
                if (argv.Length > 1)
                {
                    arguments[argv[0]] = argv[1];
                }
            }
            if (args.Length > 1)
            {
                return true;
            } else
            {
                return MessageBox.Show("Update.exe isn't supposed to be executed by the user. Updating HunterPie manually will make it look for files in the master branch. Are you sure you want to update from master branch?", "Auto-Update", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
            }
        }

        private string[] ParseArgValue(string arg)
        {
            try
            {
                return arg.Split('=');
            } catch {
                return Array.Empty<string>();
            }
        }

        private void GetRandomBackgroundImage()
        {
            Random rng = new Random();
            BackgroundUrl = $"https://hunterpie.herokuapp.com/media/background_{rng.Next(0, 80)}.png";
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void OnImageLoaded(object sender, EventArgs e)
        {
            UpdateFiles();
        }

        private void OnImageFail(object sender, ExceptionEventArgs e)
        {
            UpdateFiles();
        }
    }
}
