using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_EnergyRegenComp : HediffCompProperties
    {
        public bool hasNaturalRegeneration = true;
        public float chargePerHour = 8;

        public HediffCompProperties_EnergyRegenComp()
        {
            compClass = typeof(HediffComp_EnergyRegenComp);
        }
    }
    public class HediffComp_EnergyRegenComp : HediffComp
    {
        public HediffCompProperties_EnergyComp Props => (HediffCompProperties_EnergyComp)props;

        private HediffComp_EnergyComp _EnergyComp;
        private HediffComp_EnergyComp EnergyComp
        {
            get
            {
                if (_EnergyComp == null)
                {
                    _EnergyComp = this.parent.GetComp<HediffComp_EnergyComp>();
                }

                return _EnergyComp;
            }
        }


        public override void CompPostMake()
        {
            base.CompPostMake();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            float chargePerTick = Props.chargePerHour / GenDate.TicksPerHour;


            if (EnergyComp != null)
            {
                EnergyComp.AddEnergy(chargePerTick);
            }


        }
    }
}