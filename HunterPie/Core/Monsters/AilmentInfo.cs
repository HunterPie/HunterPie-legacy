using HunterPie.Core.Enums;

namespace HunterPie.Core.Monsters
{
    public class AilmentInfo
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public bool CanSkip { get; set; }
        public string Group { get; set; }
        public AilmentType Type { get; set; }
    }
}
