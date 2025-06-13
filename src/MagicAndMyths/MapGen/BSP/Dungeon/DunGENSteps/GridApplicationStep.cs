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
            ClearWalls(context);
        }

        private void ApplyRoomsToGrid(DungeonGenerationContext context)
        {
            FillMapWithWalls(context);

            foreach (var room in context.Dungeon.GetAllRooms())
            {
                foreach (IntVec3 cell in room.roomCellRect)
                {
                    context.Dungeon.MarkCellAsFloor(cell);
                }

                foreach (IntVec3 cell in room.roomWalls.EdgeCells)
                {
                    context.Dungeon.GridManager.MarkCellProtected(cell, true);
                }

                foreach (var connection in room.connections)
                {
                    foreach (var connectionCell in connection.GetAllCells())
                    {
                        context.Dungeon.GridManager.MarkCellProtected(connectionCell, true);
                    }          
                }
            }
        }

        private void FillMapWithWalls(DungeonGenerationContext context)
        {
            foreach (IntVec3 cell in context.Map.AllCells)
            {
                Thing thing = ThingMaker.MakeThing(
                    context.Def.WallDef,
                    context.Def.WallStuffDef ?? GenStuff.DefaultStuffFor(context.Def.WallDef));
                GenSpawn.Spawn(thing, cell, context.Map);
                context.Map.terrainGrid.SetTerrain(cell, context.Def.TerrainDef);
                context.Map.terrainGrid.SetUnderTerrain(cell, context.Def.TerrainDef);
            }
        }

        private void ClearWalls(DungeonGenerationContext context)
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
