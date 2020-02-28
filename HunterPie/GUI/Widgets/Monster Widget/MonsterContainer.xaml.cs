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

namespace HunterPie.GUI.Widgets {
    /// <summary>
    /// Interaction logic for MonsterContainer.xaml
    /// </summary>
    public partial class MonsterContainer : UserControl {

        Game Context;
        MonsterHealth f_MonsterWidget;
        MonsterHealth s_MonsterWidget;
        MonsterHealth t_MonsterWidget;

        public MonsterContainer() {
            InitializeComponent();
        }

        public void SetContext(Game Ctx) {
            Context = Ctx;
        }

        private void HookEvents() {
            Context.Player.OnPeaceZoneEnter += OnPeaceZoneEnter;
            Context.Player.OnPeaceZoneLeave += OnPeaceZoneLeave;
        }

        public void UnhookEvents() {
            Context.Player.OnPeaceZoneEnter -= OnPeaceZoneEnter;
            Context.Player.OnPeaceZoneLeave -= OnPeaceZoneLeave;
        }

        private void OnPeaceZoneLeave(object source, EventArgs args) {
            throw new NotImplementedException();
        }

        private void OnPeaceZoneEnter(object source, EventArgs args) {
            f_MonsterWidget?.UnhookEvents();
        }
    }
}
