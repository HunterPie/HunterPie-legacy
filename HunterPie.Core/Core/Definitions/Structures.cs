using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace HunterPie.Core.Definitions
{
    public class Helpers
    {
        public static string Serialize(object obj) => JsonConvert.SerializeObject(obj);
    }
}
