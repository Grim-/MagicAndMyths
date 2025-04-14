using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{

    public abstract class CompProperties_TriggerBase : CompProperties
    {
        public bool applyInRadius = false;
        public float applyRadius = 4f;
        public IntRange triggerCooldownTicks = new IntRange(10, 10);
        public EffecterDef triggerEffecter;
        public float triggerEffecterScale = 1f;

        public bool spawnEffecterOnTarget = false;
    }


    /// <summary>
    /// Base class for components triggered by sensors
    /// </summary>
    public abstract class Comp_TriggerBase : ThingComp
    {
        private CompProperties_TriggerBase Props => (CompProperties_TriggerBase)props;
        protected int LastTriggerTick = -1;


        protected int _CooldownTicks = 10;
        protected virtual int CooldownTicks
        {
            get => _CooldownTicks;
        }


        public bool IsOnCooldown
        {
            get
            {
                if (LastTriggerTick == -1)
                {
                    return false;
                }
                return Current.Game.tickManager.TicksGame <= LastTriggerTick + CooldownTicks;
            }
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _CooldownTicks = Props.triggerCooldownTicks.RandomInRange;
        }

        protected virtual void PlayTriggerVFX(Pawn pawn)
        {
            if (Props.triggerEffecter != null)
            {
                IntVec3 spawnPosition = Props.spawnEffecterOnTarget == false ? this.parent.Position : pawn.Position;
                Props.triggerEffecter.Spawn(spawnPosition, this.parent.Map, Props.triggerEffecterScale);
            }
        }

        public virtual void Trigger(Pawn pawn)
        {
            if (!CanTrigger(pawn))
            {
                return;
            }

            LastTriggerTick = Current.Game.tickManager.TicksGame;
            PlayTriggerVFX(pawn);

            if (Props.applyInRadius)
            {
                foreach (var item in GetPawnsInRange(this.parent.Position, this.parent.Map, Props.applyRadius))
                {
                    if (CanApplyToPawn(item))
                    {
                        ApplyTo(item);
                    }
                }
            }
            else
            {
                if (CanApplyToPawn(pawn))
                {
                    ApplyTo(pawn);
                }
            }
        }

        protected virtual List<Pawn> GetPawnsInRange(IntVec3 Position, Map map, float radius)
        {
            return GenRadial.RadialDistinctThingsAround(Position, map, radius, true).Where(x => x is Pawn).Cast<Pawn>().ToList();
        }

        protected bool CanTrigger(Pawn pawn)
        {
            return !IsOnCooldown;
        }

        protected abstract void ApplyTo(Pawn pawn);

        protected bool CanApplyToPawn(Pawn pawn)
        {
            return !pawn.Dead && !pawn.Destroyed;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref LastTriggerTick, "lastTriggerTick", -1);
        }
    }
}