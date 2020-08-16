using HunterPie.Core;

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
}
