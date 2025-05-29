using RimWorld;
using System.Text;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ArtifactEffectHeal : CompProperties
    {
        public HealParameters healingParams;
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

            float healAmount = Props.healAmount.RandomInRange;
            float actualHealed = targetPawn.SpendHealingAmount(healAmount, Props.healingParams);

            if (actualHealed > 0)
            {
                MoteMaker.ThrowText(target.Thing.Position.ToVector3Shifted(), target.Thing.Map, $"{targetPawn.LabelShort} Healed {actualHealed:F1}", Color.green, 3);
            }
        }
    }
}