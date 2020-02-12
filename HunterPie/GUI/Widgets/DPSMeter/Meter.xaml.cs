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

        Party Context;

        public Meter() {
            InitializeComponent();
        }

        public void SetContext(Party ctx) {
            Context = ctx;
            PassContextToPlayerComponents();
        }

        private void PassContextToPlayerComponents() {
            for (int i = 0; i < Context.MaxSize; i++) {
                Parts.PartyMember pMember = (Parts.PartyMember)this.Party.Children[i];
                pMember.SetContext(Context[i], Context);
            }
        }
    }
}
