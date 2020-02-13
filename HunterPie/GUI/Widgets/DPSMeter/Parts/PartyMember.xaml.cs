using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HunterPie.Core;

namespace HunterPie.GUI.Widgets.DPSMeter.Parts {
    /// <summary>
    /// Interaction logic for PartyMember.xaml
    /// </summary>
    public partial class PartyMember : UserControl {
        
        public object Color {
            get { return (object)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(object), typeof(Rectangle), new PropertyMetadata(0));

    public static readonly DependencyProperty ClassProperty = DependencyProperty.Register("Class", typeof(object), typeof(Image), new PropertyMetadata(0));

        Member Context;
        Party PartyContext;

        public PartyMember() { 
            InitializeComponent();
            this.DataContext = this;
            this.Visibility = Visibility.Collapsed;
        }

        ~PartyMember() {
            UnhookEvents();
        }

        public void SetContext(Member ctx, Party pctx) {
            Context = ctx;
            PartyContext = pctx;
            HookEvents();
        }

        private void HookEvents() {
            Context.OnSpawn += OnPlayerSpawn;
            Context.OnDamageChange += OnPlayerDamageChange;
            Context.OnWeaponChange += OnPlayerWeaponChange;
        }

        public void UnhookEvents() {
            Context.OnSpawn -= OnPlayerSpawn;
            Context.OnDamageChange -= OnPlayerDamageChange;
            Context.OnWeaponChange -= OnPlayerWeaponChange;
        }

        private void OnPlayerSpawn(object source, PartyMemberEventArgs args) {
            Dispatch(() => {
                PlayerName.Content = args.Name;
                PlayerClassIcon.Source = args.Weapon == null ? null : (ImageSource)TryFindResource(args.Weapon);
                this.Visibility = args.IsInParty ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void OnPlayerWeaponChange(object source, PartyMemberEventArgs args) {
            Dispatch(() => {
                PlayerClassIcon.Source = args.Weapon == null ? null : (ImageSource)TryFindResource(args.Weapon);
            });
        }

        private void OnPlayerDamageChange(object source, PartyMemberEventArgs args) {
            float percentage;
            string DamageText;
            if (PartyContext.TotalDamage == 0) {
                percentage = 0;
            } else {
                percentage =  args.Damage / (float)PartyContext.TotalDamage;
            }
            if (PartyContext.ShowDPS) {
                float TimeElapsed = (float)PartyContext.Epoch.TotalSeconds;
                TimeElapsed = TimeElapsed > 0 ? TimeElapsed : 1;
                DamageText = $"{args.Damage / TimeElapsed:0.00}/s ({percentage * 100:0}%)";
            } else {
                DamageText = $"{args.Damage} ({percentage * 100:0}%)";
            }
            Dispatch(() => {
                DPSText.Content = DamageText;
                PlayerDPSBar.Width = percentage * PlayerDPSBar.MaxWidth;
            });
        }

        private void Dispatch(Action f) {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, f);
        }

    }
}
