using System;
using System.Windows;
using System.Windows.Controls;
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
            ApplySettings();
            SetWindowFlags(this);
            SetContext(Context);
            this.Show();
        }

        public override void ApplySettings() {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                this.Top = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Position[1];
                this.Left = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Position[0];
                this.Visibility = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Enabled ? Visibility.Visible : Visibility.Hidden;
            }));
        }

        public void ScaleWidget(double NewScaleX, double NewScaleY) {
            Width = BaseWidth * NewScaleX;
            Height = BaseHeight * NewScaleY;
            this.HarvestBoxComponent.LayoutTransform = new ScaleTransform(NewScaleX, NewScaleY);
        }

        public void SetContext(Player ctx) {
            PlayerContext = ctx;
            GetAnimations();
            HookEvents();
            this.Show();
        }

        private void Dispatch(Action function) {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, function);
        }

        private void GetAnimations() {
            ANIM_FERTILIZER_EXPIRE = FindResource("FertilizerExpiring") as Storyboard;
        }

        private void HookEvents() {
            PlayerContext.OnVillageEnter += ShowHarvestBox;
            PlayerContext.OnVillageLeave += HideHarvestBox;
            Context.OnCounterChange += OnCounterChange;
            Context.Box[0].OnAmountUpdate += UpdateFirstFertilizer;
            Context.Box[0].OnFertilizerChange += UpdateFirstFertilizer;

            Context.Box[1].OnAmountUpdate += UpdateSecondFertilizer;
            Context.Box[1].OnFertilizerChange += UpdateSecondFertilizer;

            Context.Box[2].OnAmountUpdate += UpdateThirdFertilizer;
            Context.Box[2].OnFertilizerChange += UpdateThirdFertilizer;

            Context.Box[3].OnAmountUpdate += UpdateFourthFertilizer;
            Context.Box[3].OnFertilizerChange += UpdateFourthFertilizer;
        }

        public void UnhookEvents() {
            PlayerContext.OnVillageEnter -= ShowHarvestBox;
            PlayerContext.OnVillageLeave -= HideHarvestBox;
            Context.OnCounterChange -= OnCounterChange;
            Context.Box[0].OnAmountUpdate -= UpdateFirstFertilizer;
            Context.Box[0].OnFertilizerChange -= UpdateFirstFertilizer;

            Context.Box[1].OnAmountUpdate -= UpdateSecondFertilizer;
            Context.Box[1].OnFertilizerChange -= UpdateSecondFertilizer;

            Context.Box[2].OnAmountUpdate -= UpdateThirdFertilizer;
            Context.Box[2].OnFertilizerChange -= UpdateThirdFertilizer;

            Context.Box[3].OnAmountUpdate -= UpdateFourthFertilizer;
            Context.Box[3].OnFertilizerChange -= UpdateFourthFertilizer;
            PlayerContext = null;
        }


        private void ShowHarvestBox(object source, EventArgs args) {
            if (this.Visibility == Visibility.Visible || !UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Enabled) return;
            Dispatch(() => {
                this.Visibility = Visibility.Visible;
            });
        }

        private void HideHarvestBox(object source, EventArgs args) {
            if (this.Visibility == Visibility.Hidden) return;
            Dispatch(() => {
                this.Visibility = Visibility.Hidden;
            });
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
        }
    }
}
