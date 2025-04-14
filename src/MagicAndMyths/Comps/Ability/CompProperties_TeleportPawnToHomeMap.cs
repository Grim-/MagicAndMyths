using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_TeleportPawnToHomeMap : CompProperties_AbilityEffect
    {
        public CompProperties_TeleportPawnToHomeMap()
        {
            compClass = typeof(CompAbilityEffect_TeleportPawnToHomeMap);
        }
    }

    public class CompAbilityEffect_TeleportPawnToHomeMap : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (parent.pawn?.Map == null)
                return;
            Map homeMap = WorldComp_DungeonManager.StartingColonyMap;

            if (homeMap == null)
            {
                return;
            }

            parent.pawn.TransferToMap(CellFinder.StandableCellNear(homeMap.Center, homeMap, 3), homeMap);
        }
    }
}
