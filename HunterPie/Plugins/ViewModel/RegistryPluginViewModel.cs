using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HunterPie.Core;
using HunterPie.Core.Readme;
using HunterPie.UI.Infrastructure;

namespace HunterPie.Plugins
{
    public class RegistryPluginViewModel : BaseViewModel, IPluginViewModel
    {
        private readonly PluginRegistryEntry entry;
        private IPluginListProxy pluginList;
        private readonly ReadmeService readmeService;
        private readonly HttpClient http;

        private PluginActionViewModel openDirAction;

        private bool isBusy;
        private bool isFiltered;

        public RegistryPluginViewModel(PluginRegistryEntry entry, IPluginListProxy pluginList, ReadmeService readmeService, HttpClient http)
        {
            this.entry = entry;
            this.pluginList = pluginList;
            this.readmeService = readmeService;
            this.http = http;

            DownloadCommand = new ArglessRelayCommand(
                () => CanInstall && IsVersionOk, DownloadPlugin
            );
            DeleteCommand = new ArglessRelayCommand(
                () => CanDelete, Delete
            );
            DownloadImage();
            LastUpdateLong = PluginViewModelHelper.FormatLongDate(entry.ReleaseDate);
            LastUpdateShort = PluginViewModelHelper.FormatShortDate(entry.ReleaseDate);
            Actions = new ObservableCollection<PluginActionViewModel>(GetActions());
        }

        public void Delete()
        {

            try
            {
                PluginManager.DeleteNonPreloadedPlugin(entry.InternalName);
                // when plugin is removed, we shouldn't be able to open it's folder anymore
                if (openDirAction != null)
                {
                    this.Actions.Remove(openDirAction);
                    openDirAction = null;
                }
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
                var moduleContent = await http.GetStringAsync(entry.Module);

                var modulePath = await Hunterpie.Instance.InstallPlugin(moduleContent);
#if !DEBUG
                // we don't actually care if this request is failed, nor interested in value, so we will not await it
                _ = PluginRegistryService.Instance.ReportInstall(entry.InternalName);
#endif
                DownloadReadme(modulePath);

                // when plugin is installed, we can open it's directory
                openDirAction = new PluginActionViewModel(
                    GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_PLUGIN_DIRECTORY']"),
                    Application.Current.FindResource("ICON_FOLDER") as ImageSource,
                    new ArglessRelayCommand(() => Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        "modules", this.InternalName)))
                );
                this.Actions.Insert(0, openDirAction);

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
                await ReadmeDownloadHelper.DownloadReadme(readmeService, modPath, ReadmeUrl, Image as BitmapImage);
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
        public ICommand ToggleCommand { get; private set; } = DisabledCommand.Instance;
        public ICommand RestoreCommand { get; private set; } = DisabledCommand.Instance;

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

        public ObservableCollection<PluginActionViewModel> Actions { get; }

        public string LastUpdateShort { get; }
        public string LastUpdateLong { get; }
        private IEnumerable<PluginActionViewModel> GetActions()
        {
            // custom links
            if (entry.Links != null)
            {
                foreach (var action in entry.Links.Select(PluginViewModelHelper.ParseLink))
                {
                    yield return action;
                }
            }
            else if (PluginViewModelHelper.TryParseGithubLink(entry.Readme, out var action))
            {
                yield return action;
            }
        }


        private bool isDisposed = false;

        public void Dispose()
        {
            if (isDisposed) return;

            DownloadCommand = ToggleCommand = RestoreCommand = DeleteCommand = null;
            pluginList = null;
            isDisposed = true;
            // don't dispose http, since we didn't create it
            GC.SuppressFinalize(this);
        }

        ~RegistryPluginViewModel() => Dispose();
    }
}
