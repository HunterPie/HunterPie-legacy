using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        public static readonly DependencyProperty PreviewImgProperty = DependencyProperty.Register(
            "PreviewImg", typeof(ImageSource), typeof(Plugins), new PropertyMetadata(default(ImageSource)));

        public ImageSource PreviewImg
        {
            get { return (ImageSource)GetValue(PreviewImgProperty); }
            set
            {
                SetValue(PreviewImgProperty, value);
                IsPreviewVisible = value != null;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public static readonly DependencyProperty IsPreviewVisibleProperty = DependencyProperty.Register(
            "IsPreviewVisible", typeof(bool), typeof(Plugins), new PropertyMetadata(default(bool)));

        public bool IsPreviewVisible
        {
            get { return (bool)GetValue(IsPreviewVisibleProperty); }
            set { SetValue(IsPreviewVisibleProperty, value); }
        }

        public PluginListViewModel PluginList { get; set; }

        public ICommand OpenPluginsFolderCommand { get; } = new ArglessRelayCommand(
            () => Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modules")));

        public ICommand RestartCommand { get; } = new ArglessRelayCommand(Hunterpie.Instance.Reload);


        public ICommand MagnifyImageCommand { get; }

        public ICommand CloseImagePreviewCommand { get; }


        private KeyBinding[] KeyBindings { get; }

        public Plugins()
        {
            PluginList = new PluginListViewModel(PluginRegistryService.Instance);

            // commands
            MagnifyImageCommand = new RelayCommand(Magnify);
            CloseImagePreviewCommand = new ArglessRelayCommand(ClearMagnify);

            InitializeComponent();
            KeyBindings = CreateKeyBindings();
            PluginList.Refresh();
        }

        private KeyBinding[] CreateKeyBindings() => new[]
            {
                new KeyBinding(SearchBarControl.ToggleSearchCommand, Key.F, ModifierKeys.Control)
                {
                    CommandParameter = true
                },
                new KeyBinding(PluginList.RefreshCommand, Key.F5, ModifierKeys.None),
                new KeyBinding(CloseImagePreviewCommand, Key.Escape, ModifierKeys.None)
            };


        public void OnActivate()
        {
            Hunterpie.Instance.InputBindings.AddRange(KeyBindings);
        }

        public void OnDeactivate()
        {
            foreach (var binding in KeyBindings)
            {
                Hunterpie.Instance.InputBindings.Remove(binding);
            }
        }

        public void Magnify(object arg) => PreviewImg = arg as ImageSource;

        public void ClearMagnify() => PreviewImg = null;

    }
}
