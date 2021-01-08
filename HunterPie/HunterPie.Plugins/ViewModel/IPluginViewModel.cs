using System.Windows.Input;
using System.Windows.Media;

namespace HunterPie.Plugins
{
    public interface IPluginViewModel
    {
        bool CanToggle { get; }
        bool CanDelete { get; }
        bool CanInstall { get; }
        bool CanRestore { get; }

        bool IsVersionOk { get; }
        bool HasImage { get; }
        bool IsFailed { get; }

        ICommand DownloadCommand { get; }
        ICommand DeleteCommand { get; }
        ICommand ToggleCommand { get; }
        ICommand RestoreCommand { get; }

        string ReadmeUrl { get; }

        string Name { get; }
        string Description { get; }
        string SubText { get; }

        bool IsEnabled { get; set; }
        bool IsBusy { get; }
        string InternalName { get; }

        ImageSource Image { get; }
    }
}
