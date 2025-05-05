﻿using RimWorld;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_GrantTrait : EnchantEffectDef
    {
        public TraitDef trait;
        public bool overwriteExisting = true;
        public bool removeOnUnEquip = true;

        public EnchantEffectDef_GrantTrait()
        {
            workerClass = typeof(EnchantEffect_GrantTrait);
        }

        public override string EffectDescription => $"Grants the {trait.LabelCap} trait while equipped";


    }

    public class EnchantEffect_GrantTrait : EnchantWorker
    {
        EnchantEffectDef_GrantTrait Def => (EnchantEffectDef_GrantTrait)def;

        private bool hasGranted = false;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);


            if (EquippingPawn.story != null && !EquippingPawn.story.traits.HasTrait(Def.trait))
            {
                int defaultDegree = 0;
                TraitDegreeData degreeData = Def.trait.degreeDatas.FirstOrDefault(d => d.degree == defaultDegree)
                    ?? Def.trait.degreeDatas.FirstOrDefault();

                if (degreeData != null)
                {
                    EquippingPawn.story.traits.GainTrait(new Trait(Def.trait, defaultDegree));
                    hasGranted = true;
                }
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            if (hasGranted && EquippingPawn.story.traits.HasTrait(Def.trait))
            {
                Trait trait = EquippingPawn.story.traits.GetTrait(Def.trait);
                if (trait != null)
                {
                    EquippingPawn.story.traits.RemoveTrait(trait);
                    hasGranted = false;
                }
            }

            base.Notify_Unequipped(pawn);
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref hasGranted, "hasGranted");
        }
    }
}