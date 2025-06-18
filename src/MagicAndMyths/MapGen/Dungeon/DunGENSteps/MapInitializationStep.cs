using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class MapInitializationStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            FillMapWithWalls(context);
        }

        private void FillMapWithWalls(DungeonGenerationContext context)
        {
            foreach (IntVec3 cell in context.Map.AllCells)
            {
                context.Constructor.PlaceWall(cell);
            }
        }
    }
}
