using RimWorld;
using System;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_EquippableAbility : CompProperties
    {
        public CompProperties_EquippableAbility()
        {
            this.compClass = typeof(CompEquippableAbility);
        }

        public AbilityDef abilityDef;
    }


	public class CompEquippableAbility : ThingComp
	{
		private CompProperties_EquippableAbility Props
		{
			get
			{
				return this.props as CompProperties_EquippableAbility;
			}
		}
		private Ability ability;
		public Ability AbilityForReading
		{
			get
			{
				if (this.ability == null)
				{
					this.ability = AbilityUtility.MakeAbility(this.Props.abilityDef, EquippedPawn);
				}
				return this.ability;
			}
		}

		protected Pawn EquippedPawn = null;
		public override void Notify_Equipped(Pawn pawn)
		{
			EquippedPawn = pawn;

			if (pawn.abilities.GetAbility(Props.abilityDef) == null)
			{
				this.AbilityForReading.pawn = EquippedPawn;
				this.AbilityForReading.verb.caster = EquippedPawn;
				pawn.abilities.GainAbility(Props.abilityDef);
			}
		}

		public override void Notify_Unequipped(Pawn pawn)
		{
			if (pawn.abilities.GetAbility(Props.abilityDef) != null)
			{
				pawn.abilities.RemoveAbility(Props.abilityDef);
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Deep.Look<Ability>(ref this.ability, "ability", Array.Empty<object>());
			if (Scribe.mode == LoadSaveMode.PostLoadInit && EquippedPawn != null)
			{
				this.AbilityForReading.pawn = EquippedPawn;
				this.AbilityForReading.verb.caster = EquippedPawn;
			}
		}

	}
}
