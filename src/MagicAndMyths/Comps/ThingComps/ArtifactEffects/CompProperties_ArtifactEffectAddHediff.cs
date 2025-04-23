using RimWorld;
using System;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ArtifactEffectAddHediff : CompProperties
    {
        public HediffDef hediff;
        public FloatRange severity = new FloatRange(1f, 1f);

        public CompProperties_ArtifactEffectAddHediff()
        {
            compClass = typeof(Comp_ArtifactEffectAddHediff);
        }
    }

    public class Comp_ArtifactEffectAddHediff : Comp_BaseAritfactEffect
    {
        private CompProperties_ArtifactEffectAddHediff Props => (CompProperties_ArtifactEffectAddHediff)props;

        public override void Apply(Pawn user, LocalTargetInfo target, Thing item)
        {
            if (Props.hediff == null)
                return;

            if (target.Thing != null)
            {
                if (target.Thing is Pawn targetPawn)
                {
                    Hediff hediff = targetPawn.health.GetOrAddHediff(Props.hediff);
                    hediff.Severity += Props.severity.RandomInRange;
                }
            }
        }
    }
}