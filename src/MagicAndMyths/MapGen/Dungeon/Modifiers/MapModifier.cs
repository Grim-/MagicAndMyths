using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public abstract class MapModifier : IExposable
    {
        public MapModifierDef def;
        public Map map;
        protected int ticksUntilNext;


        protected virtual bool AppliesInstantly => true;

        public virtual int MinTicksBetweenEffects => 250;
        public virtual int MaxTicksBetweenEffects => 1000;
        public virtual Color ModifierColor => Color.white;

        public MapModifier(Map map)
        {
            this.map = map;
            ResetTimer();
            if (AppliesInstantly)
            {
                ApplyEffect();
            }
        }


        protected void ResetTimer()
        {
            ticksUntilNext = Rand.Range(MinTicksBetweenEffects, MaxTicksBetweenEffects);
        }

        public void Tick()
        {
            ticksUntilNext--;
            if (ticksUntilNext <= 0)
            {
                ApplyEffect();
                ResetTimer();
            }
        }

        public virtual bool ShouldRemove()
        {
            return false;
        }

        public virtual Texture2D GetModifierTexture()
        {
            return TexCommand.Draft;
        }

        public virtual string GetModifierExplanation()
        {
            return $"Map modifier: {GetType().Name}\nTime until next effect: {ticksUntilNext} ticks";
        }

        public virtual string GetModifierName()
        {
            return GetType().Name.Replace("MapModifier_", "");
        }

        public abstract void ApplyEffect();
        public virtual void ExposeData()
        {
            Scribe_References.Look(ref map, "map");
            Scribe_Values.Look(ref ticksUntilNext, "ticksUntilNext");
        }
    }
}

