using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using HunterPie.UI.Annotations;

namespace HunterPie.UI.Infrastructure
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected void Dispatch(Action act) => Application.Current?.Dispatcher.Invoke(act);
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
