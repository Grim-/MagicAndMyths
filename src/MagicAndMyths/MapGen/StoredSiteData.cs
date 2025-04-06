using RimWorld.Planet;
using Verse;

namespace MagicAndMyths
{
    public class StoredSiteData : IExposable
    {
        public int tileId;
        public Site site;
        public DungeonMapParent mapParent;
        public StoredSiteData() { }

        public StoredSiteData(int tileId, Site site, DungeonMapParent mapParent)
        {
            this.tileId = tileId;
            this.site = site;
            this.mapParent = mapParent;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref tileId, "tileId");
            Scribe_References.Look(ref site, "site");
            Scribe_References.Look(ref mapParent, "mapParent");
        }
    }
}
