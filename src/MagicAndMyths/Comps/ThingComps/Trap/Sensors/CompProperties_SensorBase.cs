using Verse;

namespace MagicAndMyths
{
    public class CompProperties_SensorBase : CompProperties
    {
        public CompProperties_SensorBase()
        {
            compClass = typeof(Comp_SensorBase);
        }
    }

    public class Comp_SensorBase : ThingComp
    {
        protected int LastSenseTick = -1;


        protected int _CooldownTicks = 10;
        protected virtual int CooldownTicks
        {
            get => _CooldownTicks;
        }


        public bool IsOnCooldown
        {
            get
            {
                if (LastSenseTick == -1)
                {
                    return false;
                }
                return Current.Game.tickManager.TicksGame <= LastSenseTick + CooldownTicks;
            }
        }




        protected bool IsForceDisabled = false;

        protected virtual bool IsEnabled
        {
            get
            {

                if (IsForceDisabled)
                {
                    return false;
                }

                return true;
            }
        }


        protected virtual void OnTargetSensed(Pawn pawn)
        {
            if (IsOnCooldown || !IsEnabled)
            {
                return;
            }

            this.parent.GetComp<Comp_TrapBase>()?.OnTrapSensorTriggered(pawn);
        }





        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref IsForceDisabled, "isForceDisabled", false);
            Scribe_Values.Look(ref LastSenseTick, "lastTriggerTick", -1);
        }
    }
}