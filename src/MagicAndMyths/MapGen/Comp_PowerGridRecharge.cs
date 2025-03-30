using RimWorld;

namespace MagicAndMyths
{
    public class CompProperties_PowerGridRecharge : CompProperties_EnergyRecharge
    {

        public CompProperties_PowerGridRecharge()
        {
            compClass = typeof(Comp_PowerGridRecharge);
        }
    }
    public class Comp_PowerGridRecharge : Comp_EnergyRecharge
    {
        private bool PowerAvailable()
        {
            var powerComp = parent.GetComp<CompPowerTrader>();
            return powerComp?.PowerOn ?? false;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (PowerAvailable())
            {
                ApplyRecharge(Props.rechargeRate);
            }
        }
    }
}