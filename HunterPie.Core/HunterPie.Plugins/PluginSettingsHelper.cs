using System.Linq;
using Newtonsoft.Json.Linq;

namespace HunterPie.Plugins
{
    public class PluginSettingsHelper
    {
        public static PluginSettings GetPluginSettings(JObject json)
        {
            if (json.Properties().Count() == 0)
            {
                return new PluginSettings();
            }
            else
            {
                return json.ToObject<PluginSettings>();
            }
        }

        public static void mergePluginSettings(JObject json, PluginSettings overrides)
        {
            json.Merge(JObject.FromObject(overrides));
        }

    }
}
