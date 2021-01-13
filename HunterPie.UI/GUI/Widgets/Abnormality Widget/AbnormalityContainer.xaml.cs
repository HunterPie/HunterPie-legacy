using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HunterPie.Core;
using HunterPie.Core.Events;

namespace HunterPie.GUI.Widgets.Abnormality_Widget
{
    /// <summary>
    /// Interaction logic for AbnormalityContainer.xaml
    /// </summary>
    public partial class AbnormalityContainer : Widget
    {

        public new WidgetType Type => WidgetType.AbnormalityWidget;

        readonly Dictionary<string, Parts.AbnormalityControl> activeAbnormalities = new Dictionary<string, Parts.AbnormalityControl>();

        Player Context { get; set; }
        public int AbnormalityTrayIndex { get; set; }

        private UserSettings.Config.AbnormalityBar Settings =>
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex];

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
            if (AbnormalityTrayIndex >= UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars)
            {
                Close();
                return;
            }
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {

                WidgetActive = Settings.Enabled;
                Top = Settings.Position[1] + UserSettings.PlayerConfig.Overlay.Position[1];
                Left = Settings.Position[0] + UserSettings.PlayerConfig.Overlay.Position[0];
                BuffTray.Orientation = Settings.Orientation == "Horizontal" ? Settings.ShowNames ? Orientation.Vertical : Orientation.Horizontal : Orientation.Vertical;
                BackgroundTray.Opacity = Settings.BackgroundOpacity;
                int BuffTrayMaxSize = Math.Max(Settings.MaxSize, 0);
                if (BuffTrayMaxSize > 7000) BuffTrayMaxSize = 0;
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
                ScaleWidget(Settings.Scale, Settings.Scale);
                base.ApplySettings();
            }));
        }

        private void SaveSettings()
        {
            if (AbnormalityTrayIndex >= UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars)
            {
                Close();
                return;
            }
            Settings.Position[0] = (int)Left - UserSettings.PlayerConfig.Overlay.Position[0];
            Settings.Position[1] = (int)Top - UserSettings.PlayerConfig.Overlay.Position[1];
            Settings.MaxSize = MaxSize;
            Settings.Orientation = BuffTray.Orientation == Orientation.Horizontal ? "Horizontal" : "Vertical";
            Settings.Scale = DefaultScaleX;
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
            if (!Settings.AcceptedAbnormalities.Contains(args.Abnormality.InternalID)) return;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
            {
                WidgetHasContent = true;
                Parts.AbnormalityControl AbnormalityBox = new Parts.AbnormalityControl()
                {
                    ShowAbnormalityTimerText = Settings.ShowTimeLeftText,
                    AbnormalityTimerTextFormat = Settings.TimeLeftTextFormat,
                    ShowAbnormalityName = Settings.ShowNames
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

        public new void ScaleWidget(double NewScaleX, double NewScaleY)
        {
            if (NewScaleX <= 0.2) return;
            BuffTray.LayoutTransform = new ScaleTransform(NewScaleX, NewScaleY);
            MinHeight = MinWidth = 40 * NewScaleX;
            if (BuffTray.Orientation == Orientation.Horizontal)
            {
                Height = MinHeight;
            }
            else
            {
                Width = MinWidth;
            }
            DefaultScaleX = NewScaleX;
            DefaultScaleY = NewScaleY;
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
                
                Settings.Position[0] = (int)Left - UserSettings.PlayerConfig.Overlay.Position[0];
                Settings.Position[1] = (int)Top - UserSettings.PlayerConfig.Overlay.Position[1];
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
