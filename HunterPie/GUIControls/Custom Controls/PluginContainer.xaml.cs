using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public PluginContainer()
        {
            InitializeComponent();
        }
    }
}
