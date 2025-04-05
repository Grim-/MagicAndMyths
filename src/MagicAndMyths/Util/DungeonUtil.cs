using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public static class DungeonUtil
    {
        public static void SpawnTerrainForRoom(Map map, CellRect roomRect, TerrainDef Terrain)
        {
            foreach (var cell in roomRect.Cells)
            {
                if (cell.InBounds(map))
                {
                    map.terrainGrid.SetTerrain(cell, Terrain);
                    map.terrainGrid.SetUnderTerrain(cell, Terrain);
                }
            }
        }

        public static void SpawnTerrain(Map map, IntVec3 cell, TerrainDef Terrain)
        {
            if (cell.InBounds(map))
            {
                map.terrainGrid.SetTerrain(cell, Terrain);
                map.terrainGrid.SetUnderTerrain(cell, Terrain);
            }
        }

        public static void SpawnDoorsForRoom(Map map, List<BspUtility.BspNode> rooms)
        {
            foreach (var room in rooms)
            {
                foreach (var item in room.roomWalls.EdgeCells)
                {
                    if (item.GetFirstBuilding(map) == null)
                    {
                        IntVec3 left = new IntVec3(item.x - 1, item.y, item.z);
                        IntVec3 right = new IntVec3(item.x + 1, item.y, item.z);

                        IntVec3 top = new IntVec3(item.x, item.y, item.z - 1);
                        IntVec3 bottom = new IntVec3(item.x, item.y, item.z + 1);

                        bool canPlaceHorizontal = IsWall(map, left) && IsWall(map, right);
                        bool canPlaceVertical = IsWall(map, top) && IsWall(map, bottom);

                        if (canPlaceHorizontal || canPlaceVertical)
                        {
                            bool hasValidOpening = false;

                            if (canPlaceHorizontal)
                            {
                                if (IsPassable(map, top) || IsPassable(map, bottom))
                                {
                                    hasValidOpening = true;
                                }
                            }

                            if (canPlaceVertical && !hasValidOpening)
                            {
                                if (IsPassable(map, left) || IsPassable(map, right))
                                {
                                    hasValidOpening = true;
                                }
                            }

                            if (hasValidOpening)
                            {
                                Thing thing = ThingMaker.MakeThing(MagicAndMythDefOf.DungeonHiddenWallDoor, ThingDefOf.Steel);
                                thing = GenSpawn.Spawn(thing, item, map);
                                thing.SetFaction(Faction.OfAncientsHostile);
                            }
                        }
                    }
                }
            }
        }
        public static bool IsPassable(Map map, IntVec3 cell)
        {
            if (!cell.InBounds(map))
                return false;

            Building building = cell.GetFirstBuilding(map);
            if (building == null)
                return true;

            if (building.def != MagicAndMythDefOf.DungeonWall && !building.def.passability.Equals(Traversability.Impassable))
            {
                return true;
            }

            return false;
        }

        public static bool IsWall(Map map, IntVec3 cell)
        {
            if (!cell.InBounds(map))
                return false;

            Building building = cell.GetFirstBuilding(map);
            return building != null && building.def == MagicAndMythDefOf.DungeonWall;
        }

        public static int CountAdjacentOpenSpace(Map map, IntVec3 cell)
        {
            int count = 0;
            foreach (IntVec3 adj in GenAdjFast.AdjacentCellsCardinal(cell))
            {
                if (adj.InBounds(map) && IsPassable(map, adj))
                    count++;
            }
            return count;
        }

        public static bool IsTooCloseToExistingDoors(IntVec3 candidate, List<IntVec3> existingDoors, int minSpacing = 3)
        {
            foreach (IntVec3 existingDoor in existingDoors)
            {
                float distance = (candidate - existingDoor).LengthHorizontal;
                if (distance < minSpacing)
                    return true;
            }

            return false;
        }
    }
}
