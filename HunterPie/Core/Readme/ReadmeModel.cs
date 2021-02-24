using System.Collections.Generic;

namespace HunterPie.Core.Readme
{
    public class ReadmeModel
    {
        public string Readme { get; set; }

        public Dictionary<string, byte[]> Images { get; set; } = new();
    }
}
