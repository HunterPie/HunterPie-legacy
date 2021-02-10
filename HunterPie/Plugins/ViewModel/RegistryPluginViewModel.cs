using System;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HunterPie.Core;
using HunterPie.UI.Infrastructure;

namespace HunterPie.Plugins
{
    public class RegistryPluginViewModel : BaseViewModel, IPluginViewModel
    {
        private readonly PluginRegistryEntry entry;
        private readonly IPluginListProxy pluginList;
        private bool isBusy;
        private bool isFiltered;

        public RegistryPluginViewModel(PluginRegistryEntry entry, IPluginListProxy pluginList)
        {
            this.entry = entry;
            this.pluginList = pluginList;

            DownloadCommand = new ArglessRelayCommand(
                () => CanInstall && IsVersionOk, DownloadPlugin
            );
            DeleteCommand = new ArglessRelayCommand(
                () => CanDelete, Delete
            );
            DownloadImage();
        }

        public void Delete()
        {

            try
            {
                PluginManager.DeleteNonPreloadedPlugin(entry.InternalName);
                pluginList.UpdatePluginsArrangement();
                OnPropertyChanged(nameof(CanDelete));
                OnPropertyChanged(nameof(CanInstall));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error on delete", MessageBoxButton.OK, MessageBoxImage.Error);
                PluginManager.MarkForRemoval(entry.InternalName);
                pluginList.UpdatePluginsArrangement();
                OnPropertyChanged(nameof(CanDelete));
                OnPropertyChanged(nameof(CanInstall));
            }
        }

        private async void DownloadPlugin()
        {
            IsBusy = true;
            try
            {
                using var http = new HttpClient();
                var moduleContent = await http.GetStringAsync(entry.Module);

                var modulePath = await Hunterpie.Instance.InstallPlugin(moduleContent);
                // we don't actually care if this request is failed, nor interested in value, so we will not await it
                _ = PluginRegistryService.Instance.ReportInstall(entry.InternalName);
                DownloadReadme(modulePath);

                pluginList.UpdatePluginsArrangement();

                OnPropertyChanged(nameof(CanDelete));
                OnPropertyChanged(nameof(CanInstall));
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void DownloadImage()
        {
            try
            {
                using var http = new HttpClient();
                if (string.IsNullOrEmpty(entry.ImageUrl)) return;
                var bytes = await http.GetByteArrayAsync(entry.ImageUrl);
                Image = LoadImage(bytes);
                Dispatch(() =>
                {
                    OnPropertyChanged(nameof(Image));
                    OnPropertyChanged(nameof(HasImage));
                });
            }
            catch
            {
                // whatever
            }
        }

        private async void DownloadReadme(string modPath)
        {
            try
            {
                using var http = new HttpClient();
                var readme = await http.GetStringAsync(ReadmeUrl);
                File.WriteAllText(Path.Combine(modPath, "README.md"), readme);
            }
            catch
            {
                // readme can be missing, that's okay
            }
        }

        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public ImageSource Image { get; set; }

        public bool IsFiltered
        {
            get => isFiltered;
            set
            {
                if (value == isFiltered) return;
                isFiltered = value;
                OnPropertyChanged();
            }
        }

        public bool HasImage => Image != null;

        public bool IsVersionOk => PluginUpdate.IsVersionOk(entry.MinVersion);

        public bool CanToggle => false;
        public bool CanDelete => !IsBusy && PluginManager.IsInstalled(entry.InternalName) && !PluginManager.IsMarkedForRemoval(entry.InternalName);
        public bool CanInstall => !IsBusy && !PluginManager.IsInstalled(entry.InternalName);
        public bool CanRestore => false;
        public bool IsFailed => false;

        public ICommand DownloadCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand ToggleCommand => DisabledCommand.Instance;
        public ICommand RestoreCommand => DisabledCommand.Instance;

        public string ReadmeUrl => entry.Readme;
        public string InternalName => entry.InternalName;
        public string Name => entry.DisplayName;
        public string Description => entry.Description;
        public string SubText => GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_PLUGIN_SUBTITLE_ROW']")
            .Replace("{version}", entry.Version)
            .Replace("{author}", entry.Author)
            .Replace("{downloads}", entry.Downloads.ToString());

        public bool IsEnabled
        {
            get => false;
            set {}
        }

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                    OnPropertyChanged(nameof(CanInstall));
                    OnPropertyChanged(nameof(CanDelete));
                }
            }
        }

        public long SortValue => entry.Downloads;
    }
}
