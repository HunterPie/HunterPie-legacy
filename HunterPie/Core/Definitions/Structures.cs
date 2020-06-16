using JsonConvert = Newtonsoft.Json.JsonConvert;
using Formatting = Newtonsoft.Json.Formatting;

namespace HunterPie.Core.Definitions
{
    public class Helpers
    {
        public static string Serialize(object obj) {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
    }
}
