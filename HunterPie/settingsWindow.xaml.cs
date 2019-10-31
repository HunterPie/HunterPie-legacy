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

        private void TypeColor(object sender, KeyEventArgs e) {
            char[] HEX_CHARS = new char[16] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            Console.WriteLine(e.Key);
            if (HEX_CHARS.Contains(e.Key.ToString()[e.Key.ToString().Length - 1])) {
                e.Handled = false;
            } else {
                e.Handled = true;
            }
            return;
        }

        private void TypeColor(object sender, TextChangedEventArgs e) {

        }

        private void TypeNumber(object sender, KeyEventArgs e) {
            TextBox source = (TextBox)sender;
            
            Console.WriteLine(source.Text.ToString());
        }
    }
}
