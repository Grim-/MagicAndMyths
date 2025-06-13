using Verse;

namespace MagicAndMyths
{
    public class MinimumSpanningTreeStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            Log.Message("Creating Minimum spanning tree...");
            var mstGenerator = new MinimumSpanningTreeGenerator(context.Dungeon);
            mstGenerator.CreateMinimumSpanningTree();
        }
    }
}