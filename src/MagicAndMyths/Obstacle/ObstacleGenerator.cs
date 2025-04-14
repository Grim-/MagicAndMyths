using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public static class ObstacleGenerator
    {

        static Dictionary<ObstacleDef, int> placedObstacles = new Dictionary<ObstacleDef, int>();
        /// <summary>
        /// Places obstacles throughout the dungeon after room generation is complete
        /// </summary>
        public static void GenerateObstacles(Map map, Dungeon Dungeon)
        {
            //if (Dungeon.nodeToRoomMap.Count <= 1)
            //    return;

            int obstacleCount = DetermineObstacleCount(Dungeon.nodeToRoomMap.Count);
            Log.Message($"Attempting to place {obstacleCount} obstacles in dungeon with {Dungeon.nodeToRoomMap.Count} rooms");

            int reattempCount = 0;

            for (int i = 0; i < obstacleCount; i++)
            {
                DungeonRoom dungeonRoom = Dungeon.Rooms.Where(x=> x.def.roomType != RoomType.Start && x.def.roomType != RoomType.End).ToList().RandomElement();

                if (dungeonRoom == null || dungeonRoom.def == null)
                {
                    continue;
                }

                ObstacleDef obstacleDef = SelectObstacleDef(DefDatabase<ObstacleDef>.AllDefsListForReading);

                if (TryPlaceObstacle(map, Dungeon, dungeonRoom, obstacleDef))
                {
                    Log.Message($"Successfully placed {obstacleDef.defName} in {dungeonRoom}");
                }
                else
                {
                    reattempCount++;
                    //Log.Message($"failed to place {obstacleDef.defName} in {dungeonRoom}");
                }
            }

            for (int i = 0; i < reattempCount; i++)
            {
                DungeonRoom dungeonRoom = Dungeon.Rooms.Where(x => x.def.roomType != RoomType.Start && x.def.roomType != RoomType.End).ToList().RandomElement();

                if (dungeonRoom == null || dungeonRoom.def == null)
                {
                    continue;
                }

                ObstacleDef obstacleDef = SelectObstacleDef(DefDatabase<ObstacleDef>.AllDefsListForReading);

                if (TryPlaceObstacle(map, Dungeon, dungeonRoom, obstacleDef))
                {
                    Log.Message($"Successfully placed {obstacleDef.defName} in {dungeonRoom}");
                }
            }

            placedObstacles.Clear();
        }

        /// <summary>
        /// Determines how many obstacles to place based on dungeon size
        /// </summary>
        private static int DetermineObstacleCount(int roomCount)
        {
            int baseCount = Mathf.Max(1, roomCount / 3);

            int variance = Mathf.Max(1, baseCount / 2);
            int finalCount = baseCount + Rand.RangeInclusive(-variance, variance);

            return Mathf.Min(finalCount, roomCount - 1);
        }

        /// <summary>
        /// Attempts to place a specific obstacle in the dungeon
        /// </summary>
        private static bool TryPlaceObstacle(Map map, Dungeon Dungeon, DungeonRoom Room, ObstacleDef obstacleDef)
        {
            try
            {
                return obstacleDef.DoWorker(map, Dungeon, Room);
            }
            catch (Exception ex)
            {
                Log.Error($"Error placing obstacle {obstacleDef.defName}: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Selects an obstacle def based on weighted probability
        /// </summary>
        private static ObstacleDef SelectObstacleDef(List<ObstacleDef> availableObstacles)
        {
            if (availableObstacles.Count == 0)
                return null;
            return availableObstacles.RandomElementByWeight(x => x.commonality);
        }
    }
}
