using System.Collections.Generic;

namespace HunterPie.Core.Readme
{
    public class ReadmeIndex
    {
        public ReadmeIndex()
        {
        }

        public string Source { get; set; }
        public Dictionary<string, string> Images { get; set; } = new();
    }
}
