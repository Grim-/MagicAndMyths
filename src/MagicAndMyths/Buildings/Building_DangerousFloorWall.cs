using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class Building_DangerousFloorWall : Building_Door
    {
        public override bool BlocksPawn(Pawn p)
        {
            return false;
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
            return (ushort)(p.Drafted ? 0 : 40);
        }

        public override bool PawnCanOpen(Pawn p)
        {
            return p.Drafted && base.PawnCanOpen(p);
        }
    }
}
