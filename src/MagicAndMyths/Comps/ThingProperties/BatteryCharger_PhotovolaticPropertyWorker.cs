using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class BatteryCharger_PhotovolaticPropertyWorker : PhotovolaticPropertyWorker
    {
        protected Building Building => parent as Building;

        protected Building_Battery _Battery;

        protected Building_Battery Battery
        {
            get
            {
                if (_Battery == null)
                {
                    if (Building != null && Building is Building_Battery battery)
                    {
                        _Battery = battery;
                    }
                }

                return _Battery;
            }
        }

        protected override void OnRechargeTick()
        {
            base.OnRechargeTick();

            if (Battery != null && Battery.PowerComp is CompPowerBattery powerBattery)
            {
                if (CanRecharge())
                {
                    powerBattery.AddEnergy(100);
                }
            }
        }


        protected override bool CanRecharge()
        {
            return Battery != null && !Building.Position.Roofed(parent.Map) && GenCelestial.IsDaytime(GenCelestial.CurCelestialSunGlow(parent.Map));
        }

        public override string GetDescription()
        {
            return "Generates power every hour the sun is up.";
        }
    }
}
