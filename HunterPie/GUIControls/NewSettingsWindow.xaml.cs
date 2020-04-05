using System;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using HunterPie.Core;
using System.Windows.Media.Animation;

namespace HunterPie.GUIControls {
    /// <summary>
    /// Interaction logic for NewSettingsWindow.xaml
    /// </summary>
    public partial class NewSettingsWindow : UserControl {
        public string fullGamePath = "";
        public string fullMonsterDataPath = "";
        public string fullLaunchArgs = "";
        private string[] AvailableBranches = new string[2] { "master", "BETA" };
        private KeyboardHook KeyboardInputHook = new KeyboardHook();

        public NewSettingsWindow() {
            InitializeComponent();
            KeyboardInputHook.InstallHooks();
            KeyboardInputHook.OnKeyboardKeyPress += KeyboardInputHook_OnKeyboardKeyPress;
            PopulateBuffTrays();
            PopulateLanguageBox();
            PopulateThemesBox();
            PopulateMonsterBox();
            PopulateDockBox();
        }

        public void UnhookEvents() {
            KeyboardInputHook.UninstallHooks();
            KeyboardInputHook.OnKeyboardKeyPress -= KeyboardInputHook_OnKeyboardKeyPress;

        }

        private void PopulateMonsterBox() {
            MonsterShowModeSelection.Items.Add(GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_MONSTER_BAR_MODE_0']"));
            MonsterShowModeSelection.Items.Add(GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_MONSTER_BAR_MODE_1']"));
            MonsterShowModeSelection.Items.Add(GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_MONSTER_BAR_MODE_2']"));
            MonsterShowModeSelection.Items.Add(GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_MONSTER_BAR_MODE_3']"));
            MonsterShowModeSelection.Items.Add(GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_MONSTER_BAR_MODE_4']"));
        }

        private void PopulateDockBox() {
            MonsterBarDock.Items.Add(GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_MONSTER_BAR_DOCK_0']"));
            MonsterBarDock.Items.Add(GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_MONSTER_BAR_DOCK_1']"));
        }

        private void PopulateLanguageBox() {
            foreach (string filename in Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages"))) {
                LanguageFilesCombobox.Items.Add(@"Languages\"+Path.GetFileName(filename));
            }
        }

        private void PopulateThemesBox() {
            foreach (string filename in Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes"))) {
                ThemeFilesCombobox.Items.Add(Path.GetFileName(filename));
            }
        }

        private void selectPathBttn_Click(object sender, System.Windows.RoutedEventArgs e) {
            using (var filePicker = new System.Windows.Forms.OpenFileDialog()) {
                Button source = (Button)sender;

                filePicker.Filter = "Executable|MonsterHunterWorld.exe";
                System.Windows.Forms.DialogResult result = filePicker.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK) {
                   
                    fullGamePath = filePicker.FileName;
                    if (filePicker.FileName.Length > 15) {
                        int i = (fullGamePath.Length / 2) - 10;
                        source.Content = "..." + fullGamePath.Substring(i);
                        source.Focusable = false;
                        return;
                    }
                    source.Content = fullGamePath;
                    
                    
                }
                source.Focusable = false;
            }

        }

        private void argsTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            fullLaunchArgs = argsTextBox.Text;
        }

        private void argsTextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e) {
            if (argsTextBox.Text == "No arguments" || argsTextBox.Text == GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_LAUNCHARGS_NOARGS']")) argsTextBox.Text = "";
        }

        private void argsTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e) {
            if (argsTextBox.Text == "") argsTextBox.Text = GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_LAUNCHARGS_NOARGS']");
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
                    Enabled = (bool)UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[i].Enabled,
                    TrayIndex = i
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
                Enabled = (bool)UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets.LastOrDefault().Enabled,
                TrayIndex = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets.Length - 1
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

        private void OnToggleOverlayKeyDown(object sender, KeyEventArgs e) {
            // Credits to this stackoverflow post I found: https://stackoverflow.com/questions/2136431/how-do-i-read-custom-keyboard-shortcut-from-user-in-wpf
            // The text box grabs all input.
            e.Handled = true;

            // Fetch the actual shortcut key.
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            // Ignore modifier keys.
            if (key == Key.LeftShift || key == Key.RightShift
                || key == Key.LeftCtrl || key == Key.RightCtrl
                || key == Key.LeftAlt || key == Key.RightAlt
                || key == Key.LWin || key == Key.RWin) {
                return;
            }

            // Build the shortcut key name.
            StringBuilder shortcutText = new StringBuilder();
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0) {
                shortcutText.Append("Ctrl+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) {
                shortcutText.Append("Shift+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0) {
                shortcutText.Append("Alt+");
            }
            shortcutText.Append(key.ToString());
            Button btn = (Button)sender;
            // Update the text box.
            btn.Content = shortcutText.ToString();
        }

        private void SwitchEnableParts_MouseDown(object sender, MouseButtonEventArgs e) {
            PartsCustomizer.IsEnabled = switchEnableParts.IsEnabled;
        }
        
    }
}
