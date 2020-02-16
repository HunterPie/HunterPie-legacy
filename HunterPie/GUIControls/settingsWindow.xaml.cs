using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;


namespace HunterPie.GUIControls {
    /// <summary>
    /// Interaction logic for settingsWindow.xaml
    /// </summary>
    public partial class settingsWindow : UserControl {
        public string fullGamePath = "";
        public string fullLaunchArgs = "";
        private string[] AvailableBranches = new string[2] { "master", "BETA" };

        public settingsWindow()
        {
            InitializeComponent();
            PopulateBranchBox();
            PopulateLanguageBox();
        }

        private void PopulateBranchBox() {
            foreach (string branch in AvailableBranches) {
                branchesCombobox.Items.Add(branch);
            }
        }

        private void PopulateLanguageBox() { 
            foreach (string filename in Directory.GetFiles("Languages")) {
                LanguageFilesCombobox.Items.Add(filename);
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

        private void TypeNumber(object sender, TextChangedEventArgs e) {
            char[] NUMBERS = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            TextBox obj = (TextBox)sender;
            int offset = 0;
            foreach (char n in obj.Text) {
                if (!NUMBERS.Contains(n)) {
                    break;
                } else {
                    offset++;
                }
            }
            obj.Text = obj.Text.Substring(0, offset);
            if (obj.Text.Length == 0) obj.Text = "0";
            obj.CaretIndex = offset;
        }

        private void selectPathBttn_Click(object sender, System.Windows.RoutedEventArgs e) {
            using (var filePicker = new System.Windows.Forms.OpenFileDialog()) {
                filePicker.Filter = "Executable|MonsterHunterWorld.exe";
                System.Windows.Forms.DialogResult result = filePicker.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK) {
                    fullGamePath = filePicker.FileName;
                    if (filePicker.FileName.Length > 15) {
                        int i = (fullGamePath.Length / 2) - 10;
                        selectPathBttn.Content = "..." + fullGamePath.Substring(i);
                        return;
                    }
                    selectPathBttn.Content = fullGamePath;
                }

            }
        }

        private void argsTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            fullLaunchArgs = argsTextBox.Text;
        }

        private void argsTextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e) {
            if (argsTextBox.Text == "No arguments") argsTextBox.Text = "";
        }

        private void argsTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e) {
            if (argsTextBox.Text == "") argsTextBox.Text = "No arguments";
        }
    }
}
