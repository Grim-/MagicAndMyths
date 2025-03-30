using Verse;

namespace MagicAndMyths
{
    public class CompProperties_EnergyRecharge : CompProperties
    {
        public float rechargeRate = 0.5f;

        public CompProperties_EnergyRecharge()
        {
            compClass = typeof(Comp_EnergyRecharge);
        }
    }

    public class Comp_EnergyRecharge : ThingComp
    {
        protected Comp_Energy energyComp;
        protected bool isRecharging;

        public CompProperties_EnergyRecharge Props => (CompProperties_EnergyRecharge)props;
        public bool IsRecharging => isRecharging;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            energyComp = parent.GetComp<Comp_Energy>();
        }

        protected virtual void ApplyRecharge(float amount)
        {
            if (energyComp == null) return;

            float previousEnergy = energyComp.Energy;
            energyComp.AddEnergy(amount);
            isRecharging = previousEnergy < energyComp.Energy;
        }

        public override string CompInspectStringExtra()
        {
            return isRecharging ? "Recharging" : null;
        }
    }
}