using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ToggleHediff : CompProperties_AbilityEffect
    {
        public HediffDef hediffDef;

        public CompProperties_ToggleHediff()
        {
            compClass = typeof(CompAbilityEffect_ToggleHediff);
        }
    }

    public class CompAbilityEffect_ToggleHediff : CompAbilityEffect
    {
        new CompProperties_ToggleHediff Props => (CompProperties_ToggleHediff)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (parent.pawn == null || Props.hediffDef == null)
                return;

            if (parent.pawn.health.hediffSet.HasHediff(Props.hediffDef))
            {
                parent.pawn.health.RemoveHediff(parent.pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef));
            }
            else
            {
                parent.pawn.health.AddHediff(Props.hediffDef, null);
            }

        }
    }
}
