using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using HunterPie.Plugins;
using HunterPie.UI.Infrastructure;

namespace HunterPie.GUIControls
{
    /// <summary>
    /// Interaction logic for Plugins.xaml
    /// </summary>
    public partial class Plugins : UserControl
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

        public Plugins()
        {
            PluginList = new PluginListViewModel(PluginRegistryService.Instance);
            InitializeComponent();
            PluginList.Refresh();
        }
    }
}
