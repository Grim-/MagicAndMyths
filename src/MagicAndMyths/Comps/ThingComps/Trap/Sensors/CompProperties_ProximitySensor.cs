using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ProximitySensor : CompProperties_SensorBase
    {
        public float sensorRadius = 3f;

        public CompProperties_ProximitySensor()
        {
            compClass = typeof(Comp_ProximitySensor);
        }
    }


    public class Comp_ProximitySensor : Comp_SensorBase
    {
        private CompProperties_ProximitySensor Props => (CompProperties_ProximitySensor)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                EventManager.Instance.OnCellEntered += EventManager_OnCellEntered;
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            EventManager.Instance.OnCellEntered -= EventManager_OnCellEntered;
            base.PostDestroy(mode, previousMap);
        }

        private void EventManager_OnCellEntered(Pawn arg1, IntVec3 arg2)
        {
            if (IsInProximity(arg1))
            {
                this.OnTargetSensed(arg1);
                return;
            }
        }


        public virtual bool IsInProximity(Pawn pawn)
        {
            return pawn.Position.InHorDistOf(this.parent.Position, Props.sensorRadius);
        }
    }


}