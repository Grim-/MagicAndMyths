using RimWorld;
using System;
using System.Text;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ArtifactEffectHeal : CompProperties
    {
        public bool onlyLifeThreatening = false;
        public bool onlyBleeding = false;
        public bool onlyPermanent = false;
        public FloatRange healAmount = new FloatRange(10f, 10f);

        public CompProperties_ArtifactEffectHeal()
        {
            compClass = typeof(Comp_ArtifactEffectHeal);
        }
    }

    public class Comp_ArtifactEffectHeal : Comp_BaseAritfactEffect
    {
        private CompProperties_ArtifactEffectHeal Props => (CompProperties_ArtifactEffectHeal)props;

        public override void Apply(Pawn user, LocalTargetInfo target, Thing item)
        {
            if (target.Thing == null || !(target.Thing is Pawn targetPawn))
                return;

            Func<Hediff, bool> filter = CreateInjuryFilter();

            if (MagicUtil.TryGetWorstInjury(targetPawn, out Hediff hediff, out BodyPartRecord part, filter))
            {
                if (hediff != null)
                {
                    float reductionAmount = Props.healAmount.RandomInRange;
                    hediff.Severity -= reductionAmount;
                    MoteMaker.ThrowText(target.Thing.Position.ToVector3Shifted(), target.Thing.Map, $"{targetPawn.LabelShort} Healed {reductionAmount}", Color.green, 3);
                }
            }
        }

        private Func<Hediff, bool> CreateInjuryFilter()
        {
            return (Hediff h) =>
            {
                if (h.def.isInfection || h.def.IsAddiction || h is Hediff_MissingPart missingPart)
                {
                    return false;
                }

                return true;
            };
        }
    }
}