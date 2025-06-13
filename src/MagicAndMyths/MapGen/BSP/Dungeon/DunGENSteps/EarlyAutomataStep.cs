namespace MagicAndMyths
{
    public class EarlyAutomataStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            if (context.Def.earlyAutomata != null)
            {
                CellularAutomataManager.ApplyRules(context.Map, context.Dungeon, context.Def.earlyAutomata);
            }
        }
    }
}
