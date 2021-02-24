using System.Windows.Input;
using System.Windows.Media;
using HunterPie.UI.Infrastructure;

namespace HunterPie.Plugins
{
    public class PluginActionViewModel : BaseViewModel
    {
        public string Name { get; set; }
        public ImageSource Icon { get; set; }
        public ICommand Command { get; set; }

        public PluginActionViewModel(string name, ImageSource icon, ICommand command)
        {
            Name = name;
            Icon = icon;
            Command = command;
        }

        public PluginActionViewModel()
        {
        }
    }
}
