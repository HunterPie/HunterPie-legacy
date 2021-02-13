using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using HunterPie.Annotations;
using HunterPie.Plugins;

namespace HunterPie.GUIControls
{
    /// <summary>
    /// Interaction logic for PluginListCategoryControl.xaml
    /// </summary>
    public partial class PluginListCategoryControl : UserControl, INotifyPropertyChanged
    {
        public PluginListCategoryControl()
        {
            // to avoid binding errors
            CollectionView = new ListCollectionView(new object[0]);
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
            "Collection", typeof(ObservableCollection<IPluginViewModel>), typeof(PluginListCategoryControl),
            new PropertyMetadata(default(ObservableCollection<IPluginViewModel>), CollectionChangedCallback));

        private static void CollectionChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // When Collection is changed, update collection view to be able to filter it
            var me = (PluginListCategoryControl)d;
            var collection = (ObservableCollection<IPluginViewModel>)e.NewValue;

            if (me.CollectionView != null)
            {
                ((ICollectionView)me.CollectionView).CollectionChanged -= me.CollectionViewChanged;
            }
            me.CollectionView = new ListCollectionView(collection)
            {
                Filter = PluginFilter,
                LiveFilteringProperties = { nameof(IPluginViewModel.IsFiltered) },
                IsLiveFiltering = true,

                LiveSortingProperties = { nameof(IPluginViewModel.SortValue) },
                IsLiveSorting = true,
                SortDescriptions =
                {
                    new SortDescription(nameof(IPluginViewModel.SortValue), ListSortDirection.Descending)
                }
            };
            ((ICollectionView)me.CollectionView).CollectionChanged += me.CollectionViewChanged;

            me.OnPropertyChanged(nameof(CollectionView));
            me.CollectionView.Refresh();
        }

        private void CollectionViewChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CollectionView.Count == Collection.Count)
            {
                Header = $"({Collection.Count})";
            } else
            {
                Header = $"({CollectionView.Count}/{Collection.Count})";
            }

            OnPropertyChanged(nameof(Header));
        }

        public ObservableCollection<IPluginViewModel> Collection
        {
            get => (ObservableCollection<IPluginViewModel>)GetValue(CollectionProperty);
            set => SetValue(CollectionProperty, value);
        }

        public ListCollectionView CollectionView { get; private set; }

        public string Header { get; set; } = "(0)";


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

        public static Predicate<object> PluginFilter => obj => obj is IPluginViewModel {IsFiltered: false};


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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
