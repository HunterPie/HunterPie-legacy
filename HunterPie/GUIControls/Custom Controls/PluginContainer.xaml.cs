using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HunterPie.Logger;
using HunterPie.Plugins;

namespace HunterPie.GUIControls.Custom_Controls
{
    /// <summary>
    /// PluginContainer displays the Plugin information in the HunterPie Modules manager screen
    /// </summary>
    public partial class PluginContainer : UserControl
    {

        /// <summary>
        /// The name of the plugin
        /// </summary>
        public string PluginName
        {
            get { return (string)GetValue(PluginNameProperty); }
            set { SetValue(PluginNameProperty, value); }
        }
        public static readonly DependencyProperty PluginNameProperty =
            DependencyProperty.Register("PluginName", typeof(string), typeof(PluginContainer));

        /// <summary>
        /// The description of the plugin
        /// </summary>
        public string PluginDescription
        {
            get { return (string)GetValue(PluginDescriptionProperty); }
            set { SetValue(PluginDescriptionProperty, value); }
        }
        public static readonly DependencyProperty PluginDescriptionProperty =
            DependencyProperty.Register("PluginDescription", typeof(string), typeof(PluginContainer));

        /// <summary>
        /// The version of the plugin
        /// </summary>
        public string PluginVersion
        {
            get { return (string)GetValue(PluginVersionProperty); }
            set { SetValue(PluginVersionProperty, value); }
        }
        public static readonly DependencyProperty PluginVersionProperty =
            DependencyProperty.Register("PluginVersion", typeof(string), typeof(PluginContainer));

        /// <summary>
        /// Whether this plugin is enabled or not
        /// </summary>
        public bool IsPluginEnabled
        {
            get { return (bool)GetValue(IsPluginEnabledProperty); }
            set { SetValue(IsPluginEnabledProperty, value); }
        }
        public static readonly DependencyProperty IsPluginEnabledProperty =
            DependencyProperty.Register("IsPluginEnabled", typeof(bool), typeof(PluginContainer));

        /// <summary>
        /// Whether the description panel is visible or not
        /// </summary>
        public bool IsDescriptionOpen
        {
            get { return (bool)GetValue(IsDescriptionOpenProperty); }
            set { SetValue(IsDescriptionOpenProperty, value); }
        }
        public static readonly DependencyProperty IsDescriptionOpenProperty =
            DependencyProperty.Register("IsDescriptionOpen", typeof(bool), typeof(PluginContainer));

        /// <summary>
        /// The current plugin
        /// </summary>
        PluginPackage pluginPackage;

        public PluginContainer()
        {
            InitializeComponent();
        }

        public void InitializePluginContainer(PluginPackage package)
        {
            PluginName = package.information.Name;
            PluginDescription = package.information.Description;
            PluginVersion = package.information.Version;
            pluginPackage = package;

            IsPluginEnabled = package.settings.IsEnabled;
        }

        private void OpenDescription(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsDescriptionOpen = !IsDescriptionOpen;
        }

        private void OnTogglePluginButtonClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            IsPluginEnabled = !IsPluginEnabled;
            pluginPackage.settings.IsEnabled = IsPluginEnabled;
            PluginManager.UpdatePluginSettings(pluginPackage.path, pluginPackage.settings);
            if (IsPluginEnabled)
            {
                PluginManager.LoadPlugin(pluginPackage.plugin);
            } else
            {
                if (PluginManager.UnloadPlugin(pluginPackage.plugin)) Debugger.Module($"Unloaded {pluginPackage.information.Name}");
            }
        }
    }
}
