using System.Linq;
using UnityEngine;
using Verse;
namespace MagicAndMyths
{
    public class StackingHediffDef : HediffDef
    {
        public int initialStacks = 1;
        public int maxStacks = 10;
        public bool useStagesAsStackCount = false;
        public bool stackGainRefreshesDisappearsDuration = false;
        public bool stackGainAddsDisappearsDuration = false;
        public int stackGainDurationAdd = 1250;
        public bool losesStacksPerInterval = false;
        public int ticksBetweenStackLoss = 300;
        public int stacksLostPerTickInterval = 1;
        public bool removeOnZeroStacks = true;
        public StackingHediffDef()
        {
            hediffClass = typeof(HediffWithStacks);
        }
    }


    //public class StackStageData
    //{
    //    public HediffStage stage;
    //}

    public class HediffWithStacks : HediffWithComps
    {
        protected int _CurrentStackLevel = 1;
        public int StackLevel => _CurrentStackLevel;
        public int MaxStackLevel
        {
            get
            {
                if (Def != null)
                {
                    if (Def.useStagesAsStackCount)
                    {
                        return Def.stages.Count;
                    }
                    return Def.maxStacks;
                }
                return def.stages.Count;
            }
        }

        public override string Label => $"[{StackLevel + 1}] " + base.Label;

        public override string Description => base.Description + $"\r\n{StackLevel + 1} stacks.";
        public override HediffStage CurStage => GetStageForStackLevel(StackLevel);
        protected int stackLossTicker = 0;
        private StackingHediffDef Def => (StackingHediffDef)def;
        public override void Tick()
        {
            base.Tick();
            if (Def.losesStacksPerInterval)
            {
                stackLossTicker++;
                if (stackLossTicker >= Def.ticksBetweenStackLoss)
                {
                    RemoveStack(Def.stacksLostPerTickInterval);
                    stackLossTicker = 0;
                }
            }
        }
        public HediffStage GetStageForStackLevel(int Level)
        {
            if (def.stages == null || def.stages.Count == 0 || Level <= 0)
            {
                return null;
            }
            if (Level > def.stages.Count)
            {
                return def.stages[def.stages.Count - 1];
            }
            return def.stages[Level - 1];
        }

        public void AddStack(int stacksToAdd = 1)
        {
            _CurrentStackLevel += stacksToAdd;
            if (_CurrentStackLevel > MaxStackLevel)
            {
                _CurrentStackLevel = MaxStackLevel;

            }
            if (this.TryGetComp(out HediffComp_Disappears _Disappears) && Def.stackGainRefreshesDisappearsDuration)
            {
                _Disappears.SetDuration(_Disappears.Props.disappearsAfterTicks.RandomInRange);
            }

            if (_CurrentStackLevel >= MaxStackLevel)
            {
                OnMaxStacks();
            }

            OnStacksChange(_CurrentStackLevel);
        }

        public void RemoveStack(int stacksToRemove = 1)
        {
            _CurrentStackLevel -= stacksToRemove;
            OnStacksChange(_CurrentStackLevel);

            if (_CurrentStackLevel <= 0)
            {
                _CurrentStackLevel = 0;
                if (Def != null && Def.removeOnZeroStacks)
                {
                    this.pawn.health.RemoveHediff(this);
                }
            }

        }

        protected virtual void OnStacksChange(int newStackLevel)
        {
            foreach (var item in comps)
            {
                if (item is HediffComp_BaseStack baseStack)
                {
                    baseStack.OnStacksChanged(newStackLevel);
                }
            }
        }

        protected virtual void OnMaxStacks()
        {
            foreach (var item in comps)
            {
                if (item is HediffComp_BaseStack baseStack)
                {
                    baseStack.OnMaxStacks();
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _CurrentStackLevel, "CurrentStackLevel", 0);
            Scribe_Values.Look(ref stackLossTicker, "StackLossTicker", 0);
        }
    }
}