using RimWorld;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class GridApplicationStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            Log.Message("Drawing connections");
            context.Dungeon.ConnectionManager.ApplyConnectionsToGrid();

            Log.Message("Drawing rooms");
            ApplyRoomsToGrid(context);
            ClearFloorCell(context);
        }


        public static void DrawGrid(DungeonGenerationContext context)
        {
            if (context !=null && context.Dungeon != null && context.Dungeon.Rooms != null && context.Dungeon.GridManager.dungeonGrid != null)
            {
                Log.Message("Drawing connections");
                context.Dungeon.ConnectionManager.ApplyConnectionsToGrid();
                ApplyRoomsToGrid(context);
                ClearFloorCell(context);
            }
        }

        private static void ApplyRoomsToGrid(DungeonGenerationContext context)
        {
            foreach (var room in context.Dungeon.Rooms)
            {
                if (room?.roomCells == null)
                    continue;

                foreach (IntVec3 cell in room.roomCells)
                {
                    context.Dungeon.MarkCellAsFloor(cell);
                }

                if (context.Dungeon.ConnectionManager == null)
                    continue;

                var connections = context.Dungeon.ConnectionManager.GetConnectionsForRoom(room);

                foreach (var connection in connections)
                {
                    if (connection?.Corridoor == null)
                        continue;

                    var connectionCells = connection.GetAllCells();
                    if (connectionCells == null)
                        continue;

                    foreach (var connectionCell in connectionCells)
                    {
                        context.Dungeon.MarkCellAsFloor(connectionCell);
                    }
                }
            }
        }

        private static void ClearFloorCell(DungeonGenerationContext context)
        {
            foreach (IntVec3 cell in context.Map.AllCells)
            {
                if (context.Dungeon.IsCellFloor(cell))
                {
                    context.Map.thingGrid.ThingsAt(cell)
                        .ToList()
                        .ForEach(t => t.Destroy());

                    context.Map.terrainGrid.SetTerrain(cell, context.Def.TerrainDef);
                    context.Map.terrainGrid.SetUnderTerrain(cell, context.Def.TerrainDef);
                }
            }
        }
    }
}
