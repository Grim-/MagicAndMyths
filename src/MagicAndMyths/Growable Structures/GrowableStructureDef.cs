using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class GrowableStructureDef : ThingDef
    {
        public StructureLayoutDef structureLayout;
        public ThingDef rootDef;
        public int growthDays = 3;

        public int ticksBetweenPlacements = 50;

        public ThingDef defaultWallStuff;
        public ThingDef defaultDoorStuff;
        public ThingDef defaultFurnitureStuff;
        public TerrainDef defaultFloorStuff;

        public ThingFilter disallowedThingFilter;
    }


}
