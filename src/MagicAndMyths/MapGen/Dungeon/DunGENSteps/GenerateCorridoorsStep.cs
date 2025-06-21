using Verse;

namespace MagicAndMyths
{
    public class GenerateCorridoorsStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            Log.Message($"Creating corridoors for room connections..");
            foreach (var item in context.Dungeon.ConnectionManager.AllConnections)
            {
                item.SetCorridoor(CorridoorUtility.GenerateCorridor(context.Map, context.Dungeon, item.roomA, item.roomB, context.CorridoorWidth, false));
            }
        }
    }
}
