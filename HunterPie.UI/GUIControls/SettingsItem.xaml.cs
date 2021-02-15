using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using HunterPie.Logger;
using HunterPie.Settings;
using HunterPie.UI.Annotations;

namespace HunterPie.GUIControls
{
    public partial class SettingsItem : TabItem, INotifyPropertyChanged
    {
        private ISettingsTab tab;
        private string error;

        public SettingsItem()
        {
            InitializeComponent();
        }

        public SettingsItem(ISettingsTab tab)
        {
            Tab = tab;
            tab.Settings.LoadSettings();
            InitializeComponent();
        }


        public ISettingsTab Tab
        {
            get => tab;
            set
            {
                if (Equals(value, tab)) return;
                tab = value;
                OnPropertyChanged();
            }
        }

        public string Error
        {
            get => error;
            set
            {
                if (value == error) return;
                error = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(Error);

        public bool Save()
        {
            try
            {
                Error = this.tab.Settings.ValidateSettings();
                if (string.IsNullOrEmpty(Error))
                {
                    this.tab.Settings.SaveSettings();
                }
            }
            catch (Exception ex)
            {
                Debugger.Warn(ex.ToString());
                Error = ex.GetBaseException().Message;
            }

            return HasError;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

