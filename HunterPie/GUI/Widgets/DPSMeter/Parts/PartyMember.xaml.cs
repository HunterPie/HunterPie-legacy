using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HunterPie.Core;

namespace HunterPie.GUI.Widgets.DPSMeter.Parts {
    /// <summary>
    /// Interaction logic for PartyMember.xaml
    /// </summary>
    public partial class PartyMember : UserControl, IEquatable<PartyMember>, IComparable<PartyMember> {
        
        public Member Context { get; private set; }
        Party PartyContext;

        public PartyMember(string Color) {
            InitializeComponent();
            ChangeColor(Color);
        }

        public void SetContext(Member ctx, Party pctx) {
            Context = ctx;
            PartyContext = pctx;
            HookEvents();
            SetPlayerInformation();
        }

        private void HookEvents() {
            Context.OnSpawn += OnPlayerSpawn;
            Context.OnWeaponChange += OnPlayerWeaponChange;
        }

        public void UnhookEvents() {
            Context.OnSpawn -= OnPlayerSpawn;
            Context.OnWeaponChange -= OnPlayerWeaponChange;
            Context = null;
            PartyContext = null;
        }

        private void OnPlayerSpawn(object source, PartyMemberEventArgs args) {
            Dispatch(() => {
                PlayerName.Text = args.Name;
                PlayerClassIcon.Source = args.Weapon == null ? null : (ImageSource)TryFindResource(args.Weapon);
                this.Visibility = args.IsInParty ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void OnPlayerWeaponChange(object source, PartyMemberEventArgs args) {
            Dispatch(() => {
                PlayerClassIcon.Source = args.Weapon == null ? null : (ImageSource)TryFindResource(args.Weapon);
            });
        }

        public void UpdateDamage() {
            float TimeElapsed = (float)PartyContext.Epoch.TotalSeconds;
            Dispatch(() => {
                DamagePerSecond.Text = $"{Context.Damage / TimeElapsed:0.00}/s";
                TotalDamage.Text = Context.Damage.ToString();
                Percentage.Text = $"{Context.DamagePercentage * 100:0.0}%";
                PlayerDPSBar.Width = Context.DamagePercentage * PlayerDPSBar.MaxWidth;
            });
        }

        public void SetPlayerInformation() {
            float TimeElapsed = (float)PartyContext.Epoch.TotalSeconds;
            Dispatch(() => {
                PlayerName.Text = Context.Name;
                DamagePerSecond.Text = $"{Context.Damage / TimeElapsed:0.00}/s";
                TotalDamage.Text = Context.Damage.ToString();
                Percentage.Text = $"{Context.DamagePercentage * 100:0.0}%";
                PlayerClassIcon.Source = Context.WeaponIconName == null ? null : (ImageSource)TryFindResource(Context.WeaponIconName);
                this.Visibility = Context.IsInParty ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        public void ChangeColor(string hexColor) {
            Color PlayerColor = (Color)ColorConverter.ConvertFromString(hexColor);
            LinearGradientBrush ShinyEffect = new LinearGradientBrush() {
                StartPoint = new Point(1, 1),
                EndPoint = new Point(1, 0)
            };
            ShinyEffect.GradientStops.Add(new GradientStop(PlayerColor, 0.055));
            PlayerColor.A = 0x55;
            ShinyEffect.GradientStops.Add(new GradientStop(PlayerColor, 0.064));
            ShinyEffect.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#00000000"), 1));
            ShinyEffect.Freeze();
            this.PlayerDPSBar.Fill = ShinyEffect;
        }

        private void Dispatch(Action f) {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, f);
        }

        public bool Equals(PartyMember other) {
            if (other == null) return false;
            else return this.Context.Damage.Equals(other.Context.Damage);
        }

        public int CompareTo(PartyMember other) {
            if (this.Context.Damage > other.Context.Damage) return -1;
            else if (this.Context.Damage < other.Context.Damage) return 1;
            else if (this.Equals(other)) return 0;
            else return 0;
        }
    }
}
