using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CustomMapParent : MapParent
    {
        private List<MapModifier> activeModifiers = new List<MapModifier>();
        public bool ShouldDestroy = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ShouldDestroy, "shouldDestroy", false);
        }

        public override void Tick()
        {
            base.Tick();


            if (this.Map != null)
            {
                foreach (var modifier in activeModifiers)
                {
                    modifier.Tick();
                }
            }
        }

        public void AddModifier(MapModifier modifier)
        {
            activeModifiers.Add(modifier);
        }

        public void RemoveModifier(MapModifier modifier)
        {
            activeModifiers.Remove(modifier);
        }

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            alsoRemoveWorldObject = false;
            return ShouldDestroy && !this.Map.mapPawns.AnyPawnBlockingMapRemoval;
        }
    }
}
