using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_TriggerEffectApplyHediff : CompProperties_TriggerBase
    {
        public HediffDef hediff;
        public FloatRange applicationChance = new FloatRange(100, 100);
        public FloatRange severityAmount = new FloatRange(1, 1);
        public CompProperties_TriggerEffectApplyHediff()
        {
            compClass = typeof(CompTrap_TriggerEffectApplyHediff);
        }
    }


    public class CompTrap_TriggerEffectApplyHediff : Comp_TriggerBase
    {
        private CompProperties_TriggerEffectApplyHediff Props => (CompProperties_TriggerEffectApplyHediff)props;

        public override void Trigger(Pawn pawn)
        {
            base.Trigger(pawn);
            MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, $"{pawn.LabelShort} has recieved {Props.hediff.LabelCap} from triggering {this.parent.LabelShort}!", Color.green, 4);
        }

        protected override void ApplyTo(Pawn pawn)
        {
            if (Props.hediff == null)
            {
                return;
            }

           Hediff hediff = pawn.health.GetOrAddHediff(Props.hediff);
           hediff.Severity += Props.severityAmount.RandomInRange;

        }
    }
}