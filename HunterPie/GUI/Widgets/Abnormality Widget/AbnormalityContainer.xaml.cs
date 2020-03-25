using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HunterPie.Core;

namespace HunterPie.GUI.Widgets.Abnormality_Widget {
    /// <summary>
    /// Interaction logic for AbnormalityContainer.xaml
    /// </summary>
    public partial class AbnormalityContainer : Widget {

        Dictionary<string, Parts.AbnormalityControl> ActiveAbnormalities = new Dictionary<string, Parts.AbnormalityControl>();
        AbnormalityTraySettings AbnormalityWidgetSettings;
        Player Context;
        public int AbnormalityTrayIndex;
        private int MaxSize;

        public AbnormalityContainer(Player context, int TrayIndex) {
            InitializeComponent();
            BaseWidth = Width;
            BaseHeight = Height;
            WidgetType = 5;
            AbnormalityTrayIndex = TrayIndex;
            ApplySettings();
            SetWindowFlags();
            SetContext(context);
        }

        public override void ApplySettings(bool FocusTrigger = false) {
            if (AbnormalityTrayIndex >= UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars) {
                this.Close();
                return;
            }
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                if (!FocusTrigger) {
                    this.WidgetActive = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Enabled;
                    this.Top = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Position[1] + UserSettings.PlayerConfig.Overlay.Position[1];
                    this.Left = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Position[0] + UserSettings.PlayerConfig.Overlay.Position[0];
                    this.BuffTray.Orientation = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Orientation == "Horizontal" ? Orientation.Horizontal : Orientation.Vertical;
                    int BuffTrayMaxSize = Math.Max(UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].MaxSize, 0);
                    if (BuffTrayMaxSize > 7000) BuffTrayMaxSize = 0;
                    if (this.BuffTray.Orientation == Orientation.Horizontal) {
                        this.BuffTray.MaxWidth = BuffTrayMaxSize == 0 ? 300 : BuffTrayMaxSize;
                        this.MaxSize = (int)this.BuffTray.MaxWidth;
                        if (InDesignMode) {
                            this.Width = MaxSize;
                            this.Height = BuffTray.MinHeight;
                        }
                    } else {
                        this.BuffTray.MaxHeight = BuffTrayMaxSize == 0 ? 300 : BuffTrayMaxSize;
                        this.MaxSize = (int)this.BuffTray.MaxHeight;
                        if (InDesignMode) {
                            this.Height = MaxSize;
                            this.Width = BuffTray.MinWidth;
                        }
                    }
                    ScaleWidget(UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Scale, UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Scale);
                }
                base.ApplySettings();
            }));
        }

        private void SaveSettings() {
            if (AbnormalityTrayIndex >= UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars) {
                this.Close();
                return;
            }
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Position[0] = (int)Left - UserSettings.PlayerConfig.Overlay.Position[0];
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Position[1] = (int)Top - UserSettings.PlayerConfig.Overlay.Position[1];
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].MaxSize = this.MaxSize;
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Orientation = BuffTray.Orientation == Orientation.Horizontal ? "Horizontal" : "Vertical";
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Scale = DefaultScaleX;
        }

        private void SetContext(Player ctx) {
            Context = ctx;
            HookEvents();
        }

        public override void EnterWidgetDesignMode() {
            blocker = 2;
            SizeToContent = SizeToContent.Manual;
            if (BuffTray.Orientation == Orientation.Horizontal) {
                this.Width = MaxSize;
                this.Height = BuffTray.MinHeight;
            } else {
                this.Height = MaxSize;
                this.Width = BuffTray.MinWidth;
            }
            base.EnterWidgetDesignMode();
            this.ResizeMode = ResizeMode.CanResizeWithGrip;
            this.SettingsButton.Visibility = Visibility.Visible;
            RemoveWindowTransparencyFlag();
        }

        public override void LeaveWidgetDesignMode() {
            base.LeaveWidgetDesignMode();
            SizeToContent = SizeToContent.WidthAndHeight;
            this.SettingsButton.Visibility = Visibility.Collapsed;
            ApplyWindowTransparencyFlag();
            SaveSettings();
            this.ResizeMode = ResizeMode.CanResize;
        }

        #region Game events

        private void HookEvents() {
            Context.Abnormalities.OnNewAbnormality += OnPlayerNewAbnormality;
            Context.Abnormalities.OnAbnormalityRemove += OnPlayerAbnormalityEnd;
        }

        private void UnhookEvents() {
            Context.Abnormalities.OnNewAbnormality -= OnPlayerNewAbnormality;
            Context.Abnormalities.OnAbnormalityRemove -= OnPlayerAbnormalityEnd;
        }

        private void OnPlayerAbnormalityEnd(object source, AbnormalityEventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() => {
                this.ActiveAbnormalities.Remove(args.Abnormality.InternalID);
                this.RedrawComponent();
            }));
        }

        private void OnPlayerNewAbnormality(object source, AbnormalityEventArgs args) {
            // Ignore abnormalities that aren't enabled for this tray
            if (!UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].AcceptedAbnormalities.Contains(args.Abnormality.InternalID)) return;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() => {
                this.WidgetHasContent = true;
                Parts.AbnormalityControl AbnormalityBox = new Parts.AbnormalityControl() {
                    ShowAbnormalityTimerText = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].ShowTimeLeftText,
                    AbnormalityTimerTextFormat = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].TimeLeftTextFormat
                };
                AbnormalityBox.Initialize(args.Abnormality);
                this.ActiveAbnormalities.Add(args.Abnormality.InternalID, AbnormalityBox);
                this.RedrawComponent();
            }));
        }

        #endregion

        #region Rendering

        private void RedrawComponent() {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => {
                this.BuffTray.Children.Clear();
                if (this.ActiveAbnormalities.Count == 0) {
                    this.WidgetHasContent = false;
                }
                ChangeVisibility(false);
                foreach (Parts.AbnormalityControl Abnorm in ActiveAbnormalities.Values.OrderByDescending(abnormality => abnormality.Context?.Duration)) {
                    this.BuffTray.Children.Add(Abnorm);
                }
            }));
        }

        public void ScaleWidget(double NewScaleX, double NewScaleY) {
            if (NewScaleX <= 0.2) return;
            this.BuffTray.LayoutTransform = new ScaleTransform(NewScaleX, NewScaleY);
            this.MinHeight = MinWidth = 40 * NewScaleX;
            if (this.BuffTray.Orientation == Orientation.Horizontal) {
                this.Height = MinHeight;
            } else {
                this.Width = MinWidth;
            }
            DefaultScaleX = NewScaleX;
            DefaultScaleY = NewScaleY;
        }

        #endregion


        #region Window events

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            this.UnhookEvents();
            this.BuffTray.Children.Clear();
            this.IsClosed = true;
        }

        private void OnMouseEnter(object sender, MouseEventArgs e) {
            this.MouseOver = true;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                this.ResizeMode = ResizeMode.NoResize;
                this.MoveWidget();
                UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Position[0] = (int)Left - UserSettings.PlayerConfig.Overlay.Position[0];
                UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Position[1] = (int)Top - UserSettings.PlayerConfig.Overlay.Position[1];
                UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Scale = DefaultScaleX;
            }
            if (e.LeftButton == MouseButtonState.Released) {
                this.ResizeMode = ResizeMode.CanResizeWithGrip;
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e) {
            if (this.MouseOver) {
                if (e.Delta > 0) {
                    ScaleWidget(DefaultScaleX + 0.05, DefaultScaleY + 0.05);
                } else {
                    ScaleWidget(DefaultScaleX - 0.05, DefaultScaleY - 0.05);
                }
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs e) {
            this.MouseOver = false;
        }

        int blocker = 2;
        private void OnSizeChange(object sender, SizeChangedEventArgs e) {
            blocker--;
            if (!InDesignMode || blocker > 0) return;
            if (this.BuffTray.Orientation == Orientation.Horizontal) {
                this.MaxSize = (int)e.NewSize.Width;
                this.BuffTray.MaxWidth = MaxSize;
            } else {
                this.MaxSize = (int)e.NewSize.Height;
                this.BuffTray.MaxHeight = MaxSize;
            }
        }

        private void OnSettingsButtonClick(object sender, MouseButtonEventArgs e) {
            if (this.AbnormalityWidgetSettings == null || this.AbnormalityWidgetSettings.IsClosed) {
                AbnormalityWidgetSettings = new AbnormalityTraySettings(this, this.AbnormalityTrayIndex);
                AbnormalityWidgetSettings.Show();
            }
        }

        #endregion
    }
}
