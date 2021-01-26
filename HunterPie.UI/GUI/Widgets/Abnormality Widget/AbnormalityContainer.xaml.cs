using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HunterPie.Core;
using HunterPie.Core.Events;
using HunterPie.Core.Settings;

namespace HunterPie.GUI.Widgets.Abnormality_Widget
{
    /// <summary>
    /// Interaction logic for AbnormalityContainer.xaml
    /// </summary>
    public partial class AbnormalityContainer : Widget
    {

        public override WidgetType Type => WidgetType.AbnormalityWidget;

        readonly Dictionary<string, Parts.AbnormalityControl> activeAbnormalities = new Dictionary<string, Parts.AbnormalityControl>();

        Player Context { get; set; }
        public int AbnormalityTrayIndex { get; set; }

        public override IWidgetSettings Settings =>
            ConfigManager.Settings.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex];

        private AbnormalityBar settings => (AbnormalityBar)Settings;

        private int MaxSize { get; set; }

        public AbnormalityContainer(Player context, int TrayIndex)
        {
            InitializeComponent();
            AbnormalityTrayIndex = TrayIndex;
            SetWindowFlags();
            SetContext(context);
        }

        public override void ApplySettings()
        {
            if (AbnormalityTrayIndex >= ConfigManager.Settings.Overlay.AbnormalitiesWidget.ActiveBars)
            {
                Close();
                return;
            }
            BuffTray.Orientation = settings.Orientation == "Horizontal" ? settings.ShowNames ? Orientation.Vertical : Orientation.Horizontal : Orientation.Vertical;
            BackgroundTray.Opacity = settings.BackgroundOpacity;
            int BuffTrayMaxSize = Math.Max(settings.MaxSize, 0);

            if (BuffTrayMaxSize > 7000)
                BuffTrayMaxSize = 0;

            if (BuffTray.Orientation == Orientation.Horizontal)
            {
                BuffTray.MaxWidth = BuffTrayMaxSize == 0 ? 300 : BuffTrayMaxSize;
                MaxSize = (int)BuffTray.MaxWidth;
                if (InDesignMode)
                {
                    Width = MaxSize;
                    Height = BuffTray.MinHeight;
                }
            }
            else
            {
                BuffTray.MaxHeight = BuffTrayMaxSize == 0 ? 300 : BuffTrayMaxSize;
                MaxSize = (int)BuffTray.MaxHeight;
                if (InDesignMode)
                {
                    Height = MaxSize;
                    Width = BuffTray.MinWidth;
                }
            }
            base.ApplySettings();
        }

        public override void SaveSettings()
        {
            if (AbnormalityTrayIndex >= ConfigManager.Settings.Overlay.AbnormalitiesWidget.ActiveBars)
            {
                Close();
                return;
            }
            settings.MaxSize = MaxSize;
            settings.Orientation = BuffTray.Orientation == Orientation.Horizontal ? "Horizontal" : "Vertical";
            base.SaveSettings();
        }

        private void SetContext(Player ctx)
        {
            Context = ctx;
            HookEvents();
        }

        public override void EnterWidgetDesignMode()
        {
            blocker = 2;
            SizeToContent = SizeToContent.Manual;
            if (BuffTray.Orientation == Orientation.Horizontal)
            {
                Width = MaxSize;
                Height = BuffTray.MinHeight;
            }
            else
            {
                Height = MaxSize;
                Width = BuffTray.MinWidth;
            }
            base.EnterWidgetDesignMode();
            ResizeMode = ResizeMode.CanResizeWithGrip;
            SettingsButton.Visibility = Visibility.Visible;
            RemoveWindowTransparencyFlag();
        }

        public override void LeaveWidgetDesignMode()
        {
            base.LeaveWidgetDesignMode();
            SizeToContent = SizeToContent.WidthAndHeight;
            SettingsButton.Visibility = Visibility.Collapsed;
            ApplyWindowTransparencyFlag();
            SaveSettings();
            ResizeMode = ResizeMode.CanResize;
        }

        #region Game events

        private void HookEvents()
        {
            CompositionTarget.Rendering += OnAbnormalityTrayRender;
            Context.Abnormalities.OnNewAbnormality += OnPlayerNewAbnormality;
            Context.Abnormalities.OnAbnormalityRemove += OnPlayerAbnormalityEnd;
        }

        int renderCounter = 0;
        private void OnAbnormalityTrayRender(object sender, EventArgs e)
        {
            renderCounter++;
            // Only redraws the component once every 60 render calls
            if (renderCounter >= 60)
            {
                RedrawComponent();
                renderCounter = 0;
            }
        }

        private void UnhookEvents()
        {
            CompositionTarget.Rendering -= OnAbnormalityTrayRender;
            Context.Abnormalities.OnNewAbnormality -= OnPlayerNewAbnormality;
            Context.Abnormalities.OnAbnormalityRemove -= OnPlayerAbnormalityEnd;
        }

        private void OnPlayerAbnormalityEnd(object source, AbnormalityEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
        {
            activeAbnormalities.Remove(args.Abnormality.InternalID);
        }));

        private void OnPlayerNewAbnormality(object source, AbnormalityEventArgs args)
        {
            // Ignore abnormalities that aren't enabled for this tray
            if (!settings.AcceptedAbnormalities.Contains(args.Abnormality.InternalID)) return;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
            {
                WidgetHasContent = true;
                Parts.AbnormalityControl AbnormalityBox = new Parts.AbnormalityControl()
                {
                    ShowAbnormalityTimerText = settings.ShowTimeLeftText,
                    AbnormalityTimerTextFormat = settings.TimeLeftTextFormat,
                    ShowAbnormalityName = settings.ShowNames
                };
                AbnormalityBox.Initialize(args.Abnormality);
                activeAbnormalities.Add(args.Abnormality.InternalID, AbnormalityBox);
                RedrawComponent();
            }));
        }

        #endregion

        #region Rendering

        private void RedrawComponent()
        {
            BuffTray.Children.Clear();
            if (activeAbnormalities.Count == 0)
            {
                WidgetHasContent = false;
            }
            ChangeVisibility();
            foreach (Parts.AbnormalityControl Abnorm in activeAbnormalities.Values.OrderBy(abnormality => abnormality.Context?.Duration))
            {
                BuffTray.Children.Add(Abnorm);
            }
        }

        public override void ScaleWidget(double newScaleX, double newScaleY)
        {
            if (newScaleX <= 0.2)
                return;

            BuffTray.LayoutTransform = new ScaleTransform(newScaleX, newScaleY);
            MinHeight = MinWidth = 40 * newScaleX;
            if (BuffTray.Orientation == Orientation.Horizontal)
            {
                Height = MinHeight;
            }
            else
            {
                Width = MinWidth;
            }
            DefaultScaleX = newScaleX;
            DefaultScaleY = newScaleY;
        }

        #endregion


        #region Window events

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnhookEvents();
            BuffTray.Children.Clear();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ResizeMode = ResizeMode.NoResize;
                
                Settings.Position[0] = (int)Left - ConfigManager.Settings.Overlay.Position[0];
                Settings.Position[1] = (int)Top - ConfigManager.Settings.Overlay.Position[1];
                Settings.Scale = DefaultScaleX;
            }
            if (e.LeftButton == MouseButtonState.Released)
            {
                ResizeMode = ResizeMode.CanResizeWithGrip;
            }
        }

        int blocker = 2;
        private void OnSizeChange(object sender, SizeChangedEventArgs e)
        {
            blocker--;
            if (!InDesignMode || blocker > 0) return;
            if (BuffTray.Orientation == Orientation.Horizontal)
            {
                MaxSize = (int)e.NewSize.Width;
                BuffTray.MaxWidth = MaxSize;
            }
            else
            {
                MaxSize = (int)e.NewSize.Height;
                BuffTray.MaxHeight = MaxSize;
            }
        }

        private void OnSettingsButtonClick(object sender, MouseButtonEventArgs e)
        {
            bool settingsWindowIsOpen = Application.Current.Windows
                .Cast<Window>().Any(w => w.Title == "Abnormality Tray Settings");

            if (settingsWindowIsOpen)
                return;

            AbnormalityTraySettings traySettingsWindow = new AbnormalityTraySettings(AbnormalityTrayIndex);
            traySettingsWindow.Show();
        }

        #endregion
    }
}
