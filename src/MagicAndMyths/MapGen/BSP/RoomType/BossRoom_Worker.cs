using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace MagicAndMyths
{
    public class BossRoomDef : RoomTypeDef
    {
        public List<SpecificEncounter> bossEncounters = new List<SpecificEncounter>();

        public BossRoomDef()
        {
            roomTypeWorker = typeof(BossRoom_Worker);
        }
    }

    public class BossRoom_Worker : RoomTypeWorker
    {
        BossRoomDef Def => (BossRoomDef)def;

        public override void ApplyRoom(Map map, Dungeon dungeon, DungeonRoom room)
        {
            base.ApplyRoom(map, dungeon, room);

            var validBosses = Def.bossEncounters
                .Where(e => room.ProgressionValue >= e.progressionRange.min &&
                           room.ProgressionValue <= e.progressionRange.max)
                .ToList();

            if (validBosses.Count > 0)
            {
                var bossEncounter = validBosses.RandomElementByWeight(e => e.weight);
                GenerateBossEncounter(map, room.roomCellRect, bossEncounter, room.ProgressionValue);
            }
        }

        private void GenerateBossEncounter(Map map, CellRect roomRect, SpecificEncounter encounter, float progression)
        {
            List<Pawn> spawn = new List<Pawn>();

            foreach (var enemy in encounter.enemies)
            {
                int count = enemy.maxAmount.RandomInRange;
                count = Mathf.RoundToInt(count * (1f + progression));

                for (int i = 0; i < count; i++)
                {
                    Pawn pawn = PawnGenerator.GeneratePawn(enemy.kindDef, Faction.OfAncientsHostile);
                    GenSpawn.Spawn(pawn, roomRect.Cells.RandomElement(), map);
                    spawn.Add(pawn);
                }
            }

            LordJob_DefendPoint lordJob = new LordJob_DefendPoint(roomRect.CenterCell, 0, 1, false, false);
            Lord bossLord = LordMaker.MakeNewLord(Faction.OfAncientsHostile, lordJob, map, spawn);
            map.GetComponent<MapComp_DungeonEnemies>().AddLord(map.uniqueID, bossLord);
        }
    }
}
