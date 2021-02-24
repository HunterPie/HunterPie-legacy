using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using HunterPie.Core.Readme;
using HunterPie.UI.Infrastructure;
using HunterPie.Utils;

namespace HunterPie.Plugins
{
    public class PluginListViewModel : BaseViewModel, IPluginListProxy
    {
        private readonly PluginRegistryService service;
        private readonly ReadmeService readmeService;
        private bool isLoadingPlugins;
        private bool isLoadingLocalPlugins;
        private Task refreshTask = Task.CompletedTask;
        private List<PluginRegistryEntry> pluginRegistryCache = new();
        private string searchQueryCache = null;
        private HttpClient http;

        private IEnumerable<IPluginViewModel> AllPlugins =>
            InstalledPlugins
            .Concat(AvailablePlugins)
            .Concat(FreshPlugins)
            .Concat(MarkedForRemovalPlugins)
            .ToList();


        public PluginListViewModel(PluginRegistryService service) : this()
        {
            this.service = service;
            http = new HttpClient();
            readmeService = new ReadmeService(http);
            Readme = new ReadmeViewModel(readmeService);

            RefreshCommand = new ArglessRelayCommand(
                () => !IsLoadingPlugins && !IsLoadingLocalPlugins && AllPlugins.All(p => !p.IsBusy),
                Refresh);
            FilterCommand = new RelayCommand<string>(Filter);
        }

        public PluginListViewModel()
        {
            AvailablePlugins = new ObservableCollection<IPluginViewModel>();
            FreshPlugins = new ObservableCollection<IPluginViewModel>();
            InstalledPlugins = new ObservableCollection<IPluginViewModel>();
            MarkedForRemovalPlugins = new ObservableCollection<IPluginViewModel>();
        }

        ~PluginListViewModel()
        {
            ClearPlugins();
        }

        public void MovePluginToCorrectCollection(IPluginViewModel plugin)
        {
            if (PluginManager.IsInstalled(plugin.InternalName))
            {
                if (PluginManager.IsMarkedForRemoval(plugin.InternalName))
                {
                    // installed & marked for removal -> marked for removal
                    SetPluginToCollection(plugin, MarkedForRemovalPlugins);
                }
                else if (PluginManager.packages.Any(p => p.information.Name == plugin.InternalName) || PluginManager.IsFailed(plugin.InternalName))
                {
                    // installed & (activated || failed activation) -> installed
                    SetPluginToCollection(plugin, InstalledPlugins);
                }
                else
                {
                    // installed, not activated -> fresh
                    SetPluginToCollection(plugin, FreshPlugins);
                }
            }
            else
            {
                // not installed -> available
                SetPluginToCollection(plugin, AvailablePlugins);
            }
        }

        public void SetPluginToCollection(IPluginViewModel plugin, ObservableCollection<IPluginViewModel> target)
        {
            var collections = new[] {InstalledPlugins, AvailablePlugins, FreshPlugins, MarkedForRemovalPlugins};
            foreach (var collection in collections)
            {
                if (collection != target)
                {
                    collection.Remove(plugin);
                }
            }

            if (!target.Contains(plugin))
            {
                target.Add(plugin);
            }
        }

        public void Refresh()
        {
            // only one refresh at a time
            if (!refreshTask.IsCompleted) return;
            refreshTask = InternalRefresh();
        }

        public void ClearPlugins()
        {
            SelectedPlugin = null;

            // dispose already created entries so they can be garbage collected
            foreach (var plugin in AllPlugins) plugin.Dispose();

            Dispatch(() =>
            {
                InstalledPlugins.Clear();
                AvailablePlugins.Clear();
                FreshPlugins.Clear();
                MarkedForRemovalPlugins.Clear();
            });
        }

        private async Task InternalRefresh()
        {
            // should be called from STA
            var wasSelected = SelectedPlugin?.InternalName;

            ClearPlugins();

            try
            {
                IsLoadingLocalPlugins = true;
                // making sure that all packages are loaded
                await PluginManager.PreloadTask.ConfigureAwait(false);
                var installedPlugins = await Task.Run(() => PluginManager.GetAllPlugins().ToList()).ConfigureAwait(false);

                // display installed plugins
                Dispatch(() =>
                {
                    foreach (var vm in installedPlugins.Select(plugin => new LoadedPluginViewModel(plugin, this)))
                    {
                        MovePluginToCorrectCollection(vm);
                    }
                    IsLoadingLocalPlugins = false;
                    RestorePluginSelection(wasSelected);
                });

                await LoadRegistryPlugins(installedPlugins);

                // restore plugin selection if user didn't select other menu item while plugins were loading
                RestorePluginSelection(wasSelected);
            }
            finally
            {
                // in case plugin loading failed, disable loading anyway
                Dispatch(() => IsLoadingLocalPlugins = false);
            }
        }

        private void RestorePluginSelection(string wasSelected)
        {
            // if user selected plugin during refresh, don't change selection
            if (SelectedPlugin != null) return;

            foreach (var plugin in AllPlugins)
            {
                if (plugin.Name == wasSelected)
                {
                    Dispatch(() => SelectedPlugin = plugin);
                }
            }
        }

        private async Task LoadRegistryPlugins(List<PluginEntry> installedPlugins)
        {
            if (IsLoadingPlugins) return;

            try
            {
                Dispatch(() =>
                {
                    IsLoadingPlugins = true;
                    PluginLoadingError = "";
                    OnPropertyChanged(nameof(PluginLoadingError));
                    AvailablePlugins.Clear();
                });
                var plugins = await service.GetAll();
                // caching results for further reference
                pluginRegistryCache = plugins;
                Dispatch(() =>
                {
                    foreach (var plugin in plugins)
                    {
                        // filter already installed plugins
                        if (installedPlugins.Any(p => p.PluginInformation.Name == plugin.InternalName))
                        {
                            continue;
                        }

                        AvailablePlugins.Add(new RegistryPluginViewModel(plugin, this, readmeService, http));
                    }
                });
            }
            catch (Exception ex)
            {
                PluginLoadingError = ex.GetBaseException().Message;
            }
            finally
            {
                Dispatch(() =>
                {
                    IsLoadingPlugins = false;
                    UpdatePluginsArrangement();
                    OnPropertyChanged(nameof(PluginLoadingError));
                    CommandManager.InvalidateRequerySuggested();
                    RegistryPluginsLoaded?.Invoke(this, EventArgs.Empty);
                    Filter(this.searchQueryCache);
                });
            }
        }

        public string PluginLoadingError { get; private set; }

        public event EventHandler RegistryPluginsLoaded;

        public void UpdatePluginsArrangement()
        {
            foreach (var plugin in AllPlugins)
            {
                MovePluginToCorrectCollection(plugin);
            }
            OnListsChanged();
        }

        public PluginRegistryEntry TryGetRegistryEntry(string pluginName)
        {
            if (IsLoadingPlugins) return null;
            return pluginRegistryCache.FirstOrDefault(e => e.InternalName == pluginName);
        }

        public void Filter(string query)
        {
            foreach (IPluginViewModel plugin in AllPlugins)
            {
                if (string.IsNullOrEmpty(query))
                {
                    plugin.IsFiltered = false;
                } else
                {
                    var isMatch = plugin.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase)
                                  || plugin.InternalName.Contains(query, StringComparison.CurrentCultureIgnoreCase)
                                  || plugin.Description.Contains(query, StringComparison.CurrentCultureIgnoreCase);

                    plugin.IsFiltered = !isMatch;
                }
            }

            this.searchQueryCache = query;
        }

        public bool IsLoadingPlugins
        {
            get => isLoadingPlugins;
            set
            {
                if (isLoadingPlugins != value)
                {
                    isLoadingPlugins = value;
                    OnPropertyChanged(nameof(IsLoadingPlugins));
                }
            }
        }

        public bool IsLoadingLocalPlugins
        {
            get => isLoadingLocalPlugins;
            set
            {
                if (isLoadingLocalPlugins != value)
                {
                    isLoadingLocalPlugins = value;
                    OnPropertyChanged(nameof(IsLoadingLocalPlugins));
                }
            }
        }

        public ICommand RefreshCommand { get; }

        public ICommand FilterCommand { get; }

        public ReadmeViewModel Readme { get; }

        public ObservableCollection<IPluginViewModel> InstalledPlugins { get; set; }
        public ObservableCollection<IPluginViewModel> MarkedForRemovalPlugins { get; set; }
        public ObservableCollection<IPluginViewModel> AvailablePlugins { get; set; }
        public ObservableCollection<IPluginViewModel> FreshPlugins { get; set; }

        public bool NeedsReload => MarkedForRemovalPlugins.Count > 0 || FreshPlugins.Count > 0;

        private IPluginViewModel selectedPlugin;

        public bool HasSelectedPlugin => selectedPlugin != null;

        public IPluginViewModel SelectedPlugin
        {
            get => selectedPlugin;
            set
            {
                if (selectedPlugin == value) return;
                selectedPlugin = value;
                Readme.Load(value?.ReadmeUrl);
                OnListsChanged();
            }
        }

        public IPluginViewModel SelectedInstalledPlugin
        {
            get => InstalledPlugins.Contains(SelectedPlugin) ? SelectedPlugin : null;
            set => SelectedPlugin = value;
        }
        public IPluginViewModel SelectedAvailablePlugin
        {
            get => AvailablePlugins.Contains(SelectedPlugin) ? SelectedPlugin : null;
            set => SelectedPlugin = value;
        }
        public IPluginViewModel SelectedFreshPlugin
        {
            get => FreshPlugins.Contains(SelectedPlugin) ? SelectedPlugin : null;
            set => SelectedPlugin = value;
        }
        public IPluginViewModel SelectedMarkedForRemovalPlugin
        {
            get => MarkedForRemovalPlugins.Contains(SelectedPlugin) ? SelectedPlugin : null;
            set => SelectedPlugin = value;
        }

        private void OnListsChanged()
        {
            OnPropertyChanged(nameof(SelectedInstalledPlugin));
            OnPropertyChanged(nameof(SelectedAvailablePlugin));
            OnPropertyChanged(nameof(SelectedFreshPlugin));
            OnPropertyChanged(nameof(SelectedMarkedForRemovalPlugin));
            OnPropertyChanged(nameof(NeedsReload));
            OnPropertyChanged(nameof(SelectedPlugin));
            OnPropertyChanged(nameof(HasSelectedPlugin));
        }
    }
}
