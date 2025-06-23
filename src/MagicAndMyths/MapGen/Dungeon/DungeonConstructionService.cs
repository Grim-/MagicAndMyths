using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class DungeonConstructionService
    {
        private readonly DungeonGenerationContext context;

        public DungeonConstructionService(DungeonGenerationContext context)
        {
            this.context = context;
        }

        public void PlaceWall(IntVec3 cell, bool indestructible = false)
        {
            if (!cell.InBounds(context.Map))
                return;

            ClearCell(cell);

            ThingDef wallDef = indestructible ? context.Def.IndestructibleWallDef : context.Def.WallDef;
            ThingDef stuffDef = context.Def.WallStuffDef ?? GenStuff.DefaultStuffFor(wallDef);

            Thing wall = ThingMaker.MakeThing(wallDef, stuffDef);
            GenSpawn.Spawn(wall, cell, context.Map);
        }

        public void PlaceWalls(IEnumerable<IntVec3> cells, bool indestructible = false)
        {
            foreach (var cell in cells)
            {
                PlaceWall(cell, indestructible);
            }
        }
        public bool IsValidDoorPosition(IntVec3 cell, DungeonRoom room)
        {
            if (!cell.InBounds(this.context.Map))
                return false;


            return true;
            if (this.context.Dungeon.IsCellFloor(cell))
                return false;

            int roomConnections = 0;
            int corridorConnections = 0;
            int wallConnections = 0;

            foreach (var dir in GenAdj.CardinalDirections)
            {
                IntVec3 neighbor = cell + dir;
                if (!neighbor.InBounds(this.context.Map))
                    continue;

                if (room.roomCells.Contains(neighbor))
                {
                    roomConnections++;
                }
                else if (this.context.Dungeon.IsPathCell(neighbor))
                {
                    corridorConnections++;
                }
                else if (!this.context.Dungeon.IsCellFloor(neighbor))
                {
                    wallConnections++;
                }
            }

            return roomConnections >= 1 && (corridorConnections >= 1 || wallConnections >= 2);
        }

        public void PlaceDoor(IntVec3 cell, Rot4 rotation, ThingDef doorDef = null, ThingDef stuffDef = null)
        {
            if (!cell.InBounds(context.Map))
                return;

            ClearCell(cell);

            doorDef = doorDef ?? ThingDefOf.Door;
            stuffDef = stuffDef ?? GenStuff.DefaultStuffFor(doorDef);

            Thing door = ThingMaker.MakeThing(doorDef, stuffDef);
            GenSpawn.Spawn(door, cell, context.Map, rotation);
        }

        public Thing PlaceDoubleDoor(IntVec3 cell, Rot4 rotation, ThingDef doorDef = null, ThingDef stuffDef = null)
        {
            if (!cell.InBounds(context.Map))
                return null;

            ClearCell(cell);

            doorDef = doorDef ?? ThingDefOf.SecurityDoor;
            stuffDef = stuffDef ?? GenStuff.DefaultStuffFor(doorDef);

            Thing door = ThingMaker.MakeThing(doorDef, stuffDef);
            GenSpawn.Spawn(door, cell, context.Map, rotation);

            return door;
        }

        public void SetTerrain(IntVec3 cell, TerrainDef terrain = null)
        {
            if (!cell.InBounds(context.Map))
                return;

            terrain = terrain ?? context.Def.TerrainDef;
            context.Map.terrainGrid.SetTerrain(cell, terrain);
            context.Map.terrainGrid.SetUnderTerrain(cell, terrain);
        }

        public void SetTerrain(IEnumerable<IntVec3> cells, TerrainDef terrain = null)
        {
            foreach (var cell in cells)
            {
                SetTerrain(cell, terrain);
            }
        }

        public void ClearCell(IntVec3 cell)
        {
            if (!cell.InBounds(context.Map))
                return;

            context.Map.thingGrid.ThingsAt(cell)
                .ToList()
                .ForEach(t => t.Destroy());
        }

        public void ClearCells(IEnumerable<IntVec3> cells)
        {
            foreach (var cell in cells)
            {
                ClearCell(cell);
            }
        }
        public void BuildWallsToEdge(IntVec3 startCell, IntVec3 direction, HashSet<IntVec3> validCells, ThingDef thingToBuild = null)
        {
            IntVec3 currentCell = startCell;
            while (validCells.Contains(currentCell) && currentCell.InBounds(context.Map))
            {
                bool hasWall = false;
                var things = context.Map.thingGrid.ThingsAt(currentCell);
                foreach (var thing in things)
                {
                    if (thing.def.category == ThingCategory.Building && thing.def.passability == Traversability.Impassable)
                    {
                        hasWall = true;
                        break;
                    }
                }

                if (!hasWall)
                {
                    context.Dungeon.GridManager.MarkCellAsWall(currentCell);


                    if (thingToBuild != null)
                    {
                        ThingDef stuffDef = context.Def.WallStuffDef;
                        PlaceThing(currentCell, thingToBuild, stuffDef);
                    }
                    else
                    {
                        PlaceWall(currentCell);
                    }
                   
                }

                currentCell += direction;
            }
        }
        public void CreateFloorArea(IEnumerable<IntVec3> cells)
        {
            ClearCells(cells);
            SetTerrain(cells);

            foreach (var cell in cells)
            {
                context.Dungeon.MarkCellAsFloor(cell);
            }
        }

        public void FillMapWithWalls()
        {
            foreach (IntVec3 cell in context.Map.AllCells)
            {
                PlaceWall(cell);
                SetTerrain(cell);
            }
        }

        public void PlaceThing(IntVec3 cell, ThingDef thingDef, ThingDef stuffDef = null)
        {
            if (!cell.InBounds(context.Map))
                return;

            stuffDef = stuffDef ?? GenStuff.DefaultStuffFor(thingDef);
            Thing thing = ThingMaker.MakeThing(thingDef, stuffDef);
            GenSpawn.Spawn(thing, cell, context.Map);
        }

        public void PlaceThings(IEnumerable<IntVec3> cells, ThingDef thingDef, ThingDef stuffDef = null)
        {
            foreach (var cell in cells)
            {
                PlaceThing(cell, thingDef, stuffDef);
            }
        }

        public bool CanPlaceAt(IntVec3 cell)
        {
            return cell.InBounds(context.Map) &&
                   !context.Map.thingGrid.ThingsAt(cell).Any(t => t.def.category == ThingCategory.Building);
        }

        public List<IntVec3> GetValidPlacementCells(CellRect area)
        {
            return area.Cells.Where(CanPlaceAt).ToList();
        }
    }
}