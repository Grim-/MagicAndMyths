using Verse;

namespace MagicAndMyths
{
    public class PostAutomataStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            if (context.Def.postGenAutomata != null)
            {
                Log.Message("Applying Post-Generation Cellular Automata");
                CellularAutomataManager.ApplyRules(context.Map, context.Dungeon, context.Def.postGenAutomata);
            }
        }
    }
}
