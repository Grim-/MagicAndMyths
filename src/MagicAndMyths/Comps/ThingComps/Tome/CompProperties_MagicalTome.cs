using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_MagicalTome : CompProperties
    {
        public List<AbilityDef> abilityDefs;

        public CompProperties_MagicalTome()
        {
            this.compClass = typeof(Comp_MagicalTome);
        }
    }

    public class Comp_MagicalTome : CompEquippable
    {
        private List<Ability> abilities = new List<Ability>();

        private CompProperties_MagicalTome Props => (CompProperties_MagicalTome)props;

        public List<Ability> AbilitiesForReading
        {
            get
            {
                if (this.abilities.NullOrEmpty())
                {
                    this.abilities = new List<Ability>();
                    foreach (AbilityDef abilityDef in Props.abilityDefs)
                    {
                        this.abilities.Add(AbilityUtility.MakeAbility(abilityDef, Holder));
                    }
                }
                return this.abilities;
            }
        }

        public virtual void UsedOnce()
        {

        }

        public override void Notify_Equipped(Pawn pawn)
        {
            foreach (Ability ability in AbilitiesForReading)
            {
                ability.pawn = pawn;
                ability.verb.caster = pawn;

                if (!pawn.abilities.abilities.Contains(ability))
                {
                    pawn.abilities.abilities.Add(ability);
                }
            }

            pawn.abilities.Notify_TemporaryAbilitiesChanged();
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            foreach (Ability ability in AbilitiesForReading)
            {
                ability.pawn = pawn;
                ability.verb.caster = pawn;

                if (pawn.abilities.abilities.Contains(ability))
                {
                    pawn.abilities.abilities.Remove(ability);
                }
            }

            pawn.abilities.Notify_TemporaryAbilitiesChanged();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref abilities, "abilities", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit && Holder != null)
            {
                foreach (Ability ability in AbilitiesForReading)
                {
                    ability.pawn = Holder;
                    ability.verb.caster = Holder;
                }
            }
        }
    }


}
