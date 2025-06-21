using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class ObstacleRoomDef : RoomTypeDef
    {
        public List<ObstacleDef> obstacles = new List<ObstacleDef>();

        public ObstacleRoomDef()
        {
            this.roomTypeWorker = typeof(ObstacleRoom);
        }
    }

    public class ObstacleRoom : RoomTypeWorker
    {
        ObstacleRoomDef Def => (ObstacleRoomDef)def;
        static Dictionary<ObstacleDef, int> placedObstacles = new Dictionary<ObstacleDef, int>();

        public override void ApplyRoom(DungeonGenerationContext dungeonGenerationContext, DungeonRoom Room)
        {
            base.ApplyRoom(dungeonGenerationContext, Room);
            List<ObstacleDef> defsToUse = Def.obstacles.ToList();

            int obstaclesToPlace = DetermineObstacleCount(dungeonGenerationContext.Dungeon.Rooms.Count);

            for (int i = 0; i < obstaclesToPlace; i++)
            {
                ObstacleDef randomDef = defsToUse.Where(x => CanPlaceObstacle(x)).RandomElement();
                if (randomDef != null)
                {
                    if (!TryPlaceObstacle(dungeonGenerationContext.Map, dungeonGenerationContext.Dungeon, this.currentRoom, randomDef))
                    {
                        defsToUse.Remove(randomDef);
                    }
                }
            }
        }

        private bool CanPlaceObstacle(ObstacleDef obstacleDef)
        {
            if (placedObstacles.ContainsKey(obstacleDef))
            {
                return placedObstacles[obstacleDef] < obstacleDef.maxCount;
            }
            return true;
        }

        private int DetermineObstacleCount(int roomCount)
        {
            int baseCount = Mathf.Max(1, roomCount / 3);

            int variance = Mathf.Max(1, baseCount / 2);
            int finalCount = baseCount + Rand.RangeInclusive(-variance, variance);

            return Mathf.Min(finalCount, roomCount - 1);
        }

        /// <summary>
        /// Attempts to place a specific obstacle in the dungeon
        /// </summary>
        public bool TryPlaceObstacle(Map map, Dungeon Dungeon, DungeonRoom Room, ObstacleDef obstacleDef)
        {
            try
            {
                if (obstacleDef.DoWorker(map, Dungeon, Room))
                {
                    if (!placedObstacles.ContainsKey(obstacleDef))
                    {
                        placedObstacles.Add(obstacleDef, 1);
                    }
                    else
                    {
                        placedObstacles[obstacleDef] += 1;
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"Error placing obstacle {obstacleDef.defName}: {ex}");
                return false;
            }
        }
    }
}
