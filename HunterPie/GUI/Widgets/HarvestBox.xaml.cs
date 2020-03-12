using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HunterPie.Core;

namespace HunterPie.GUI.Widgets {

    public partial class HarvestBox : Widget {

        Player PlayerContext;
        Core.HarvestBox Context {
            get { return PlayerContext?.Harvest; }
        }

        // Animations
        Storyboard ANIM_FERTILIZER_EXPIRE;

        public HarvestBox(Player Context) {
            InitializeComponent();
            BaseWidth = Width;
            BaseHeight = Height;
            WidgetType = 3;
            ApplySettings();
            SetWindowFlags();
            SetContext(Context);
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

        public override void ApplySettings() {
            bool ShowEverywhere = false;
            if (UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.AlwaysShow) {
                ShowEverywhere = true;
            } else {
                if (PlayerContext != null && PlayerContext.inHarvestZone) { ShowEverywhere = true; } else { ShowEverywhere = false; }
            }
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                this.Top = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Position[1] + UserSettings.PlayerConfig.Overlay.Position[1];
                this.Left = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Position[0] + UserSettings.PlayerConfig.Overlay.Position[0];
                this.WidgetActive = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Enabled;
                this.WidgetHasContent = ShowEverywhere;
                this.ScaleWidget(UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Scale, UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Scale);
                base.ApplySettings();
            }));
        }

        public void ScaleWidget(double NewScaleX, double NewScaleY) {
            if (NewScaleX <= 0.2) return;
            Width = BaseWidth * NewScaleX;
            Height = BaseHeight * NewScaleY;
            this.HarvestBoxComponent.LayoutTransform = new ScaleTransform(NewScaleX, NewScaleY);
            this.DefaultScaleX = NewScaleX;
            this.DefaultScaleY = NewScaleY;
        }

        public void SetContext(Player ctx) {
            PlayerContext = ctx;
            GetAnimations();
            HookEvents();
        }

        private void Dispatch(Action function) {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, function);
        }

        private void GetAnimations() {
            ANIM_FERTILIZER_EXPIRE = FindResource("FertilizerExpiring") as Storyboard;
        }

        private void HookEvents() {
            PlayerContext.OnZoneChange += ChangeHarvestBoxState;
            Context.OnCounterChange += OnCounterChange;
            // TODO: Make fertilizers a separate usercontrol
            Context.Box[0].OnAmountUpdate += UpdateFirstFertilizer;
            Context.Box[0].OnFertilizerChange += UpdateFirstFertilizer;

            Context.Box[1].OnAmountUpdate += UpdateSecondFertilizer;
            Context.Box[1].OnFertilizerChange += UpdateSecondFertilizer;

            Context.Box[2].OnAmountUpdate += UpdateThirdFertilizer;
            Context.Box[2].OnFertilizerChange += UpdateThirdFertilizer;

            Context.Box[3].OnAmountUpdate += UpdateFourthFertilizer;
            Context.Box[3].OnFertilizerChange += UpdateFourthFertilizer;

            PlayerContext.Activity.OnNaturalSteamChange += OnNaturalSteamFuelChange;
            PlayerContext.Activity.OnStoredSteamChange += OnStoredSteamFuelChange;
            PlayerContext.Activity.OnTailraidersDaysChange += OnTailraidersQuestChange;
            PlayerContext.Activity.OnArgosyDaysChange += OnArgosyDaysChange;
        }

        public void UnhookEvents() {
            PlayerContext.OnZoneChange -= ChangeHarvestBoxState;
            Context.OnCounterChange -= OnCounterChange;
            Context.Box[0].OnAmountUpdate -= UpdateFirstFertilizer;
            Context.Box[0].OnFertilizerChange -= UpdateFirstFertilizer;

            Context.Box[1].OnAmountUpdate -= UpdateSecondFertilizer;
            Context.Box[1].OnFertilizerChange -= UpdateSecondFertilizer;

            Context.Box[2].OnAmountUpdate -= UpdateThirdFertilizer;
            Context.Box[2].OnFertilizerChange -= UpdateThirdFertilizer;

            Context.Box[3].OnAmountUpdate -= UpdateFourthFertilizer;
            Context.Box[3].OnFertilizerChange -= UpdateFourthFertilizer;

            PlayerContext.Activity.OnNaturalSteamChange -= OnNaturalSteamFuelChange;
            PlayerContext.Activity.OnStoredSteamChange -= OnStoredSteamFuelChange;
            PlayerContext.Activity.OnArgosyDaysChange -= OnArgosyDaysChange;
            PlayerContext.Activity.OnTailraidersDaysChange -= OnTailraidersQuestChange;
            PlayerContext = null;
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
                this.StoredFuelText.Text = args.Available.ToString();
            });
        }

        private void OnNaturalSteamFuelChange(object source, SteamFuelEventArgs args) {
            Dispatch(() => {
                this.SteamFuelWarnIcon.Visibility = args.Available >= args.Max ? Visibility.Visible : Visibility.Hidden;
                this.NaturalFuelText.Text = args.Available.ToString();
            });
        }

        private void ChangeHarvestBoxState(object source, EventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                if (UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.AlwaysShow) {
                    WidgetHasContent = true;
                } else {
                    if (PlayerContext.inHarvestZone) { WidgetHasContent = true; }
                    else { WidgetHasContent = false; }
                }
                ChangeVisibility();
            }));
        }

        private void UpdateFirstFertilizer(object source, FertilizerEventArgs args) {
            bool ApplyAnimation = false;
            if (args.Amount <= 4) ApplyAnimation = true;
            Dispatch(() => {
                if (ApplyAnimation) {
                    ANIM_FERTILIZER_EXPIRE.Begin(fert1Counter, true);
                    ANIM_FERTILIZER_EXPIRE.Begin(fert1Name, true);
                } else {
                    ANIM_FERTILIZER_EXPIRE.Remove(fert1Counter);
                    ANIM_FERTILIZER_EXPIRE.Remove(fert1Name);
                }
                this.fert1Name.Content = args.Name;
                this.fert1Counter.Content = $"x{args.Amount}";
            });
        }

        private void UpdateSecondFertilizer(object source, FertilizerEventArgs args) {
            bool ApplyAnimation = false;
            if (args.Amount <= 4) ApplyAnimation = true;
            Dispatch(() => {
                if (ApplyAnimation) {
                    ANIM_FERTILIZER_EXPIRE.Begin(fert2Counter, true);
                    ANIM_FERTILIZER_EXPIRE.Begin(fert2Name, true);
                } else {
                    ANIM_FERTILIZER_EXPIRE.Remove(fert2Counter);
                    ANIM_FERTILIZER_EXPIRE.Remove(fert2Name);
                }
                this.fert2Name.Content = args.Name;
                this.fert2Counter.Content = $"x{args.Amount}";
            });
        }

        private void UpdateThirdFertilizer(object source, FertilizerEventArgs args) {
            bool ApplyAnimation = false;
            if (args.Amount <= 4) ApplyAnimation = true;
            Dispatch(() => {
                if (ApplyAnimation) {
                    ANIM_FERTILIZER_EXPIRE.Begin(fert3Counter, true);
                    ANIM_FERTILIZER_EXPIRE.Begin(fert3Name, true);
                } else {
                    ANIM_FERTILIZER_EXPIRE.Remove(fert3Counter);
                    ANIM_FERTILIZER_EXPIRE.Remove(fert3Name);
                }
                this.fert3Name.Content = args.Name;
                this.fert3Counter.Content = $"x{args.Amount}";
            });
        }

        private void UpdateFourthFertilizer(object source, FertilizerEventArgs args) {
            bool ApplyAnimation = false;
            if (args.Amount <= 4) ApplyAnimation = true;
            Dispatch(() => {
                if (ApplyAnimation) {
                    ANIM_FERTILIZER_EXPIRE.Begin(fert4Counter, true);
                    ANIM_FERTILIZER_EXPIRE.Begin(fert4Name, true);
                } else {
                    ANIM_FERTILIZER_EXPIRE.Remove(fert4Counter);
                    ANIM_FERTILIZER_EXPIRE.Remove(fert4Name);
                }
                this.fert4Name.Content = args.Name;
                this.fert4Counter.Content = $"x{args.Amount}";
            });
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

        private void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            this.MouseOver = true;
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) {
                this.MoveWidget();
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

        private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            this.MouseOver = false;
        }
    }
}
