using HunterPie.Core.Enums;

namespace HunterPie.Core.Jobs
{
    public class Lance : Job
    {
        public override int SafijiivaMaxHits => 8;
        public override Classes Type => Classes.Lance;
        public override bool IsMelee => true;
    }
}
