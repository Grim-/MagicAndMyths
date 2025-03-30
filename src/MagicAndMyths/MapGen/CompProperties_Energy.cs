using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_Energy : CompProperties
    {
        public float maxEnergy = 100f;

        public CompProperties_Energy()
        {
            compClass = typeof(Comp_Energy);
        }
    }

    public class Comp_Energy : ThingComp, IEnergyProvider
    {
        private float energy;

        public CompProperties_Energy Props => (CompProperties_Energy)props;
        public virtual float Energy => energy;
        public virtual float MaxEnergy => Props.maxEnergy;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                energy = MaxEnergy;
            }
        }

        public virtual bool TryUseEnergy(float amount)
        {
            if (energy >= amount)
            {
                energy -= amount;
                return true;
            }
            return false;
        }

        public virtual void AddEnergy(float amount)
        {
            energy = Mathf.Min(MaxEnergy, energy + amount);
        }

        public virtual bool HasEnough(float amount)
        {
            return energy >= amount;
        }

        public virtual float GetEnergyPercent()
        {
            return energy / MaxEnergy;
        }

        public override string CompInspectStringExtra()
        {
            return $"Energy: {energy:F0} / {MaxEnergy:F0}";
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref energy, "energy", 0f);
        }
    }
}