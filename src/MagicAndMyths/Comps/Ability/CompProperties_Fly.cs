using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_Fly : CompProperties_AbilityEffect
    {
        public CompProperties_Fly()
        {
            compClass = typeof(CompAbilityEffect_Fly);
        }
    }

    public class CompAbilityEffect_Fly : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (parent.pawn?.Map == null)
                return;


            Map map = parent.pawn.Map;
            PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(MagicAndMythDefOf.Thor_PawnFlyer, parent.pawn, target.Cell, null, null);
            GenSpawn.Spawn(pawnFlyer, parent.pawn.Position, map);
        }
    }
}
