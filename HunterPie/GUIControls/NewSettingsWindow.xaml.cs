using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using HunterPie.Core;

namespace HunterPie.GUIControls
{
    /// <summary>
    /// Interaction logic for NewSettingsWindow.xaml
    /// </summary>
    public partial class NewSettingsWindow : UserControl
    {
        public string fullGamePath = "";
        public string fullMonsterDataPath = "";
        public string fullLaunchArgs = "";

        public NewSettingsWindow()
        {
            InitializeComponent();
            PopulateBuffTrays();
            PopulateLanguageBox();
            PopulateThemesBox();
            PopulateMonsterBox();
            PopulateDockBox();
        }

        public void UnhookEvents()
        {
            switchEnableParts.MouseDown -= SwitchEnableParts_MouseDown;
            MonsterShowModeSelection.Items.Clear();
            LanguageFilesCombobox.Items.Clear();
            ThemeFilesCombobox.Items.Clear();
            MonsterBarDock.Items.Clear();
        }

        private void PopulateMonsterBox()
        {
            MonsterShowModeSelection.Items.Add(GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_MONSTER_BAR_MODE_0']"));
            MonsterShowModeSelection.Items.Add(GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_MONSTER_BAR_MODE_1']"));
            MonsterShowModeSelection.Items.Add(GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_MONSTER_BAR_MODE_2']"));
            MonsterShowModeSelection.Items.Add(GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_MONSTER_BAR_MODE_3']"));
            MonsterShowModeSelection.Items.Add(GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_MONSTER_BAR_MODE_4']"));
        }

        private void PopulateDockBox()
        {
            MonsterBarDock.Items.Add(GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_MONSTER_BAR_DOCK_0']"));
            MonsterBarDock.Items.Add(GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_MONSTER_BAR_DOCK_1']"));
        }

        private void PopulateLanguageBox()
        {
            foreach (string filename in Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages")))
            {
                LanguageFilesCombobox.Items.Add(@"Languages\" + Path.GetFileName(filename));
            }
        }

        private void PopulateThemesBox()
        {
            foreach (string filename in Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes")))
            {
                ThemeFilesCombobox.Items.Add(Path.GetFileName(filename));
            }
        }

        private void selectPathBttn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            using (var filePicker = new System.Windows.Forms.OpenFileDialog())
            {
                Button source = (Button)sender;

                filePicker.Filter = "Executable|MonsterHunterWorld.exe";
                System.Windows.Forms.DialogResult result = filePicker.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {

                    fullGamePath = filePicker.FileName;
                    if (filePicker.FileName.Length > 15)
                    {
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

        private void argsTextBox_TextChanged(object sender, TextChangedEventArgs e) => fullLaunchArgs = argsTextBox.Text;

        private void argsTextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (argsTextBox.Text == "No arguments" || argsTextBox.Text == GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_LAUNCHARGS_NOARGS']")) argsTextBox.Text = "";
        }

        private void argsTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (argsTextBox.Text == "") argsTextBox.Text = GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_LAUNCHARGS_NOARGS']");
        }

        private void SelectPathBttn_LostFocus(object sender, System.Windows.RoutedEventArgs e) => selectPathBttn.Focusable = true;

        private void PopulateBuffTrays()
        {
            for (int i = 0; i < UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars; i++)
            {
                Custom_Controls.BuffBarSettingControl BuffBar = new Custom_Controls.BuffBarSettingControl()
                {
                    PresetName = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[i].Name,
                    Enabled = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[i].Enabled,
                    TrayIndex = i
                };
                BuffTrays.Children.Add(BuffBar);
            }
            if (UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars >= 5)
            {
                AddNewBuffBar.Opacity = 0.5;
            }
            else if (UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars <= 1)
            {
                SubBuffBar.Opacity = 0.5;
            }
            NumberOfBuffBars.Text = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars.ToString();
        }

        private void AddNewBuffBarClick(object sender, MouseButtonEventArgs e)
        {
            if (UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars > 4)
            {
                AddNewBuffBar.Opacity = 0.5;
                return;
            }
            SubBuffBar.Opacity = 1;
            UserSettings.AddNewAbnormalityBar(1);
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars += 1;
            Custom_Controls.BuffBarSettingControl BuffBar = new Custom_Controls.BuffBarSettingControl()
            {
                PresetName = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets.LastOrDefault().Name,
                Enabled = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets.LastOrDefault().Enabled,
                TrayIndex = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets.Length - 1
            };
            BuffTrays.Children.Add(BuffBar);
            NumberOfBuffBars.Text = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars.ToString();
            if (UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars >= 5)
            {
                AddNewBuffBar.Opacity = 0.5;
                SubBuffBar.Opacity = 1;
                return;
            }
        }

        private void SubBuffBarClick(object sender, MouseButtonEventArgs e)
        {
            if (UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars <= 1)
            {
                SubBuffBar.Opacity = 0.5;
                return;
            }
            AddNewBuffBar.Opacity = 1;
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars -= 1;
            if (BuffTrays.Children.Count - 1 > 0)
            {
                BuffTrays.Children.RemoveAt(BuffTrays.Children.Count - 1);
            }
            UserSettings.RemoveAbnormalityBars();
            NumberOfBuffBars.Text = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars.ToString();
            if (UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.ActiveBars <= 1)
            {
                SubBuffBar.Opacity = 0.5;
                AddNewBuffBar.Opacity = 1;
                return;
            }
        }

        private void SwitchEnableParts_MouseDown(object sender, MouseButtonEventArgs e) => PartsCustomizer.IsEnabled = switchEnableParts.IsEnabled;

    }
}
