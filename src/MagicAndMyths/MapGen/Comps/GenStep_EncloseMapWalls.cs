using Verse;

namespace MagicAndMyths
{
    public class GenStep_EncloseMapWalls : GenStep
    {
        public override int SeedPart => 1234567;

        // Variable to control wall thickness
        public int WallThickness = 1;

        public override void Generate(Map map, GenStepParams parms)
        {
            // Get map dimensions
            int mapWidth = map.Size.x;
            int mapHeight = map.Size.z;

            for (int thickness = 0; thickness < WallThickness; thickness++)
            {
                // Top and bottom walls
                for (int x = 0; x < mapWidth; x++)
                {
                    IntVec3 topPos = new IntVec3(x, 0, thickness);
                    BuildWallAt(map, topPos);
                    IntVec3 bottomPos = new IntVec3(x, 0, mapHeight - 1 - thickness);
                    BuildWallAt(map, bottomPos);
                }

                for (int z = thickness + 1; z < mapHeight - 1 - thickness; z++)
                {
                    IntVec3 leftPos = new IntVec3(thickness, 0, z);
                    BuildWallAt(map, leftPos);
                    IntVec3 rightPos = new IntVec3(mapWidth - 1 - thickness, 0, z);
                    BuildWallAt(map, rightPos);
                }
            }
        }

        private void BuildWallAt(Map map, IntVec3 position)
        {
            GenSpawn.Spawn(MagicAndMythDefOf.DungeonWall, position, map);
        }
    }
}
