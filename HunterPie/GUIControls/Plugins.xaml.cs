using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HunterPie.Plugins;
using HunterPie.UI.Infrastructure;

namespace HunterPie.GUIControls
{
    /// <summary>
    /// Interaction logic for Plugins.xaml
    /// </summary>
    public partial class Plugins : UserControl, IActivatable
    {

        private static Plugins instance;
        public static Plugins Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Plugins();
                }
                return instance;
            }
        }

        public PluginListViewModel PluginList { get; set; }

        public ICommand OpenPluginsFolderCommand { get; } = new RelayCommand(
            _ => true,
            _ => Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modules"))
        );

        public ICommand RestartCommand { get; } = new RelayCommand(
            _ => true,
            _ => Hunterpie.Instance.Reload()
        );

        public static readonly DependencyProperty SearchOpenedProperty = DependencyProperty.Register(
            "SearchOpened", typeof(bool), typeof(Plugins), new PropertyMetadata(default(bool)));

        private KeyBinding SearchBinding { get; }
        public KeyBinding RefreshBinding { get; }

        public Plugins()
        {
            PluginList = new PluginListViewModel(PluginRegistryService.Instance);
            InitializeComponent();
            SearchBinding = new KeyBinding(SearchBarControl.ToggleSearchCommand, Key.F, ModifierKeys.Control)
            {
                CommandParameter = true
            };
            RefreshBinding = new KeyBinding(PluginList.RefreshCommand, Key.F5, ModifierKeys.None);
            PluginList.Refresh();
        }


        public void OnActivate()
        {
            Hunterpie.Instance.InputBindings.Add(SearchBinding);
            Hunterpie.Instance.InputBindings.Add(RefreshBinding);
        }

        public void OnDeactivate()
        {
            Hunterpie.Instance.InputBindings.Remove(SearchBinding);
            Hunterpie.Instance.InputBindings.Remove(RefreshBinding);
        }
    }
}
