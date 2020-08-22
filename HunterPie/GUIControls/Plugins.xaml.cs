using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using HunterPie.GUIControls.Custom_Controls;
using HunterPie.Plugins;

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


        public Plugins() => InitializeComponent();

        public void InitializePluginDisplayer(List<PluginPackage> pluginPackages)
        {
            foreach (PluginPackage package in pluginPackages)
            {
                PluginContainer container = new PluginContainer();
                container.InitializePluginContainer(package);

                PluginDisplay.Children.Add(container);
            }
        }

        private void OpenPluginsFolder(object sender, RoutedEventArgs e)
        {
            Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modules"));
        }

        private void OpenPluginsRepo(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Haato3o/HunterPie.Plugins");
        }
    }
}
