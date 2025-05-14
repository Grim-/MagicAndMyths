using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_DealDamageOnAbilityUse : HediffCompProperties_AbilityEffect
    {
        public DamageDef damageDef;
        public FloatRange damageRange;
        public FloatRange armourpenRange;

        public bool isAOE = false;
        public float radius = 5;
        public bool canTargetHostile = true;
        public bool canTargetFriendly = false;
        public bool canTargetNeutral = false;

        public HediffCompProperties_DealDamageOnAbilityUse()
        {
            compClass = typeof(HediffComp_DealDamageOnAbilityUse);
        }
    }


    public class HediffComp_DealDamageOnAbilityUse : HediffComp_AbilityEffect
    {
        public HediffCompProperties_DealDamageOnAbilityUse Props => (HediffCompProperties_DealDamageOnAbilityUse)props;

        protected override void OnAbilityUsed(Pawn pawn, RimWorld.Ability ability)
        {
            if (Props.isAOE)
            {
                TargetUtil.ApplyDamageInRadius(Props.damageDef,
                    Props.damageRange.RandomInRange,
                    Props.armourpenRange.RandomInRange,
                    Pawn.Position,
                    Pawn.Map,
                    Props.radius,
                    Pawn.Faction,
                    true,
                    Pawn,
                    Props.canTargetHostile,
                    Props.canTargetFriendly,
                    Props.canTargetNeutral);
            }
            else
            {
                Pawn.TakeDamage(new DamageInfo(Props.damageDef, Props.damageRange.RandomInRange, Props.armourpenRange.RandomInRange));
            }
        }

        public override string CompDescriptionExtra => base.CompDescriptionExtra + $"\r\nDeals {Props.damageRange.min} - {Props.damageRange.max} {Props.damageDef.LabelCap} damage every time an ability is used.";
    }
}