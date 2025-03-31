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
            bool CanRemove = ShouldDestroy && !this.Map.mapPawns.AnyPawnBlockingMapRemoval;

            if (CanRemove)
            {
                Log.Message("Returning all pawns to a player map");
                foreach (var item in this.Map.mapPawns.AllPawns)
                {
                    if (item.Faction == RimWorld.Faction.OfPlayer)
                    {
                        item.TransferToMap(Find.AnyPlayerHomeMap.Center, WorldCustomSiteManager.StartingColonyMap);
                    }
                }
            }

            alsoRemoveWorldObject = false;
            return CanRemove;
        }
    }
}
