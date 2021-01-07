using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HunterPie.Plugins;

namespace HunterPie.GUIControls
{
    /// <summary>
    /// Interaction logic for PluginListCategoryControl.xaml
    /// </summary>
    public partial class PluginListCategoryControl : UserControl
    {
        public PluginListCategoryControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            "IsBusy", typeof(bool), typeof(PluginListCategoryControl), new PropertyMetadata(default(bool)));
        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }


        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(PluginListCategoryControl), new PropertyMetadata(default(string)));
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }


        public static readonly DependencyProperty CollectionProperty = DependencyProperty.Register(
            "Collection", typeof(ObservableCollection<IPluginViewModel>), typeof(PluginListCategoryControl), new PropertyMetadata(default(ObservableCollection<IPluginViewModel>)));

        public ObservableCollection<IPluginViewModel> Collection
        {
            get { return (ObservableCollection<IPluginViewModel>)GetValue(CollectionProperty); }
            set { SetValue(CollectionProperty, value); }
        }


        public static readonly DependencyProperty SelectedPluginProperty = DependencyProperty.Register(
            "SelectedPlugin", typeof(IPluginViewModel), typeof(PluginListCategoryControl), new PropertyMetadata(default(IPluginViewModel)));

        public IPluginViewModel SelectedPlugin
        {
            get { return (IPluginViewModel)GetValue(SelectedPluginProperty); }
            set { SetValue(SelectedPluginProperty, value); }
        }

        public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register(
            "ErrorMessage", typeof(string), typeof(PluginListCategoryControl), new PropertyMetadata(default(string)));

        public string ErrorMessage
        {
            get { return (string)GetValue(ErrorMessageProperty); }
            set { SetValue(ErrorMessageProperty, value); }
        }


        private void RouteMouseWheelToParent(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }
    }
}
