using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HunterPie.Core;
using HunterPie.Core.Events;

namespace HunterPie.GUI.Widgets.DPSMeter.Parts
{
    /// <summary>
    /// Interaction logic for PartyMember.xaml
    /// </summary>
    public partial class PartyMember : UserControl, IEquatable<PartyMember>, IComparable<PartyMember>
    {

        public Member Context { get; private set; }
        Party partyContext;

        public string PlayerName
        {
            get { return (string)GetValue(PlayerNameProperty); }
            set { SetValue(PlayerNameProperty, value); }
        }
        public static readonly DependencyProperty PlayerNameProperty =
            DependencyProperty.Register("PlayerName", typeof(string), typeof(PartyMember));

        public short HighRank
        {
            get { return (short)GetValue(HighRankProperty); }
            set { SetValue(HighRankProperty, value); }
        }
        public static readonly DependencyProperty HighRankProperty =
            DependencyProperty.Register("HighRank", typeof(short), typeof(PartyMember));

        public short MasterRank
        {
            get { return (short)GetValue(MasterRankProperty); }
            set { SetValue(MasterRankProperty, value); }
        }
        public static readonly DependencyProperty MasterRankProperty =
            DependencyProperty.Register("MasterRank", typeof(short), typeof(PartyMember));

        public float DamagePercentage
        {
            get { return (float)GetValue(DamagePercentageProperty); }
            set { SetValue(DamagePercentageProperty, value); }
        }
        public static readonly DependencyProperty DamagePercentageProperty =
            DependencyProperty.Register("DamagePercentage", typeof(float), typeof(PartyMember));

        public int Damage
        {
            get { return (int)GetValue(DamageProperty); }
            set { SetValue(DamageProperty, value); }
        }
        public static readonly DependencyProperty DamageProperty =
            DependencyProperty.Register("Damage", typeof(int), typeof(PartyMember));

        public string DPS
        {
            get { return (string)GetValue(DPSProperty); }
            set { SetValue(DPSProperty, value); }
        }
        public static readonly DependencyProperty DPSProperty =
            DependencyProperty.Register("DPS", typeof(string), typeof(PartyMember));

        public ImageSource ClassIcon
        {
            get { return (ImageSource)GetValue(ClassIconProperty); }
            set { SetValue(ClassIconProperty, value); }
        }
        public static readonly DependencyProperty ClassIconProperty =
            DependencyProperty.Register("ClassIcon", typeof(ImageSource), typeof(PartyMember));

        public PartyMember(string Color)
        {
            InitializeComponent();
            ChangeColor(Color);
            UpdateDamageTextSettings();
        }

        public void SetContext(Member ctx, Party pctx)
        {
            Context = ctx;
            partyContext = pctx;
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
            partyContext = null;
        }

        private void OnPlayerSpawn(object source, PartyMemberEventArgs args)
        {
            Dispatch(() =>
            {
                PlayerName = args.Name;
                MasterRank = Context.MR;
                HighRank = Context.HR;
                if (Context.IsPartyLeader) PartyLeader.Visibility = Visibility.Visible;
                ClassIcon = args.Weapon == null ? null : (ImageSource)TryFindResource(args.Weapon);
                Visibility = args.IsInParty ? Visibility.Visible : Visibility.Collapsed;
                DPS = $"{GetCurrentDps():0.00}/s";
                Damage = Context.Damage;
                DamagePercentage = Context.DamagePercentage;
                PlayerDPSBar.Width = Context.DamagePercentage * PlayerDPSBar.MaxWidth;
            });
        }

        private void OnPlayerWeaponChange(object source, PartyMemberEventArgs args) => Dispatch(() =>
        {
            ClassIcon = args.Weapon == null ? null : (ImageSource)TryFindResource(args.Weapon);
        });

        public void UpdateDamage()
        {
            Dispatch(() =>
            {
                Damage = Context.Damage;
                DPS = $"{GetCurrentDps():0.00}/s";
                DamagePercentage = Context.DamagePercentage;
                PlayerDPSBar.Width = Context.DamagePercentage * PlayerDPSBar.MaxWidth;
                if (ConfigManager.Settings.Overlay.DPSMeter.ShowOnlyMyself)
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
            float TimeElapsed = (float)partyContext.Epoch.TotalSeconds;
            Dispatch(() =>
            {
                PlayerName = Context.Name;
                MasterRank = Context.MR;
                HighRank = Context.HR;
                if (Context.IsPartyLeader)
                    PartyLeader.Visibility = Visibility.Visible;
                Damage = Context.Damage;
                DPS = $"{GetCurrentDps():0.00}/s";
                DamagePercentage = Context.DamagePercentage;
                ClassIcon = Context.WeaponIconName == null ? null : (ImageSource)TryFindResource(Context.WeaponIconName);
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

        private float GetCurrentDps()
        {
            var damage = Context.Damage;
            var time = partyContext.Epoch.TotalSeconds - partyContext.TimeDifference.TotalSeconds;
            time = Math.Max(time, Meter.MIN_DPS_TIME_PERIOD_SECONDS);
            return damage / (float) time;
        }

        private void Dispatch(Action f) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, f);

        public bool Equals(PartyMember other)
        {
            if (other != null)
                return other.Context.Damage == Context.Damage;
            return false;
        }

        public int CompareTo(PartyMember other)
        {
            if (other != null)
            {
                int delta = other.Context.Damage - Context.Damage;
                return delta == 0 ? 0 : delta > 0 ? 1 : -1;
            }
            return 0;
        }

        public void UpdateDamageTextSettings()
        {
            bool dpsEnabled = ConfigManager.Settings.Overlay.DPSMeter.ShowDPSWheneverPossible;
            bool totalEnabled = ConfigManager.Settings.Overlay.DPSMeter.ShowTotalDamage;

            Percentage.Width = (dpsEnabled || totalEnabled) ? 54 : 132;

            DamagePerSecond.Visibility = dpsEnabled ? Visibility.Visible : Visibility.Collapsed;
            TotalDamage.Visibility = totalEnabled ? Visibility.Visible : Visibility.Collapsed;

            if (dpsEnabled || totalEnabled)
            {
                var noPadding = new Thickness(0, 0, 0, 0);
                var padding = new Thickness(0, 10, 0, 0);

                DamagePerSecond.Height = totalEnabled ? 23 : 46;
                DamagePerSecond.Padding = totalEnabled ? noPadding : padding;

                TotalDamage.Height = dpsEnabled ? 23 : 46;
                TotalDamage.Padding = dpsEnabled ? noPadding : padding;
            }
        }
    }
}
