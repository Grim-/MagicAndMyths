using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    //public static class ObstacleGenerator
    //{

    //    static Dictionary<ObstacleDef, int> placedObstacles = new Dictionary<ObstacleDef, int>();
    //    /// <summary>
    //    /// Places obstacles throughout the dungeon after room generation is complete
    //    /// </summary>
    //    public static void GenerateObstacles(Map map, Dungeon Dungeon, List<ObstacleDef> obstacles)
    //    {
    //        //if (Dungeon.nodeToRoomMap.Count <= 1)
    //        //    return;

    //        int obstacleCount = DetermineObstacleCount(Dungeon.nodeToRoomMap.Count);
    //        Log.Message($"Attempting to place {obstacleCount} obstacles in dungeon with {Dungeon.nodeToRoomMap.Count} rooms");

    //        int reattempCount = 0;

    //        for (int i = 0; i < obstacleCount; i++)
    //        {
    //            DungeonRoom dungeonRoom = Dungeon.NormalRooms.RandomElement();

    //            if (dungeonRoom == null || dungeonRoom.def == null)
    //            {
    //                continue;
    //            }

    //            ObstacleDef obstacleDef = SelectObstacleDef(DefDatabase<ObstacleDef>.AllDefsListForReading);

    //            if (TryPlaceObstacle(map, Dungeon, dungeonRoom, obstacleDef))
    //            {
    //                Log.Message($"Successfully placed {obstacleDef.defName} in {dungeonRoom}");
    //            }
    //            else
    //            {
    //                reattempCount++;
    //                //Log.Message($"failed to place {obstacleDef.defName} in {dungeonRoom}");
    //            }
    //        }

    //        for (int i = 0; i < reattempCount; i++)
    //        {
    //            DungeonRoom dungeonRoom = Dungeon.NormalRooms.RandomElement();

    //            if (dungeonRoom == null || dungeonRoom.def == null)
    //            {
    //                continue;
    //            }

    //            ObstacleDef obstacleDef = SelectObstacleDef(DefDatabase<ObstacleDef>.AllDefsListForReading);

    //            if (TryPlaceObstacle(map, Dungeon, dungeonRoom, obstacleDef))
    //            {
    //                Log.Message($"Successfully placed {obstacleDef.defName} in {dungeonRoom}");
    //            }
    //        }

    //        placedObstacles.Clear();
    //    }

    //    /// <summary>
    //    /// Determines how many obstacles to place based on dungeon size
    //    /// </summary>
     
    //}
}
