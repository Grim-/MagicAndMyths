using Verse;

namespace MagicAndMyths
{
    public class SidePathProcessingStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            Log.Message("Processing side paths...");
            var sidePathProcessor = new SidePathProcessor(context);
            sidePathProcessor.ProcessSidePaths();
            sidePathProcessor.EnsureAllRoomsConnected();
        }
    }
}