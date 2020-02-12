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
                PlayerName.Text = args.Name;
                PlayerClassIcon.Source = args.Weapon == null ? null : (ImageSource)TryFindResource(args.Weapon);
                PlayerDPSBar.Width = 0;
                DPSText.Text = "0/s (0%)";
                this.Visibility = args.IsInParty ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void OnPlayerWeaponChange(object source, PartyMemberEventArgs args) {
            Dispatch(() => { PlayerClassIcon.Source = args.Weapon == null ? null : (ImageSource)TryFindResource(args.Weapon); });
        }

        private void OnPlayerDamageChange(object source, PartyMemberEventArgs args) {
            float percentage;
            if (PartyContext.TotalDamage == 0) {
                percentage = 0;
            } else {
                percentage =  args.Damage / PartyContext.TotalDamage;
            }
            Dispatch(() => {
                DPSText.Text = $"{args.Damage} ({percentage * 100:0}%)";
                PlayerDPSBar.Width = percentage * PlayerDPSBar.MaxWidth;
            });
        }

        private void Dispatch(Action f) {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, f);
        }

    }
}
