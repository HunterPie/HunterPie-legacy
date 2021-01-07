using System;
using System.Windows.Input;

namespace HunterPie.UI.Infrastructure
{

    public class DisabledCommand : ICommand
    {
        public static DisabledCommand Instance = new DisabledCommand();

        public bool CanExecute(object parameter) => false;

        public void Execute(object parameter) {}

        public event EventHandler CanExecuteChanged;
    }
}
