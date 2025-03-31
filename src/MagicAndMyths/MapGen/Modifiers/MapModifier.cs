using Verse;

namespace MagicAndMyths
{
    public abstract class MapModifier
    {
        public Map map;
        protected int ticksUntilNext;

        public virtual int MinTicksBetweenEffects => 250;
        public virtual int MaxTicksBetweenEffects => 1000;

        public MapModifier(Map map)
        {
            this.map = map;
            ResetTimer();
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

        public abstract void ApplyEffect();
    }
}

