using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HunterPie.Core;

namespace HunterPie.GUI.Widgets.Abnormality_Widget
{
    /// <summary>
    /// Interaction logic for AbnormalityContainer.xaml
    /// </summary>
    public partial class AbnormalityContainer : Widget
    {
        readonly Dictionary<string, Parts.AbnormalityControl> ActiveAbnormalities = new Dictionary<string, Parts.AbnormalityControl>();
        Player Context { get; set; }
        public int AbnormalityTrayIndex { get; set; }
        private int MaxSize { get; set; }

        public AbnormalityContainer(Player context, int TrayIndex)
        {
            InitializeComponent();
            BaseWidth = Width;
            BaseHeight = Height;
            WidgetType = 5;
            AbnormalityTrayIndex = TrayIndex;
            ApplySettings();
            SetWindowFlags();
            SetContext(context);
        }

        public override void ApplySettings(bool FocusTrigger = false)
        {
            if (AbnormalityTrayIndex >= UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars)
            {
                Close();
                return;
            }
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                UserSettings.Config.AbnormalityBar preset = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex];
                if (!FocusTrigger)
                {
                    WidgetActive = preset.Enabled;
                    Top = preset.Position[1] + UserSettings.PlayerConfig.Overlay.Position[1];
                    Left = preset.Position[0] + UserSettings.PlayerConfig.Overlay.Position[0];
                    BuffTray.Orientation = preset.Orientation == "Horizontal" ? preset.ShowNames ? Orientation.Vertical : Orientation.Horizontal : Orientation.Vertical;
                    BackgroundTray.Opacity = preset.BackgroundOpacity;
                    int BuffTrayMaxSize = Math.Max(preset.MaxSize, 0);
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
                    ScaleWidget(preset.Scale, preset.Scale);
                }
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
            UserSettings.Config.AbnormalityBar preset = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex];
            preset.Position[0] = (int)Left - UserSettings.PlayerConfig.Overlay.Position[0];
            preset.Position[1] = (int)Top - UserSettings.PlayerConfig.Overlay.Position[1];
            preset.MaxSize = MaxSize;
            preset.Orientation = BuffTray.Orientation == Orientation.Horizontal ? "Horizontal" : "Vertical";
            preset.Scale = DefaultScaleX;
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

        int RenderCounter = 0;
        private void OnAbnormalityTrayRender(object sender, EventArgs e)
        {
            RenderCounter++;
            // Only redraws the component once every 60 render calls
            if (RenderCounter >= 60)
            {
                RedrawComponent();
                RenderCounter = 0;
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
            ActiveAbnormalities.Remove(args.Abnormality.InternalID);
        }));

        private void OnPlayerNewAbnormality(object source, AbnormalityEventArgs args)
        {
            UserSettings.Config.AbnormalityBar preset = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex];
            // Ignore abnormalities that aren't enabled for this tray
            if (!preset.AcceptedAbnormalities.Contains(args.Abnormality.InternalID)) return;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
            {
                WidgetHasContent = true;
                Parts.AbnormalityControl AbnormalityBox = new Parts.AbnormalityControl()
                {
                    ShowAbnormalityTimerText = preset.ShowTimeLeftText,
                    AbnormalityTimerTextFormat = preset.TimeLeftTextFormat,
                    ShowAbnormalityName = preset.ShowNames
                };
                AbnormalityBox.Initialize(args.Abnormality);
                ActiveAbnormalities.Add(args.Abnormality.InternalID, AbnormalityBox);
                RedrawComponent();
            }));
        }

        #endregion

        #region Rendering

        private void RedrawComponent()
        {
            BuffTray.Children.Clear();
            if (ActiveAbnormalities.Count == 0)
            {
                WidgetHasContent = false;
            }
            ChangeVisibility(false);
            foreach (Parts.AbnormalityControl Abnorm in ActiveAbnormalities.Values.OrderBy(abnormality => abnormality.Context?.Duration))
            {
                BuffTray.Children.Add(Abnorm);
            }
        }

        public void ScaleWidget(double NewScaleX, double NewScaleY)
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
            IsClosed = true;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UserSettings.Config.AbnormalityBar preset = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex];
                ResizeMode = ResizeMode.NoResize;
                MoveWidget();
                preset.Position[0] = (int)Left - UserSettings.PlayerConfig.Overlay.Position[0];
                preset.Position[1] = (int)Top - UserSettings.PlayerConfig.Overlay.Position[1];
                preset.Scale = DefaultScaleX;
            }
            if (e.LeftButton == MouseButtonState.Released)
            {
                ResizeMode = ResizeMode.CanResizeWithGrip;
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                ScaleWidget(DefaultScaleX + 0.05, DefaultScaleY + 0.05);
            }
            else
            {
                ScaleWidget(DefaultScaleX - 0.05, DefaultScaleY - 0.05);
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
            bool SettingsWindowIsOpen = App.Current.Windows.Cast<Window>()
                .Where(w => w.Title == "Abnormality Tray Settings")
                .Count() > 0;

            if (SettingsWindowIsOpen) return;

            AbnormalityTraySettings traySettingsWindow = new AbnormalityTraySettings(AbnormalityTrayIndex);
            traySettingsWindow.Show();
        }

        #endregion
    }
}
