using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HunterPie.Annotations;
using HunterPie.UI.Infrastructure;

namespace HunterPie.GUIControls
{
    /// <summary>
    /// Interaction logic for SearchBar.xaml
    /// </summary>
    public partial class SearchBar : UserControl, INotifyPropertyChanged
    {
        public SearchBar()
        {
            ToggleSearchCommand = new RelayCommand(_ => true, ToggleSearch);
            CancelCommand = new ArglessRelayCommand(() => true, Cancel);
            InitializeComponent();
        }
        private string searchQuery;


        public static readonly DependencyProperty IsSearchBarActiveProperty = DependencyProperty.Register(
            "IsSearchBarActive", typeof(bool), typeof(SearchBar), new PropertyMetadata(default(bool)));

        public bool IsSearchBarActive
        {
            get { return (bool)GetValue(IsSearchBarActiveProperty); }
            set { SetValue(IsSearchBarActiveProperty, value); }
        }

        public static readonly DependencyProperty SearchQueryUpdatedCommandProperty = DependencyProperty.Register(
            "SearchQueryUpdatedCommand", typeof(ICommand), typeof(SearchBar), new PropertyMetadata(default(ICommand)));

        public ICommand SearchQueryUpdatedCommand
        {
            get { return (ICommand)GetValue(SearchQueryUpdatedCommandProperty); }
            set { SetValue(SearchQueryUpdatedCommandProperty, value); }
        }

        public string SearchQuery
        {
            get => searchQuery;
            set
            {
                if (value == searchQuery) return;
                searchQuery = value;
                OnPropertyChanged();
                SearchQueryUpdatedCommand.Execute(value);
            }
        }

        public ICommand ToggleSearchCommand { get; }

        public ICommand CancelCommand { get; }

        public void ToggleSearch(object arg)
        {
            // toggle if value wasn't provided or set to provided value
            IsSearchBarActive = (arg is bool active) ? active : !IsSearchBarActive;

            if (IsSearchBarActive)
            {
                // if became active, focus textbox
                SearchQueryTextBox.Focus();

                // make sure SearchQuery-dependent members are initialized
                OnPropertyChanged(nameof(SearchQuery));
            }
        }

        public void Cancel()
        {
            // first Cancel will clear query, second will close search
            if (string.IsNullOrEmpty(SearchQuery))
            {
                ToggleSearch(false);
            }
            else
            {
                SearchQuery = "";
                // prevent loosing focus
                if (IsSearchBarActive) SearchQueryTextBox.Focus();
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
