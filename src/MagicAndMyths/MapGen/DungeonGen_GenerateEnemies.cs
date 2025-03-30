using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace MagicAndMyths
{
    public class DungeonGen_GenerateEnemies : DungeonGen
    {
        private readonly Faction enemyFaction;
        private readonly List<PawnKindDef> possibleEnemies;
        private readonly IntRange enemyCount;

        public override int Priority => 50;

        public DungeonGen_GenerateEnemies(Map map, Faction enemyFaction = null, IntRange? enemyCount = null) : base(map)
        {
            this.enemyFaction = enemyFaction ?? Faction.OfPirates;
            this.enemyCount = enemyCount ?? new IntRange(3, 8);
            this.possibleEnemies = new List<PawnKindDef>
            {
                PawnKindDefOf.Pirate,
            };
        }

        public override void Generate()
        {
            int numEnemies = enemyCount.RandomInRange;
            List<IntVec3> spawnPoints = FindEnemySpawnPoints(numEnemies);
            List<Pawn> spawn = new List<Pawn>();

            foreach (IntVec3 spawnPoint in spawnPoints)
            {
                PawnKindDef enemyKind = possibleEnemies.RandomElement();
                Pawn enemy = PawnGenerator.GeneratePawn(enemyKind, enemyFaction);
                GenSpawn.Spawn(enemy, spawnPoint, map);
                spawn.Add(enemy);
            }

            LordJob_DefendBase lordJob = new LordJob_DefendBase(enemyFaction, map.Center);
            Lord enemyLord = LordMaker.MakeNewLord(enemyFaction, lordJob, map, spawn);
            map.GetComponent<MapComponent_DungeonEnemies>().AddLord(map.uniqueID, enemyLord);
        }

        private List<IntVec3> FindEnemySpawnPoints(int count)
        {
            List<IntVec3> result = new List<IntVec3>();
            int attempts = 0;
            int maxAttempts = count * 3;

            while (result.Count < count && attempts < maxAttempts)
            {
                IntVec3 cell = CellFinder.RandomCell(map);
                if (IsValidEnemySpawnPoint(cell))
                {
                    result.Add(cell);
                }
                attempts++;
            }

            return result;
        }

        private bool IsValidEnemySpawnPoint(IntVec3 cell)
        {
            return cell.Standable(map);
        }
    }
}

