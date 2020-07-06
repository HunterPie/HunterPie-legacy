namespace HunterPie.Core.Monsters
{
    #region Data Structures 
    public struct CrownInfo
    {
        public float Mini;
        public float Silver;
        public float Gold;
    }
    public struct WeaknessInfo
    {
        public string Id;
        public int Stars;
    }
    public struct PartInfo
    {
        public string Id;
        public bool IsRemovable;
        public string GroupId;
        public int[] BreakThresholds;
        public bool Skip;
        public uint Index;
    }
    #endregion

    public class MonsterInfo
    {
        // Basic info
        public string Em { get; set; }
        public int Id { get; set; }
        public CrownInfo Crowns { get; set; }
        public float Capture { get; set; }
        public WeaknessInfo[] Weaknesses { get; set; }

        // Parts info
        public int MaxParts { get; set; }
        public PartInfo[] Parts { get; set; }
        public int MaxRemovableParts { get; set; }

        #region Methods
        public string GetCrownByMultiplier(float multiplier)
        {
            // Work around for this dumb crown multiplier
            multiplier = float.Parse($"{multiplier:0.00000000}");

            if (multiplier >= Crowns.Gold) return "CROWN_GOLD";
            if (multiplier >= Crowns.Silver) return "CROWN_SILVER";
            if (multiplier <= Crowns.Mini) return "CROWN_MINI";
            return null;
        }
        #endregion
    }
}
