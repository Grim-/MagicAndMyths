using RimWorld;
using SquadBehaviour;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_StarCall : CompProperties_AbilityEffect
    {
        public int amount = 5;
        public FormationUtils.FormationType formationType = FormationUtils.FormationType.Circle;
        public IntRange ticksToImpact = new IntRange(200, 300);
        public Vector2 meteorMaxSize = new Vector2(2, 2);
        public Vector2 meteorMinSize = new Vector2(0.2f, 0.2f);
        public int impactRadius = 2;
        public DamageDef overrideDamageDef = null;
        public int overrideDamageAmount = -1;

        public CompProperties_StarCall()
        {
            compClass = typeof(CompAbilityEffect_StarCall);
        }
    }

    public class CompAbilityEffect_StarCall : CompAbilityEffect
    {
        CompProperties_StarCall Props => (CompProperties_StarCall)props;

        WeaponAbility WeaponAbility => this.parent as WeaponAbility;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = this.parent.pawn.Map;

            if (target.Cell.IsValid && target.Cell.InBounds(Find.CurrentMap))
            {
                for (int i = 0; i < Props.amount; i++)
                {
                    IntVec3 targetPosition = FormationUtils.GetFormationPosition(Props.formationType, target.CenterVector3, Rot4.South, i, Props.amount);
                    if (targetPosition.InBounds(this.parent.pawn.Map))
                    {
                        Meteor meteor = Meteor.Launch(targetPosition, map, Props.meteorMaxSize, Props.meteorMinSize, Props.impactRadius, Props.ticksToImpact.RandomInRange, Props.overrideDamageDef, Props.overrideDamageAmount);
                    }           
                }
            }
        }


        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);
        }


    }
}
