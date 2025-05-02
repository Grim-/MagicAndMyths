using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_ReduceAbilityCooldown : HediffCompProperties_AbilityEffect
    {
        public IntRange baseCooldownChance = new IntRange(0, 10);
        public List<AbilityDef> excludedAbilities = new List<AbilityDef>();
        public List<AbilityDef> whitelistedAbilities = new List<AbilityDef>();
        public bool whitelistOnly = false;

        public HediffCompProperties_ReduceAbilityCooldown()
        {
            compClass = typeof(HediffComp_ReduceAbilityCooldown);
        }
    }

    public class HediffComp_ReduceAbilityCooldown : HediffComp_AbilityEffect
    {
        public HediffCompProperties_ReduceAbilityCooldown Props => (HediffCompProperties_ReduceAbilityCooldown)props;

        protected override void OnAbilityUsed(Pawn pawn, Ability usedAbility)
        {
            if (pawn?.abilities?.abilities == null)
                return;


            Ability randomAbility = Pawn.abilities.abilities.Where(x => x.HasCooldown && x != usedAbility && !Props.excludedAbilities.Contains(x.def) || (Props.whitelistOnly && !Props.whitelistedAbilities.Contains(x.def))).RandomElement();

            if (randomAbility != null)
            {
                randomAbility.ResetCooldown();
            }
        }

        public override string CompDescriptionExtra
        {
            get
            {
                string desc = base.CompDescriptionExtra + $"\r\nHas a {Props.baseCooldownChance.min} - {Props.baseCooldownChance.max} % every time an ability is used to reset the cooldown of a random ability.";
                return desc;
            }
        }
    }
}