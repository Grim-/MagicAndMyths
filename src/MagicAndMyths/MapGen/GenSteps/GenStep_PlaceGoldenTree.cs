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
}
