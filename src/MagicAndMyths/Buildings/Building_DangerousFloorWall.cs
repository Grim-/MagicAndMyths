using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class Building_DangerousFloorWall : Building
    {
        public override bool BlocksPawn(Pawn p)
        {
            return !p.Drafted;
        }

        public override bool IsDangerousFor(Pawn pawn)
        {
            if (pawn != null && pawn.Drafted)
            {
                return false;
            }
            return true;
        }

        public override ushort PathWalkCostFor(Pawn p)
        {
            return (ushort)(p.Drafted ? 0 : 4000);
        }
    }
}
