using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HunterPie.Core;
using HunterPie.Core.Enums;
using HunterPie.UI.Annotations;
using Newtonsoft.Json;
using System.Reflection;
using System.Security.Principal;
using HunterPie.Settings;

namespace HunterPie.GUIControls
{
    /// <summary>
    /// Interaction logic for NewSettingsWindow.xaml
    /// </summary>
    public partial class NewSettingsWindow : UserControl, INotifyPropertyChanged
    {
        public ICommand OpenLink { get; set; } = new OpenLink();

        public string DebugInformation
        {
            get
            {
                var winIdentity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(winIdentity);
                bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

                return JsonConvert.SerializeObject(
                    new
                    {
                        Versions = new
                        {
                            Main = Assembly.GetEntryAssembly().GetName().Version.ToString(),
                            Core = typeof(Player).Assembly.GetName().Version.ToString(),
                            UI = typeof(GUI.Overlay).Assembly.GetName().Version.ToString()
                        },
                        Windows = new {Admin = isAdmin},
                        Native = new
                        {
                            Enabled = ConfigManager.Settings.HunterPie.EnableNativeFunctions
                        }
                    }, Formatting.Indented);
            }
        }

        public NewSettingsWindow()
        {
            InitializeComponent();
            PopulateBuffTrays();
            PopulateLanguageBox();
            PopulateThemesBox();
            PopulateMonsterBox();
            PopulatePlotDisplayModeBox();
            PopulateProxyModeBox();
            PopulateDockBox();
            //debugInfoTb.Text = DebugInformation;
        }

        public void UnhookEvents()
        {
            switchEnableAilments.MouseLeftButtonDown -= SwitchEnableAilments_MouseDown;
            switchEnableParts.MouseDown -= SwitchEnableParts_MouseDown;
            MonsterShowModeSelection.Items.Clear();
            LanguageFilesCombobox.Items.Clear();
            ThemeFilesCombobox.Items.Clear();
            MonsterBarDock.Items.Clear();
        }

        private void PopulateMonsterBox()
        {
            for (int i = 0; i < 5; i++)
            {
                MonsterShowModeSelection.Items.Add(
                    GStrings.GetLocalizationByXPath($"/Settings/String[@ID='STATIC_MONSTER_BAR_MODE_{i}']"));
            }
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
                if (filename.EndsWith(".xaml"))
                    ThemeFilesCombobox.Items.Add(Path.GetFileName(filename));
            }
        }

        private void PopulatePlotDisplayModeBox()
        {
            foreach (DamagePlotMode item in Enum.GetValues(typeof(DamagePlotMode)))
            {
                comboDamagePlotMode.Items.Add(
                    GStrings.GetLocalizationByXPath($"/Settings/String[@ID='DAMAGE_PLOT_MODE_{(byte)item}']"));
            }
        }
        private void PopulateProxyModeBox()
        {
            foreach (PluginProxyMode item in Enum.GetValues(typeof(PluginProxyMode)))
            {
                comboPluginUpdateProxy.Items.Add(
                    GStrings.GetLocalizationByXPath($"/Settings/String[@ID='PLUGIN_PROXY_MODE_{item.ToString("G").ToUpperInvariant()}']"));
            }
        }

        private void PopulateBuffTrays()
        {
            for (int i = 0; i < ConfigManager.Settings.Overlay.AbnormalitiesWidget.ActiveBars; i++)
            {
                Custom_Controls.BuffBarSettingControl BuffBar = new Custom_Controls.BuffBarSettingControl()
                {
                    PresetName = ConfigManager.Settings.Overlay.AbnormalitiesWidget.BarPresets[i].Name,
                    Enabled = ConfigManager.Settings.Overlay.AbnormalitiesWidget.BarPresets[i].Enabled,
                    TrayIndex = i
                };
                BuffTrays.Children.Add(BuffBar);
            }
            if (ConfigManager.Settings.Overlay.AbnormalitiesWidget.ActiveBars >= 5)
            {
                AddNewBuffBar.Opacity = 0.5;
            }
            else if (ConfigManager.Settings.Overlay.AbnormalitiesWidget.ActiveBars <= 1)
            {
                SubBuffBar.Opacity = 0.5;
            }
            NumberOfBuffBars.Text = ConfigManager.Settings.Overlay.AbnormalitiesWidget.ActiveBars.ToString();
        }

        private void AddNewBuffBarClick(object sender, MouseButtonEventArgs e)
        {
            if (ConfigManager.Settings.Overlay.AbnormalitiesWidget.ActiveBars > 4)
            {
                AddNewBuffBar.Opacity = 0.5;
                return;
            }
            SubBuffBar.Opacity = 1;
            ConfigManager.AddNewAbnormalityBar(1);
            ConfigManager.Settings.Overlay.AbnormalitiesWidget.ActiveBars += 1;
            Custom_Controls.BuffBarSettingControl BuffBar = new Custom_Controls.BuffBarSettingControl()
            {
                PresetName = ConfigManager.Settings.Overlay.AbnormalitiesWidget.BarPresets.LastOrDefault().Name,
                Enabled = ConfigManager.Settings.Overlay.AbnormalitiesWidget.BarPresets.LastOrDefault().Enabled,
                TrayIndex = ConfigManager.Settings.Overlay.AbnormalitiesWidget.BarPresets.Length - 1
            };
            BuffTrays.Children.Add(BuffBar);
            NumberOfBuffBars.Text = ConfigManager.Settings.Overlay.AbnormalitiesWidget.ActiveBars.ToString();
            if (ConfigManager.Settings.Overlay.AbnormalitiesWidget.ActiveBars >= 5)
            {
                AddNewBuffBar.Opacity = 0.5;
                SubBuffBar.Opacity = 1;
                return;
            }
        }

        private void SubBuffBarClick(object sender, MouseButtonEventArgs e)
        {
            if (ConfigManager.Settings.Overlay.AbnormalitiesWidget.ActiveBars <= 1)
            {
                SubBuffBar.Opacity = 0.5;
                return;
            }
            AddNewBuffBar.Opacity = 1;
            ConfigManager.Settings.Overlay.AbnormalitiesWidget.ActiveBars -= 1;
            if (BuffTrays.Children.Count - 1 > 0)
            {
                BuffTrays.Children.RemoveAt(BuffTrays.Children.Count - 1);
            }
            ConfigManager.RemoveAbnormalityBars();
            NumberOfBuffBars.Text = ConfigManager.Settings.Overlay.AbnormalitiesWidget.ActiveBars.ToString();
            if (ConfigManager.Settings.Overlay.AbnormalitiesWidget.ActiveBars <= 1)
            {
                SubBuffBar.Opacity = 0.5;
                AddNewBuffBar.Opacity = 1;
                return;
            }
        }

        private void SwitchEnableParts_MouseDown(object sender, MouseButtonEventArgs e) => PartsCustomizer.IsEnabled = switchEnableParts.IsEnabled;

        private void SwitchEnableAilments_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AilmentsCustomizer.IsEnabled = switchEnableAilments.IsEnabled;
            foreach (SettingsItem settingsItem in SettingItems)
            {
                settingsItem.UpdateDefaultStyle();
            }
        }

        public ObservableCollection<SettingsItem> SettingItems { get; set; } = new();

        public void AddSettingsBlock(ISettingsTab tab)
        {
            var item = new SettingsItem(tab)
            {
                // HACK: styles may not be applied correctly for dynamically added control, so we're assigning style explicitly
                Style = Resources.Values.OfType<Style>().FirstOrDefault(s => s.TargetType == typeof(TabItem))
            };
            SettingItems.Add(item);
            TabContainer.Items.Add(item);
            OnPropertyChanged(nameof(HasPlugins));
        }

        public bool HasPlugins => SettingItems.Any();

        public void RemoveSettingsBlock(ISettingsTab tab)
        {
            var itemToRemove = SettingItems.FirstOrDefault(item => item.Tab == tab);
            if (itemToRemove != null)
            {
                SettingItems.Remove(itemToRemove);
                TabContainer.Items.Remove(itemToRemove);
                OnPropertyChanged(nameof(HasPlugins));
            }
        }

        public bool Save()
        {
            var hasErrors = false;
            foreach (var settingsItem in SettingItems)
            {
                hasErrors |= settingsItem.Save();
            }

            return hasErrors;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class OpenLink : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (parameter?.GetType() == typeof(string))
            {
                if (((string)parameter).StartsWith("http"))
                    return true;
            }

            return false;
        }

        public void Execute(object parameter)
        {
            Process.Start((string)parameter);
        }
    }
}
