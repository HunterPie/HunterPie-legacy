using HunterPie.Core;
using HunterPie.Logger;

namespace HunterPie.Plugins
{
    public interface IPlugin
    {
        string Name { get; set; }
        string Description { get; set; }
        Game Context { get; set; }

        void Initialize(Game context);
        void Unload();

    }

    public static class PluginExtension
    {
        public static void Log(this IPlugin plugin, object message)
        {
            Debugger.Module(message, plugin.Name);
        }
    }
}
