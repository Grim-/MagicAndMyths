using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThrowablePetrify : CompProperties_Throwable
    {
        public CompProperties_ThrowablePetrify()
        {
            compClass = typeof(Comp_ThrowablePetrify);
        }
    }

    public class Comp_ThrowablePetrify : Comp_Throwable
    {
        public CompProperties_ThrowablePetrify Props => (CompProperties_ThrowablePetrify)props;

        public override void OnRespawn(IntVec3 position, Thing thing, Map map, Pawn throwingPawn)
        {
            foreach (Pawn p in TargetUtil.GetPawnsInRadius(position, map, Props.radius, throwingPawn.Faction, true, throwingPawn, true, true, true))
            {
                PetrifiedStatue.PetrifyPawn(MagicAndMythDefOf.MagicAndMyths_PetrifiedStatue, p, p.Position, map);
            }
        }
    }
}