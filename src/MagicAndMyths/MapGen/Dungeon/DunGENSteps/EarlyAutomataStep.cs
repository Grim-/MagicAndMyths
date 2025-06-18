using Verse;

namespace MagicAndMyths
{
    public class EarlyAutomataStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            if (context.Def.earlyAutomata != null)
            {
                Log.Message($"Applying Early Automata..");
                CellularAutomataManager.ApplyRules(context.Map, context.Dungeon, context.Def.earlyAutomata);
            }
        }
    }
}
