using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_UseEffectTargetablePosition : CompProperties_Targetable
    {
        public CompProperties_UseEffectTargetablePosition()
        {
            compClass = typeof(CompTargetable_Position);
        }
    }


    public class CompProperties_TargetEffectAddHediff : CompProperties
    {
        public bool targetSelf = false;
        public HediffDef hediff;
        public FloatRange severity = new FloatRange(1, 1);

        public CompProperties_TargetEffectAddHediff()
        {
            compClass = typeof(CompTargetEffect_AddHediff);
        }
    }


    public class CompTargetEffect_AddHediff : CompTargetEffect
    {
        CompProperties_TargetEffectAddHediff Props => (CompProperties_TargetEffectAddHediff)props;

        public override void DoEffectOn(Pawn user, Thing target)
        {
            if (Props.hediff == null || target == null)
            {
                return;
            }

            if (Props.targetSelf)
            {
                Hediff hediff = user.health.GetOrAddHediff(Props.hediff);
                hediff.Severity += Props.severity.RandomInRange;
            }
            else
            {
                if (target is Pawn targetPawn)
                {
                    Hediff hediff = targetPawn.health.GetOrAddHediff(Props.hediff);
                    hediff.Severity += Props.severity.RandomInRange;
                }
            }     
        }
    }


    public class CompTargetable_Position : CompTargetable
    {
        protected override bool PlayerChoosesTarget
        {
            get
            {
                return true;
            }
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            return true;
        }

        protected override TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters
            {
                canTargetLocations = true,
            };
        }

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
        }

        public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
        {
            yield return targetChosenByPlayer;
            yield break;
        }
    }
}