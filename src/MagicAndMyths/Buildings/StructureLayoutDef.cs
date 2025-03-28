using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class StructureLayoutDef : Def
    {
        public List<BuildingStage> stages = new List<BuildingStage>();
    }

    public class BuildingStage
    {
        public IntVec2 size;

        public List<TerrainPlacement> terrain = new List<TerrainPlacement>();
        public List<ThingPlacement> walls = new List<ThingPlacement>();
        public List<ThingPlacement> doors = new List<ThingPlacement>();
        public List<ThingPlacement> power = new List<ThingPlacement>();
        public List<ThingPlacement> furniture = new List<ThingPlacement>();
        public List<ThingPlacement> other = new List<ThingPlacement>();
    }

    public class TerrainPlacement : ThingPlacement
    {
        public TerrainDef terrain;
    }

    public class ThingPlacement
    {
        public ThingDef thing;
        public IntVec2 position;
        public Rot4 rotation = Rot4.North;
        public ThingDef stuff;
    }
}
