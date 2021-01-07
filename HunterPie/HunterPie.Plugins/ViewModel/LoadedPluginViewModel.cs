using System;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HunterPie.Core;
using HunterPie.Logger;
using HunterPie.UI.Infrastructure;

namespace HunterPie.Plugins
{
    public class LoadedPluginViewModel : BaseViewModel, IPluginViewModel
    {
        private readonly PluginEntry plugin;
        private readonly IPluginListProxy pluginList;

        public LoadedPluginViewModel(PluginEntry plugin, IPluginListProxy pluginList)
        {
            this.plugin = plugin;
            this.pluginList = pluginList;

            ToggleCommand = new RelayCommand(
                _ => CanToggle,
                val => IsEnabled = (bool)val
            );
            DeleteCommand = new ArglessRelayCommand(
                () => CanDelete, Delete
            );
            RestoreCommand = new ArglessRelayCommand(
                () => CanRestore, Restore
            );
            pluginList.RegistryPluginsLoaded += OnRegistryPluginsLoaded;
            GetImage();
        }

        private void OnRegistryPluginsLoaded(object sender, EventArgs e)
        {
            GetImage();
        }

        public void Delete()
        {
            try
            {
                IsEnabled = false;

                if (plugin.IsFailed)
                {
                    // if plugin loading failed, it can already been loaded into AppDomain, so it isn't safe to delete it right away
                    // because dll's may be in use
                    PluginManager.MarkForRemoval(InternalName);
                }
                else if (plugin.IsLoaded && plugin.Package.HasValue)
                {
                    PluginManager.UnloadPlugin(plugin.Package.Value.plugin);
                    PluginManager.MarkForRemoval(InternalName);
                }
                else
                {
                    try
                    {
                        PluginManager.DeleteNonPreloadedPlugin(InternalName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        Debugger.Warn(ex);
                        PluginManager.MarkForRemoval(InternalName);
                    }
                }

                pluginList.UpdatePluginsArrangement();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Restore()
        {
            try
            {
                PluginManager.MarkForRemoval(InternalName, false);
                pluginList.UpdatePluginsArrangement();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        public bool CanToggle => plugin.IsLoaded && !PluginManager.IsMarkedForRemoval(InternalName);
        public bool CanDelete => !PluginManager.IsMarkedForRemoval(InternalName);
        public bool CanInstall => false;
        public bool CanRestore => PluginManager.IsMarkedForRemoval(InternalName);

        public bool IsVersionOk => true;
        public bool IsFailed => plugin.IsFailed;

        public ICommand DownloadCommand => DisabledCommand.Instance;
        public ICommand DeleteCommand { get; }
        public ICommand ToggleCommand { get; }
        public ICommand RestoreCommand { get; }

        public string InternalName => plugin.PluginInformation.Name;
        public ImageSource Image { get; set; }

        public bool HasImage => Image != null;
        public string Name => string.IsNullOrEmpty(plugin.PluginInformation.DisplayName)
            ? plugin.PluginInformation.Name
            : plugin.PluginInformation.DisplayName;
        public string ReadmeUrl
        {
            get
            {
                var fsPath = Path.Combine(plugin.RootPath, "readme.md");
                if (File.Exists(fsPath))
                {
                    return "file://" + fsPath;
                }

                // if readme isn't found locally, try to use to one from registry
                return pluginList.TryGetRegistryEntry(plugin.PluginInformation.Name)?.Readme;
            }
        }

        public string Description => plugin.PluginInformation.Description;
        public string SubText => GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_PLUGIN_SUBTITLE_ROW']")
            .Replace("{version}", plugin.PluginInformation.Version)
            .Replace("{author}", plugin.PluginInformation.Author);

        public bool IsEnabled
        {
            get => plugin.Package?.settings.IsEnabled ?? false;
            set
            {
                if (!plugin.Package.HasValue) return;
                plugin.Package.Value.settings.IsEnabled = value;
                PluginManager.UpdatePluginSettings(plugin.Package.Value.path, plugin.Package.Value.settings);
                if (plugin.Package.Value.settings.IsEnabled)
                {
                    PluginManager.LoadPlugin(plugin.Package.Value.plugin);
                }
                else
                {
                    if (PluginManager.UnloadPlugin(plugin.Package.Value.plugin))
                    {
                        Debugger.Module($"Unloaded {plugin.PluginInformation.Name}");
                    }
                }
                OnPropertyChanged();
            }
        }


        private async void GetImage()
        {
            try
            {
                // trying to display local icon
                string path = $"{plugin.RootPath}/icon.png";
                if (File.Exists(path))
                {
                    var bytes = File.ReadAllBytes(path);
                    Image = LoadImage(bytes);
                }
                else
                {
                    // if image is missing locally, try to load it from registry url
                    var url = pluginList.TryGetRegistryEntry(plugin.PluginInformation.Name)?.ImageUrl;
                    if (string.IsNullOrEmpty(url)) return;

                    using var http = new HttpClient();

                    var bytes = await http.GetByteArrayAsync(url);
                    Image = LoadImage(bytes);
                }
            }
            catch
            {
                Image = null;
            }

            Dispatch(() =>
            {
                OnPropertyChanged(nameof(Image));
                OnPropertyChanged(nameof(HasImage));
            });
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

        public bool IsBusy => false;

        ~LoadedPluginViewModel()
        {
            pluginList.RegistryPluginsLoaded -= OnRegistryPluginsLoaded;
        }
    }
}
