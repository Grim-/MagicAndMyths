using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace MagicAndMyths
{
    public class MapComponent_DungeonEnemies : MapComponent
    {
        private Dictionary<int, List<Lord>> enemyLords;
        private List<int> tempKeys;
        private List<List<Lord>> tempValues;

        public MapComponent_DungeonEnemies(Map map) : base(map)
        {
            enemyLords = new Dictionary<int, List<Lord>>();
        }

        public void AddLord(int mapId, Lord lord)
        {
            if (!enemyLords.ContainsKey(mapId))
            {
                enemyLords[mapId] = new List<Lord>();
            }
            enemyLords[mapId].Add(lord);
        }

        public List<Lord> GetLords(int mapId)
        {
            return enemyLords.TryGetValue(mapId, out var lords) ? lords : new List<Lord>();
        }

        public void RemoveLord(int mapId, Lord lord)
        {
            if (enemyLords.ContainsKey(mapId))
            {
                enemyLords[mapId].Remove(lord);
                if (enemyLords[mapId].Count == 0)
                {
                    enemyLords.Remove(mapId);
                }
            }
        }

        public void ClearLords(int mapId)
        {
            if (enemyLords.ContainsKey(mapId))
            {
                enemyLords.Remove(mapId);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(
                ref enemyLords,
                "enemyLords",
                LookMode.Value,
                LookMode.Reference,
                ref tempKeys,
                ref tempValues
            );

            if (enemyLords == null)
            {
                enemyLords = new Dictionary<int, List<Lord>>();
            }
        }
    }
}

