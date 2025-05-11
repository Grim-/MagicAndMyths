using RimWorld;
using System;
using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef: IExposable
    {
        public string defName = "ANAME";
        public string label = "ALABEL";
        public Type workerClass = typeof(EnchantWorker);

        public virtual string EffectDescription { get; private set; } = "";


        public EnchantWorker CreateWorker(ThingWithComps parentEquipment, EnchantInstance instance, Comp_EnchantProvider materiaComp)
        {
            EnchantWorker newWorker = (EnchantWorker)Activator.CreateInstance(workerClass);
            newWorker.def = this;
            newWorker.ParentEquipment = parentEquipment;
            newWorker.parentInstance = instance;
            return newWorker;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref workerClass, "workerClass");
        }
    }
}
