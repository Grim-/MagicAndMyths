using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace MagicAndMyths
{
    public class EncounterPawn
    {
        public PawnKindDef kindDef;
        public IntRange maxAmount = new IntRange(1, 1);
        public float spawnChance = 1f;
        public bool isRequired = true;
    }

    public class SpecificEncounter
    {
        public List<EncounterPawn> enemies = new List<EncounterPawn>();
        public FloatRange progressionRange = new FloatRange(0f, 1f);
        public int weight = 1;
    }

    public class EncounterRoomDef_Fight : RoomTypeDef
    {
        public IntRange enemyCountRange = new IntRange(1, 4);
        public List<PawnKindDef> possibleEnemies;
        public List<SpecificEncounter> specificEncounters = new List<SpecificEncounter>();
        public bool useProgressionScaling = true;

        public EncounterRoomDef_Fight()
        {
            roomTypeWorker = typeof(EncounterRoom_Fight);
        }
    }

    public class EncounterRoom_Fight : RoomTypeWorker
    {
        EncounterRoomDef_Fight Def => (EncounterRoomDef_Fight)def;

        public override void ApplyRoom(Map map, Dungeon dungeon, DungeonRoom room)
        {
            base.ApplyRoom(map, dungeon, room);

            var validEncounters = Def.specificEncounters
                .Where(e => room.ProgressionValue >= e.progressionRange.min &&
                           room.ProgressionValue <= e.progressionRange.max)
                .ToList();

            if (validEncounters.Count > 0)
            {
                var encounter = validEncounters.RandomElementByWeight(e => e.weight);
                GenerateSpecificEncounter(map, room.roomCellRect, encounter, room.ProgressionValue);
            }
            else
            {
                Generate(map, Def.enemyCountRange.RandomInRange, room.roomCellRect, Def.possibleEnemies, Faction.OfAncientsHostile, room.ProgressionValue);
            }
        }

        private void GenerateSpecificEncounter(Map map, CellRect roomRect, SpecificEncounter encounter, float progression)
        {
            List<Pawn> spawn = new List<Pawn>();

            foreach (var enemy in encounter.enemies)
            {
                int count = enemy.maxAmount.RandomInRange;
                if (Def.useProgressionScaling)
                {
                    count = Mathf.RoundToInt(count * (1f + progression * 0.5f));
                }

                for (int i = 0; i < count; i++)
                {
                    Pawn pawn = PawnGenerator.GeneratePawn(enemy.kindDef, Faction.OfAncientsHostile);
                    GenSpawn.Spawn(pawn, roomRect.Cells.RandomElement(), map);
                    spawn.Add(pawn);
                }
            }

            LordJob_DefendPoint lordJob = new LordJob_DefendPoint(roomRect.CenterCell, 0, 1, false, false);
            Lord enemyLord = LordMaker.MakeNewLord(Faction.OfAncientsHostile, lordJob, map, spawn);
            map.GetComponent<MapComp_DungeonEnemies>().AddLord(map.uniqueID, enemyLord);
        }

        public void Generate(Map map, int numEnemies, CellRect roomRect, List<PawnKindDef> possibleEnemies, Faction faction, float progression)
        {
            if (Def.useProgressionScaling)
            {
                numEnemies = Mathf.RoundToInt(numEnemies * (1f + progression * 0.5f));
            }

            numEnemies = Mathf.Max(1, numEnemies);

            List<Pawn> spawn = new List<Pawn>();
            for (int i = 0; i < numEnemies; i++)
            {
                PawnKindDef enemyKind = possibleEnemies.RandomElement();
                Pawn enemy = PawnGenerator.GeneratePawn(enemyKind, faction);
                GenSpawn.Spawn(enemy, roomRect.Cells.RandomElement(), map);
                spawn.Add(enemy);
            }

            LordJob_DefendPoint lordJob = new LordJob_DefendPoint(roomRect.CenterCell, 0, 1, false, false);
            Lord enemyLord = LordMaker.MakeNewLord(faction, lordJob, map, spawn);
            map.GetComponent<MapComp_DungeonEnemies>().AddLord(map.uniqueID, enemyLord);
        }
    }
}
