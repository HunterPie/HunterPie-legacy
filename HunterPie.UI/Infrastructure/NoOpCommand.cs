using System;
using System.Windows.Input;

namespace HunterPie.UI.Infrastructure
{
    public class NoOpCommand : ICommand
    {
        public static NoOpCommand Instance = new NoOpCommand();

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) { }

        public event EventHandler CanExecuteChanged;
    }
}
