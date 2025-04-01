using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class GenStep_PlaceGoldenTree : GenStep
    {
        public override int SeedPart => 1234567;

        public override void Generate(Map map, GenStepParams parms)
        {
            Plant plant = (Plant)GenSpawn.Spawn(MagicAndMythDefOf.Plant_GoldenTree, map.Center, map);
            if (plant != null)
            {
                plant.Growth = 1;
            }
        }
    }


    public class GenStep_StampStructure : GenStep
    {
        public override int SeedPart => 1234567;
        public override void Generate(Map map, GenStepParams parms)
        {
            StructureLayoutDef structureLayoutDef = DefDatabase<StructureLayoutDef>.GetNamed("EmoGrowTest");
            IntVec2 buildMaxSize = structureLayoutDef.MaxBuildSize;

            // Find a suitable position with enough space
            IntVec3 position = FindSpaceFor(map, buildMaxSize);

            if (position.IsValid)
            {
                StructureBuilder.BuildStructure(structureLayoutDef, position, structureLayoutDef.LastStageIndex, map);
            }
        }

        private IntVec3 FindSpaceFor(Map map, IntVec2 size)
        {
            // Try the center first
            if (CanPlaceAt(map, map.Center, size))
            {
                return map.Center;
            }

            int maxRadius = (int)(map.Size.x * 0.4f); // Don't search the entire map

            for (int radius = 10; radius < maxRadius; radius += 10)
            {
                foreach (IntVec3 cell in CellRect.CenteredOn(map.Center, radius).EdgeCells)
                {
                    if (CanPlaceAt(map, cell, size))
                    {
                        return cell;
                    }
                }
            }

            return IntVec3.Invalid;
        }

        private bool CanPlaceAt(Map map, IntVec3 pos, IntVec2 size)
        {
            CellRect rect = CellRect.CenteredOn(pos, size.x / 2, size.z / 2);
            if (!rect.FullyContainedWithin(new CellRect(0, 0, map.Size.x, map.Size.z)))
            {
                return false;
            }
            return true;
        }
    }
}
