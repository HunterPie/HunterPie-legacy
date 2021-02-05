using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using HunterPie.Plugins;

namespace HunterPie.GUIControls
{
    /// <summary>
    /// Interaction logic for PluginListControl.xaml
    /// </summary>
    public partial class PluginListControl : UserControl
    {
        public PluginListControl()
        {
            DataContextChanged += OnDataContextChanged;
            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // HACK: detect that document changed to scroll up
            //       FlowDocumentScrollViewer.OnLoaded is called once for first document only, so I didn't find anything better
            //       than to explicitly subscribe to property changes (sorry!)
            if (e.OldValue is PluginListViewModel prevDc)
            {
                prevDc.Readme.PropertyChanged -= PropChanged;
            }
            if (e.NewValue is PluginListViewModel newDc)
            {
                newDc.Readme.PropertyChanged += PropChanged;
            }
        }

        private void PropChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ReadmeViewModel.Document))
            {
                var scrollViewer = Viewer.Template.FindName("PART_ContentHost", Viewer) as ScrollViewer;
                scrollViewer?.ScrollToHome();
            }
        }

        private void OpenHyperlink(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            Process.Start(e.Parameter.ToString());
        }
    }
}
