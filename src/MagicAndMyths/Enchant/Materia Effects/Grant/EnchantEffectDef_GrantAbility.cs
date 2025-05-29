using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_GrantAbility : EnchantEffectDef
    {
        public AbilityDef ability;

        public override string EffectDescription => $"Grants the {ability.LabelCap} ability while equipped";
    }

    public class EnchantEffect_GrantAbility : EnchantWorker
    {
        EnchantEffectDef_GrantAbility Def => (EnchantEffectDef_GrantAbility)def;

        private Ability abilityRef;
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);

            if (EquippingPawn.abilities.GetAbility(Def.ability) == null)
            {
                EquippingPawn.abilities.GainAbility(Def.ability);
                abilityRef = EquippingPawn.abilities.GetAbility(Def.ability);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            if (abilityRef != null && EquippingPawn.abilities.GetAbility(Def.ability) != null)
            {
                EquippingPawn.abilities.RemoveAbility(Def.ability);
                abilityRef = null;
            }
            base.Notify_Unequipped(pawn);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref abilityRef, "abilityRef");
        }
    }


}