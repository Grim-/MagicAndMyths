using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_MagicTattooGrantAbility : HediffCompProperties_MagicTattooBase
    {
        public AbilityDef ability;

        public HediffCompProperties_MagicTattooGrantAbility()
        {
            compClass = typeof(HediffComp_MagicTattooGrantAbility);
        }
    }

    public class HediffComp_MagicTattooGrantAbility : HediffComp_MagicTattooBase
    {
        new public HediffCompProperties_MagicTattooGrantAbility Props => (HediffCompProperties_MagicTattooGrantAbility)props;


        private Ability abilityRef = null;

        public override void OnTattooApplied()
        {
            base.OnTattooApplied();

            if (this.Pawn.abilities != null)
            {
                if (this.Pawn.abilities.GetAbility(Props.ability) == null)
                {
                    this.Pawn.abilities.GainAbility(Props.ability);

                    abilityRef = this.Pawn.abilities.GetAbility(Props.ability);
                }
            }
        }

        public override void OnTattooRemoved()
        {
            base.OnTattooRemoved();

            if (abilityRef != null)
            {
                if (this.Pawn.abilities != null)
                {
                    if (this.Pawn.abilities.GetAbility(abilityRef.def) != null)
                    {
                        this.Pawn.abilities.RemoveAbility(Props.ability);
                        abilityRef = null;
                    }

                }
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref abilityRef, "abilityGranted");
        }
    }


}
