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

namespace HunterPie {
    /// <summary>
    /// Interaction logic for settingsWindow.xaml
    /// </summary>
    public partial class settingsWindow : UserControl {

        private string[] AvailableBranches = new string[2] { "master", "BETA" };

        public settingsWindow()
        {
            InitializeComponent();
            PopulateBranchBox();
        }

        private void PopulateBranchBox() {
            foreach (string branch in AvailableBranches) {
                branchesCombobox.Items.Add(branch);
            }
        }
    }
}
