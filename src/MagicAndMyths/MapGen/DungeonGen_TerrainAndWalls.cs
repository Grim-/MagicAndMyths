using RimWorld;
using RimWorld.BaseGen;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class DungeonGen_TerrainAndWalls : DungeonGen
    {
        private const int WALL_THICKNESS = 3;
        private readonly ThingDef wallDef = ThingDefOf.Wall;
        private readonly TerrainDef floorDef = TerrainDefOf.Concrete;

        public override int Priority => 10;

        public DungeonGen_TerrainAndWalls(Map map) : base(map) { }

        public override void Generate()
        {
            for (int x = 0; x < map.Size.x; x++)
            {
                for (int z = 0; z < map.Size.z; z++)
                {
                    bool isEdge = x < WALL_THICKNESS ||
                                x >= map.Size.x - WALL_THICKNESS ||
                                z < WALL_THICKNESS ||
                                z >= map.Size.z - WALL_THICKNESS;

                    IntVec3 cell = new IntVec3(x, 0, z);

                    if (isEdge)
                    {
                        // First clear the cell
                        map.terrainGrid.SetTerrain(cell, TerrainDefOf.Concrete);
                        foreach (Thing t in cell.GetThingList(map).ToList())
                        {
                            t.Destroy();
                        }
                        BaseGen.Generate();

                        // Then build the wall
                        Thing wall = ThingMaker.MakeThing(wallDef, ThingDefOf.Steel);
                        GenSpawn.Spawn(wall, cell, map);
                    }
                }
            }
        }
    }
}

