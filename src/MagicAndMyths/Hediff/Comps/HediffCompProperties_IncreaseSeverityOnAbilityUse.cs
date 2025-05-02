using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{



    public class HediffCompProperties_IncreaseSeverityOnAbilityUse : HediffCompProperties_AbilityEffect
    {
        public FloatRange severityIncreaseOnCast = new FloatRange(0.01f, 0.05f);

        public HediffCompProperties_IncreaseSeverityOnAbilityUse()
        {
            compClass = typeof(HediffComp_IncreaseSeverityOnAbilityUse);
        }
    }

    public class HediffComp_IncreaseSeverityOnAbilityUse : HediffComp_AbilityEffect
    {
        public HediffCompProperties_IncreaseSeverityOnAbilityUse Props => (HediffCompProperties_IncreaseSeverityOnAbilityUse)props;

        protected override void OnAbilityUsed(Pawn pawn, RimWorld.Ability ability)
        {
            float chosen = Props.severityIncreaseOnCast.RandomInRange;
            this.parent.Severity += chosen;
            if (pawn != null && pawn.Map != null && pawn.Spawned)
            {
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, $"{this.parent.Label} increasing severity by {chosen * 100}%");
            }   
        }

        public override string CompDescriptionExtra => base.CompDescriptionExtra + $"\r\nSeverity increases by {Props.severityIncreaseOnCast.min} - {Props.severityIncreaseOnCast.max} whenever an ability is used.";
    }


}