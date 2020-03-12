using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using HunterPie.Core;
using System.Windows.Media;

namespace HunterPie.GUIControls {
    /// <summary>
    /// Interaction logic for settingsWindow.xaml
    /// </summary>
    public partial class settingsWindow : UserControl {
        public string fullGamePath = "";
        public string fullLaunchArgs = "";
        private string[] AvailableBranches = new string[2] { "master", "BETA" };
        private KeyboardHook KeyboardInputHook = new KeyboardHook();

        public settingsWindow() {
            InitializeComponent();
            KeyboardInputHook.InstallHooks();
            KeyboardInputHook.OnKeyboardKeyPress += KeyboardInputHook_OnKeyboardKeyPress;
            PopulateBuffTrays();
            PopulateBranchBox();
            PopulateLanguageBox();
        }

        public void UnhookEvents() {
            KeyboardInputHook.UninstallHooks();
            KeyboardInputHook.OnKeyboardKeyPress -= KeyboardInputHook_OnKeyboardKeyPress;

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

        private void TypeNumber(object sender, TextChangedEventArgs e) {
            char[] NUMBERS = new char[11] { '-', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            TextBox obj = (TextBox)sender;
            int offset = 0;
            int charIndex = 0;
            foreach (char n in obj.Text) {
                if (charIndex > 0 && n == '-') { break; }
                if (!NUMBERS.Contains(n)) {
                    break;
                } else {
                    offset++;
                }
                charIndex++;
            }
            obj.Text = obj.Text.Substring(0, offset);
            if (obj.Text.Length == 0) obj.Text = "0";
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
                        selectPathBttn.Focusable = false;
                        return;
                    }
                    selectPathBttn.Content = fullGamePath;
                }
                selectPathBttn.Focusable = false;
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

        private void SelectPathBttn_LostFocus(object sender, System.Windows.RoutedEventArgs e) {
            selectPathBttn.Focusable = true;
        }

        private bool CanChooseKey = false;
        private bool HasFocus = false;
        public KeyboardHookHelper.KeyboardKeys KeyChoosen = KeyboardHookHelper.GetKeyboardKeyByID(UserSettings.PlayerConfig.Overlay.ToggleDesignModeKey);
        private void SelectDesignModeKeyBind(object sender, System.Windows.RoutedEventArgs e) {
            CanChooseKey = true;
            HasFocus = true;
        }

        private void KeyboardInputHook_OnKeyboardKeyPress(object sender, KeyboardInputEventArgs e) {
            if (CanChooseKey && HasFocus) {
                KeyChoosen = (KeyboardHookHelper.KeyboardKeys)Enum.Parse(typeof(KeyboardHookHelper.KeyboardKeys), e.Key.ToString());
                this.DesignModeKeyCode.Content = KeyChoosen.ToString();
                DesignModeKeyCode.Focusable = false;
                CanChooseKey = false;
            }
        }

        private void OnKeybindingButtonLoseFocus(object sender, System.Windows.RoutedEventArgs e) {
            HasFocus = false;
            CanChooseKey = false;
            DesignModeKeyCode.Focusable = true;
        }

        private void PopulateBuffTrays() {
            for (int i = 0; i < UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars; i++) {
                Custom_Controls.BuffBarSettingControl BuffBar = new Custom_Controls.BuffBarSettingControl() {
                    PresetName = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[i].Name,
                    Enabled = (bool)UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[i].Enabled
                };
                BuffTrays.Children.Add(BuffBar);
            }
            if (UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars >= 5) {
                AddNewBuffBar.Opacity = 0.5;
            } else if (UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars <= 1) {
                SubBuffBar.Opacity = 0.5;
            }
            NumberOfBuffBars.Text = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars.ToString();
        }

        private void AddNewBuffBarClick(object sender, MouseButtonEventArgs e) {
            if (UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars > 4) {
                AddNewBuffBar.Opacity = 0.5;
                return;
            }
            SubBuffBar.Opacity = 1;
            UserSettings.AddNewAbnormalityBar(1);
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars += 1;
            Custom_Controls.BuffBarSettingControl BuffBar = new Custom_Controls.BuffBarSettingControl() {
                PresetName = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets.LastOrDefault().Name,
                Enabled = (bool)UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets.LastOrDefault().Enabled
            };
            BuffTrays.Children.Add(BuffBar);
            NumberOfBuffBars.Text = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars.ToString();
            if (UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars >= 5) {
                AddNewBuffBar.Opacity = 0.5;
                SubBuffBar.Opacity = 1;
                return;
            }
        }

        private void SubBuffBarClick(object sender, MouseButtonEventArgs e) {
            if (UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars <= 1) {
                SubBuffBar.Opacity = 0.5;
                return;
            }
            AddNewBuffBar.Opacity = 1;
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars -= 1;
            if (BuffTrays.Children.Count - 1 > 0) {
                BuffTrays.Children.RemoveAt(BuffTrays.Children.Count - 1);
            }
            UserSettings.RemoveAbnormalityBars();
            NumberOfBuffBars.Text = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars.ToString();
            if (UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars <= 1) {
                SubBuffBar.Opacity = 0.5;
                AddNewBuffBar.Opacity = 1;
                return;
            }
        }

    }
}
