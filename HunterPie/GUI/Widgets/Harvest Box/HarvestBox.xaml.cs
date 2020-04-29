using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HunterPie.Core;
using FertilizerControl = HunterPie.GUI.Widgets.Harvest_Box.Parts.FertilizerControl;

namespace HunterPie.GUI.Widgets {

    public partial class HarvestBox : Widget {

        Player PlayerContext;
        Core.HarvestBox Context {
            get { return PlayerContext?.Harvest; }
        }

        public HarvestBox(Player Context) {
            InitializeComponent();
            BaseWidth = Width;
            BaseHeight = Height;
            WidgetType = 3;
            SetWindowFlags();
            SetContext(Context);
            CreateFertilizerControls();
            ApplySettings();
        }

        public override void EnterWidgetDesignMode() {
            base.EnterWidgetDesignMode();
            RemoveWindowTransparencyFlag();
        }

        public override void LeaveWidgetDesignMode() {
            base.LeaveWidgetDesignMode();
            ApplyWindowTransparencyFlag();
            SaveSettings();
        }

        private void SaveSettings() {
            UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Position[0] = (int)Left - UserSettings.PlayerConfig.Overlay.Position[0];
            UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Position[1] = (int)Top - UserSettings.PlayerConfig.Overlay.Position[1];
            UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Scale = DefaultScaleX;
        }

        public override void ApplySettings(bool FocusTrigger = false) {
            bool ShowEverywhere = false;
            if (UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.AlwaysShow) {
                ShowEverywhere = true;
            } else {
                if (PlayerContext != null && PlayerContext.InHarvestZone) { ShowEverywhere = true; } else { ShowEverywhere = false; }
            }
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                if (!FocusTrigger) {
                    this.Top = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Position[1] + UserSettings.PlayerConfig.Overlay.Position[1];
                    this.Left = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Position[0] + UserSettings.PlayerConfig.Overlay.Position[0];
                    this.WidgetActive = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Enabled;
                    this.WidgetHasContent = ShowEverywhere;
                    this.ScaleWidget(UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Scale, UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Scale);
                    this.SteamTracker.Visibility = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.ShowSteamTracker ? Visibility.Visible : Visibility.Collapsed;
                    this.ArgosyTracker.Visibility = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.ShowArgosyTracker ? Visibility.Visible : Visibility.Collapsed;
                    this.TailraidersTracker.Visibility = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.ShowTailraidersTracker ? Visibility.Visible : Visibility.Collapsed;
                    this.Opacity = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Opacity;
                    HarvestBoxContainer.Opacity = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.BackgroundOpacity;
                    SetMode(UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.CompactMode);
                }
                base.ApplySettings();
            }));
        }

        private void SetMode(bool IsCompact) {
            if (IsCompact) {
                this.BaseHeight = this.Height = 130;
                
            } else {
                this.BaseHeight = this.Height = 205;
            }
            foreach (FertilizerControl fC in HarvestBoxFertilizerHolder.Children) {
                fC.SetMode(IsCompact);
            }
        }

        public void ScaleWidget(double NewScaleX, double NewScaleY) {
            if (NewScaleX <= 0.2) return;
            Width = BaseWidth * NewScaleX;
            //Height = BaseHeight * NewScaleY;
            this.HarvestBoxComponent.LayoutTransform = new ScaleTransform(NewScaleX, NewScaleY);
            this.DefaultScaleX = NewScaleX;
            this.DefaultScaleY = NewScaleY;
        }

        public void SetContext(Player ctx) {
            PlayerContext = ctx;
            HookEvents();
        }

        private void Dispatch(Action function) {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, function);
        }

        private void HookEvents() {
            PlayerContext.OnZoneChange += ChangeHarvestBoxState;
            Context.OnCounterChange += OnCounterChange;
            
            PlayerContext.Activity.OnNaturalSteamChange += OnNaturalSteamFuelChange;
            PlayerContext.Activity.OnStoredSteamChange += OnStoredSteamFuelChange;
            PlayerContext.Activity.OnTailraidersDaysChange += OnTailraidersQuestChange;
            PlayerContext.Activity.OnArgosyDaysChange += OnArgosyDaysChange;
        }

        public void UnhookEvents() {
            DestroyFertilizerControls();
            PlayerContext.OnZoneChange -= ChangeHarvestBoxState;
            Context.OnCounterChange -= OnCounterChange;

            PlayerContext.Activity.OnNaturalSteamChange -= OnNaturalSteamFuelChange;
            PlayerContext.Activity.OnStoredSteamChange -= OnStoredSteamFuelChange;
            PlayerContext.Activity.OnArgosyDaysChange -= OnArgosyDaysChange;
            PlayerContext.Activity.OnTailraidersDaysChange -= OnTailraidersQuestChange;
            PlayerContext = null;
        }

        private void CreateFertilizerControls() {
            for (int i = 0; i < 4; i++) {
                FertilizerControl fC = new FertilizerControl();
                fC.SetContext(Context.Box[i]);
                HarvestBoxFertilizerHolder.Children.Add(fC);
            }
        }

        private void DestroyFertilizerControls() {
            foreach (FertilizerControl control in HarvestBoxFertilizerHolder.Children) {
                control.UnhookEvents();
            }
            HarvestBoxFertilizerHolder.Children.Clear();
        }

        private void OnTailraidersQuestChange(object source, DaysLeftEventArgs args) {
            Dispatch(() => {
                this.TailraidersWarnIcon.Visibility = (args.Modifier && args.Days == 0 ) ? Visibility.Visible : Visibility.Hidden;
                TailraidersDaysText.Text = args.Days.ToString();
            });
        }

        private void OnArgosyDaysChange(object source, DaysLeftEventArgs args) {
            Dispatch(() => {
                this.ArgosyWarnIcon.Visibility = args.Modifier ? Visibility.Visible : Visibility.Hidden;
                ArgosyDaysText.Text = args.Days.ToString();
            });
            
        }

        private void OnStoredSteamFuelChange(object source, SteamFuelEventArgs args) {
            Dispatch(() => {
                this.StoredFuelText.Text = FormatToK(args.Available);
            });
        }

        private void OnNaturalSteamFuelChange(object source, SteamFuelEventArgs args) {
            Dispatch(() => {
                this.SteamFuelWarnIcon.Visibility = args.Available >= args.Max ? Visibility.Visible : Visibility.Hidden;
                this.NaturalFuelText.Text = FormatToK(args.Available);
            });
        }

        private void ChangeHarvestBoxState(object source, EventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                if (UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.AlwaysShow) {
                    WidgetHasContent = true;
                } else {
                    if (PlayerContext.InHarvestZone) { WidgetHasContent = true; }
                    else { WidgetHasContent = false; }
                }
                ChangeVisibility();
            }));
        }

        private void OnCounterChange(object source, HarvestBoxEventArgs args) {
            Dispatch(() => {
                this.HarvestBoxItemsCounter.Content = $"{args.Counter}/{args.Max}";
            });
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            this.UnhookEvents();
            this.PlayerContext = null;
            this.IsClosed = true;
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) {
                this.MoveWidget();
                SaveSettings();
            }
        }

        private void OnMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if (this.MouseOver) {
                if (e.Delta > 0) {
                    ScaleWidget(DefaultScaleX + 0.05, DefaultScaleY + 0.05);
                } else {
                    ScaleWidget(DefaultScaleX - 0.05, DefaultScaleY - 0.05);
                }
            }
        }

        // Helper
        private string FormatToK(int value) {
            if (value >= 1000000) return $"{value / 1000000}M";
            if (value >= 1000) return $"{value / 1000}K";
            return value.ToString();
        }
    }
}
