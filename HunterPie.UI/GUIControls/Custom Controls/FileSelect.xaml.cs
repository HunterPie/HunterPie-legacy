using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HunterPie.UI.Annotations;
using HunterPie.UI.Infrastructure;

namespace HunterPie.GUIControls.Custom_Controls
{
    /// <summary>
    /// Interaction logic for FileSelect.xaml
    /// </summary>
    public partial class FileSelect : UserControl, INotifyPropertyChanged
    {
        public FileSelect()
        {
            ClearCommand = new ArglessRelayCommand(Clear);
            InitializeComponent();
        }

        public ICommand ClearCommand { get; }

        private void Clear()
        {
            SelectedPath = null;
        }


        public static readonly DependencyProperty FileSelectionFilterProperty = DependencyProperty.Register(
            "FileSelectionFilter", typeof(string), typeof(FileSelect), new PropertyMetadata(default(string)));

        public string FileSelectionFilter
        {
            get { return (string)GetValue(FileSelectionFilterProperty); }
            set { SetValue(FileSelectionFilterProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label", typeof(string), typeof(FileSelect), new PropertyMetadata(default(string)));

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty ButtonLabelProperty = DependencyProperty.Register(
            "ButtonLabel", typeof(string), typeof(FileSelect), new PropertyMetadata(default(string)));

        public string ButtonLabel
        {
            get { return (string)GetValue(ButtonLabelProperty); }
            set { SetValue(ButtonLabelProperty, value); }
        }

        public static readonly DependencyProperty SelectedPathProperty = DependencyProperty.Register(
            "SelectedPath", typeof(string), typeof(FileSelect), new PropertyMetadata(default(string)));

        public string SelectedPath
        {
            get { return (string)GetValue(SelectedPathProperty); }
            set
            {
                SetValue(SelectedPathProperty, value);
                this.SelectPathBtn.Content = SelectedPathDisplay;
                OnPropertyChanged(nameof(SelectedPath));
                OnPropertyChanged(nameof(FileToolTip));
            }
        }

        public string FileToolTip => string.IsNullOrEmpty(SelectedPath) ? null : SelectedPath;

        public string SelectedPathDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(SelectedPath)) return ButtonLabel;
                if (SelectedPath.Length > 15)
                {
                    int i = (SelectedPath.Length / 2) - 10;
                    return "..." + SelectedPath.Substring(i);
                }

                return SelectedPath;
            }
        }

        private void SelectPathBtn_OnLostFocus(object sender, RoutedEventArgs e)
        {
            this.SelectPathBtn.Focusable = true;
        }

        private void SelectPathBtn_OnClick(object sender, RoutedEventArgs e)
        {
            using var filePicker = new System.Windows.Forms.OpenFileDialog
            {
                Filter = FileSelectionFilter,
                InitialDirectory = string.IsNullOrEmpty(SelectedPath)
                    ? null
                    : System.IO.Path.GetDirectoryName(SelectedPath)
            };
            var source = (Button)sender;

            System.Windows.Forms.DialogResult result = filePicker.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                SelectedPath = filePicker.FileName;
            }
            source.Focusable = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
