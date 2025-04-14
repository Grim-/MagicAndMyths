using Verse;

namespace MagicAndMyths
{
    public class CompProperties_WeightSensor : CompProperties_SensorBase
    {
        public float minimumTriggerWeight = 20f;

        public CompProperties_WeightSensor()
        {
            compClass = typeof(Comp_ProximitySensor);
        }
    }


    public class Comp_WeightSensor : Comp_SensorBase
    {
        private CompProperties_WeightSensor Props => (CompProperties_WeightSensor)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                EventManager.OnCellEntered += EventManager_OnCellEntered;
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            EventManager.OnCellEntered -= EventManager_OnCellEntered;
            base.PostDestroy(mode, previousMap);
        }

        private void EventManager_OnCellEntered(Pawn arg1, IntVec3 arg2)
        {
            if (arg1.Position == this.parent.Position && arg1.BodySize >= Props.minimumTriggerWeight)
            {
                this.OnTargetSensed(arg1);
                return;
            }
        }
    }
}