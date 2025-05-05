using RimWorld;
using System;
using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef
    {
        public string defName = "ANAME";
        public string label = "ALABEL";
        public Type workerClass = typeof(EnchantWorker);

        public virtual string EffectDescription { get; private set; } = "";


        public EnchantWorker CreateWorker(ThingWithComps parentEquipment, Comp_Enchant materiaComp, EnchantSlot materiaSlot)
        {
            EnchantWorker newWorker = (EnchantWorker)Activator.CreateInstance(workerClass);
            newWorker.def = this;
            newWorker.ParentEquipment = parentEquipment;
            newWorker.MateriaComp = materiaComp;
            newWorker.MateriaSlot = materiaSlot;
            return newWorker;
        }
    }
}
