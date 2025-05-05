using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_GrantGene : EnchantEffectDef
    {
        public GeneDef gene;
        public bool isXenogene = true;

        public EnchantEffectDef_GrantGene()
        {
            workerClass = typeof(EnchantEffect_GrantGene);
        }
        public override string EffectDescription => $"Grants the {gene.LabelCap} gene while equipped. \n\n {gene.DescriptionFull}";
    }

    public class EnchantEffect_GrantGene : EnchantWorker
    {
        EnchantEffectDef_GrantGene Def => (EnchantEffectDef_GrantGene)def;

        private Gene geneRef;


        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);

            if (!ModsConfig.BiotechActive)
            {
                return;
            }


            if (!EquippingPawn.genes.HasActiveGene(Def.gene))
            {
                geneRef = EquippingPawn.genes.AddGene(Def.gene, Def.isXenogene);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
           

            if (!ModsConfig.BiotechActive)
            {
                return;
            }

            if (geneRef != null && EquippingPawn.genes.HasActiveGene(Def.gene))
            {
                EquippingPawn.genes.RemoveGene(geneRef);
                geneRef = null;
            }

            base.Notify_Unequipped(pawn);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref geneRef, "geneRef");
        }
    }

}