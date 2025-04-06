namespace MagicAndMyths
{

    public class CompProperties_SolarRecharge : CompProperties_EnergyRecharge
    {
        public CompProperties_SolarRecharge()
        {
            compClass = typeof(Comp_SolarRecharge);
        }
    }

    public class Comp_SolarRecharge : Comp_EnergyRecharge
    {
        private float GetSunlight()
        {
            if (parent.Map == null) 
                return 0f;

            return parent.Map.skyManager.CurSkyGlow;
        }

        public override void CompTick()
        {
            base.CompTick();
            float sunlight = GetSunlight();
            if (sunlight > 0f)
            {
                ApplyRecharge(Props.rechargeRate * sunlight);
            }
        }
    }
}