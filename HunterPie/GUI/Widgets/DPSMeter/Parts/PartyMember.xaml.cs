using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HunterPie.Core;

namespace HunterPie.GUI.Widgets.DPSMeter.Parts
{
    /// <summary>
    /// Interaction logic for PartyMember.xaml
    /// </summary>
    public partial class PartyMember : UserControl, IEquatable<PartyMember>, IComparable<PartyMember>
    {

        public Member Context { get; private set; }
        Party PartyContext;

        public PartyMember(string Color)
        {
            InitializeComponent();
            ChangeColor(Color);
            UpdateDamageTextSettings();
        }

        public void SetContext(Member ctx, Party pctx)
        {
            Context = ctx;
            PartyContext = pctx;
            HookEvents();
            SetPlayerInformation();
        }

        private void HookEvents()
        {
            Context.OnSpawn += OnPlayerSpawn;
            Context.OnWeaponChange += OnPlayerWeaponChange;
        }

        public void UnhookEvents()
        {
            Context.OnSpawn -= OnPlayerSpawn;
            Context.OnWeaponChange -= OnPlayerWeaponChange;
            Context = null;
            PartyContext = null;
        }

        private void OnPlayerSpawn(object source, PartyMemberEventArgs args)
        {
            float TimeElapsed = (float)PartyContext.Epoch.TotalSeconds;
            Dispatch(() =>
            {
                PlayerName.Text = args.Name;
                MasterRank.Text = Context.MR.ToString();
                HighRank.Text = Context.HR.ToString();
                if (Context.IsPartyLeader) PartyLeader.Visibility = Visibility.Visible;
                PlayerClassIcon.Source = args.Weapon == null ? null : (ImageSource)TryFindResource(args.Weapon);
                Visibility = args.IsInParty ? Visibility.Visible : Visibility.Collapsed;
                DamagePerSecond.Text = $"{Context.Damage / TimeElapsed:0.00}/s";
                TotalDamage.Text = Context.Damage.ToString();
                Percentage.Text = $"{Context.DamagePercentage * 100:0.0}%";
                PlayerDPSBar.Width = Context.DamagePercentage * PlayerDPSBar.MaxWidth;
            });
        }

        private void OnPlayerWeaponChange(object source, PartyMemberEventArgs args) => Dispatch(() =>
        {
            PlayerClassIcon.Source = args.Weapon == null ? null : (ImageSource)TryFindResource(args.Weapon);
        });

        public void UpdateDamage()
        {
            float TimeElapsed = (float)PartyContext.Epoch.TotalSeconds;
            Dispatch(() =>
            {
                DamagePerSecond.Text = $"{Context.Damage / TimeElapsed:0.00}/s";
                TotalDamage.Text = Context.Damage.ToString();
                Percentage.Text = $"{Context.DamagePercentage * 100:0.0}%";
                PlayerDPSBar.Width = Context.DamagePercentage * PlayerDPSBar.MaxWidth;
                if (UserSettings.PlayerConfig.Overlay.DPSMeter.ShowOnlyMyself)
                {
                    Visibility = Context.IsMe ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    Visibility = Context.IsInParty ? Visibility.Visible : Visibility.Collapsed;
                }
            });
        }

        public void SetPlayerInformation()
        {
            float TimeElapsed = (float)PartyContext.Epoch.TotalSeconds;
            Dispatch(() =>
            {
                PlayerName.Text = Context.Name;
                MasterRank.Text = Context.MR.ToString();
                HighRank.Text = Context.HR.ToString();
                if (Context.IsPartyLeader) PartyLeader.Visibility = Visibility.Visible;
                DamagePerSecond.Text = $"{Context.Damage / TimeElapsed:0.00}/s";
                TotalDamage.Text = Context.Damage.ToString();
                Percentage.Text = $"{Context.DamagePercentage * 100:0.0}%";
                PlayerClassIcon.Source = Context.WeaponIconName == null ? null : (ImageSource)TryFindResource(Context.WeaponIconName);
                Visibility = Context.IsInParty ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        public void ChangeColor(string hexColor)
        {
            Color PlayerColor = (Color)ColorConverter.ConvertFromString(hexColor);
            LinearGradientBrush ShinyEffect = new LinearGradientBrush()
            {
                StartPoint = new Point(1, 1),
                EndPoint = new Point(1, 0)
            };
            ShinyEffect.GradientStops.Add(new GradientStop(PlayerColor, 0.055));
            PlayerColor.A = 0x55;
            ShinyEffect.GradientStops.Add(new GradientStop(PlayerColor, 0.064));
            ShinyEffect.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#00000000"), 1));
            ShinyEffect.Freeze();
            PlayerDPSBar.Fill = ShinyEffect;
        }

        private void Dispatch(Action f) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, f);

        public bool Equals(PartyMember other)
        {
            if (other == null) return false;
            else return Context.Damage.Equals(other.Context.Damage);
        }

        public int CompareTo(PartyMember other)
        {
            if (Context.Damage > other.Context.Damage) return -1;
            else if (Context.Damage < other.Context.Damage) return 1;
            else if (Equals(other)) return 0;
            else return 0;
        }

        public void UpdateDamageTextSettings()
        {
            bool DPSEnabled = UserSettings.PlayerConfig.Overlay.DPSMeter.ShowDPSWheneverPossible;
            bool TotalEnabled = UserSettings.PlayerConfig.Overlay.DPSMeter.ShowTotalDamage;
            // So many if and elses :peepoCry:
            if (DPSEnabled)
            {
                if (TotalEnabled)
                {
                    TotalDamage.Height = 23;
                    TotalDamage.Padding = new Thickness(0, 0, 0, 0);
                    TotalDamage.Visibility = Visibility.Visible;

                    DamagePerSecond.Height = 23;
                    DamagePerSecond.Padding = new Thickness(0, 0, 0, 0);
                }
                else
                {

                    DamagePerSecond.Height = 46;
                    DamagePerSecond.Padding = new Thickness(0, 10, 0, 0);
                }
                DamagePerSecond.Visibility = Visibility.Visible;
            }
            else
            {
                DamagePerSecond.Visibility = Visibility.Collapsed;
            }
            if (TotalEnabled)
            {
                if (!DPSEnabled)
                {
                    TotalDamage.Height = 46;
                    TotalDamage.Padding = new Thickness(0, 10, 0, 0);
                }
                TotalDamage.Visibility = Visibility.Visible;
            }
            else
            {
                TotalDamage.Visibility = Visibility.Collapsed;
            }
            if (!DPSEnabled && !TotalEnabled)
            {
                Percentage.Width = 132;
            }
            else
            {
                Percentage.Width = 54;
            }
        }
    }
}
