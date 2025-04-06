using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class MapModifierManager : IExposable
    {
        private Map map;
        private List<MapModifier> activeModifiers = new List<MapModifier>();

        public MapModifierManager(Map map)
        {
            this.map = map;
        }

        public void AddModifier(MapModifier modifier)
        {
            activeModifiers.Add(modifier);
        }

        public void MapTick()
        {
            foreach (var modifier in activeModifiers)
            {
                modifier.Tick();
            }
        }

        public void ExposeData()
        {
         
        }
    }
}

