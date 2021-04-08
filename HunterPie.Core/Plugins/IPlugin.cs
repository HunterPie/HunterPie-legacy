using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
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

        public static void Error(this IPlugin plugin, object message)
        {
            Debugger.Write($"[{plugin.Name.ToUpperInvariant()}] [ERROR] {message}", Debugger.ERROR);
        }

        public static string GetPath(this IPlugin plugin)
        {
            return Path.GetDirectoryName(plugin.GetType().Assembly.Location);
        }

        public static async Task<T> LoadJson<T>(this IPlugin plugin, string json) where T : new()
        {
            var jsonPath = Path.Combine(plugin.GetPath(), json);
            try {
                using (var reader = new StreamReader(jsonPath))
                {
                    return JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync()) ?? new T();
                }
            } catch (FileNotFoundException) {
                return new T();
            }
        }

        public static async Task SaveJson<T>(this IPlugin plugin, string json, T config)
        {
            var jsonPath = Path.Combine(plugin.GetPath(), json);
            var jsonStr = JsonConvert.SerializeObject(config, Formatting.Indented);
            using (var writer = new StreamWriter(jsonPath))
            {
                await writer.WriteAsync(jsonStr);
            }
            return;
        }
    }
}
