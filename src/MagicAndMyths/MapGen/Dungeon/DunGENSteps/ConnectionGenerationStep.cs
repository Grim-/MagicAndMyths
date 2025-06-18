using Verse;

namespace MagicAndMyths
{
    public class ConnectionGenerationStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            Log.Message("Creating room connections..");
            context.Dungeon.ConnectionManager.GenerateConnectionsFromRoomGraph();
        }
    }
}
