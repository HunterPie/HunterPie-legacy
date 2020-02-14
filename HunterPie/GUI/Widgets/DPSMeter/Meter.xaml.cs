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

namespace HunterPie.GUI.Widgets.DPSMeter {
    /// <summary>
    /// Interaction logic for DPSMeter.xaml
    /// </summary>
    public partial class Meter : UserControl {

        List<Parts.PartyMember> Players = new List<Parts.PartyMember>();
        Game GameContext;
        Party Context;

        public Meter() {
            InitializeComponent();
        }

        public void SetContext(Game ctx) {
            Context = ctx.Player.PlayerParty;
            GameContext = ctx;
            PassContextToPlayerComponents();
            HookEvents();

        }

        private void HookEvents() {
            Context.OnTotalDamageChange += OnTotalDamageChange;
            GameContext.Player.OnPeaceZoneEnter += OnPeaceZoneEnter;
            GameContext.Player.OnPeaceZoneLeave += OnPeaceZoneLeave;
        }

        private void OnPeaceZoneLeave(object source, EventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                Visibility = Visibility.Visible;
            }));
        }

        private void OnPeaceZoneEnter(object source, EventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                Visibility = Visibility.Hidden;
            }));
        }

        public void UnhookEvents() {
            GameContext.Player.OnPeaceZoneEnter -= OnPeaceZoneEnter;
            GameContext.Player.OnPeaceZoneLeave -= OnPeaceZoneLeave;
            Context.OnTotalDamageChange -= OnTotalDamageChange;
        }

        private void OnTotalDamageChange(object source, EventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                SortPlayersByDamage();
                Timer.Content = string.Format("{0:hh\\:mm\\:ss}", Context.Epoch);
            }));
        }

        private void PassContextToPlayerComponents() {
            for (int i = 0; i < Context.MaxSize; i++) {
                Parts.PartyMember pMember = new Parts.PartyMember {
                    Color = "#FFFFFF"
                };
                pMember.SetContext(Context[i], Context);
                Players.Add(pMember);
            }
            this.Party.ItemsSource = Players;
            
        }

        public void DestroyPlayerComponents() {
            Players.Clear();
            UnhookEvents();
        }

        private void SortPlayersByDamage() {
            Players.Sort(delegate (Parts.PartyMember x, Parts.PartyMember y) {
                return x.CompareTo(y);
            });
            Party.Items.Refresh();
        }

    }
}
