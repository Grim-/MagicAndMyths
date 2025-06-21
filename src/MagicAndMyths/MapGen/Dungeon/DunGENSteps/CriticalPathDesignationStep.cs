using Verse;

namespace MagicAndMyths
{
    public class CriticalPathDesignationStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            Log.Message("Designating critical path");
            var pathDesignator = new CriticalPathProcessorFlexibleLength(context, context.Dungeon);
            pathDesignator.DesignateCriticalPath(context, context.Dungeon);
        }
    }
}
