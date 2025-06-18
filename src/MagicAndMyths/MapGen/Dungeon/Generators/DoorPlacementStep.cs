using Verse;

namespace MagicAndMyths
{
    public class DoorPlacementStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            DoorPlacementManager doorManager = new DoorPlacementManager(context);
            doorManager.PlaceAllDoors();
            context.DoorManager = doorManager;

            Log.Message($"<color=yellow>Placed {doorManager.PlacedDoors.Count} doors in dungeon</color>");

            foreach (var doorCell in doorManager.PlacedDoors)
            {
                context.Dungeon.MarkCellProtected(doorCell, true);
            }
        }
    }
}