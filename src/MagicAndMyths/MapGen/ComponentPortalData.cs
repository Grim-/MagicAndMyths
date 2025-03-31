using Verse;

namespace MagicAndMyths
{
    public class ComponentPortalData : IExposable
    {
        public int uniqueId;
        public MapGeneratorDef mapGeneratorDef;
        public CustomMapParent mapParent;

        public ComponentPortalData()
        {
            // For Scribe
        }

        public ComponentPortalData(int uniqueId, MapGeneratorDef mapGeneratorDef, CustomMapParent mapParent)
        {
            this.uniqueId = uniqueId;
            this.mapGeneratorDef = mapGeneratorDef;
            this.mapParent = mapParent;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref uniqueId, "uniqueId");
            Scribe_Defs.Look(ref mapGeneratorDef, "mapGeneratorDef");
            Scribe_References.Look(ref mapParent, "mapParent");
        }
    }
}
