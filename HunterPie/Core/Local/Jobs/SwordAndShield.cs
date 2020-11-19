using HunterPie.Core.Enums;

namespace HunterPie.Core.Jobs
{
    public class SwordAndShield : Job
    {
        public override int SafijiivaMaxHits => 10;
        public override Classes Type => Classes.SwordAndShield;
        public override bool IsMelee => true;
    }
}
