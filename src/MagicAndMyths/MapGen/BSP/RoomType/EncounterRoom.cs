using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace MagicAndMyths
{

    public class EncounterRoomDef_Fight : RoomTypeDef
    {
        public IntRange enemyCountRange = new IntRange(1, 4);
        public List<PawnKindDef> possibleEnemies;

        public EncounterRoomDef_Fight()
        {
            roomTypeWorker = typeof(EncounterRoom_Fight);
        }
    }

    public class EncounterRoom_Fight : RoomTypeWorker
    {
        EncounterRoomDef_Fight Def => (EncounterRoomDef_Fight)def;

        public override void ApplyRoom(Map map, DungeonRoom Room)
        {
            TerrainDef terrainDef = TerrainDefOf.Ice;
            DungeonUtil.SpawnTerrainForRoom(map, Room.roomCellRect, terrainDef);

            Generate(map, Def.enemyCountRange.RandomInRange, Room.roomCellRect, Def.possibleEnemies, Faction.OfAncientsHostile);
        }


        public void Generate(Map map, int numEnemies, CellRect roomRect, List<PawnKindDef> possibleEnemies, Faction faction)
        {
            //List<Pawn> spawn = new List<Pawn>();

            //for (int i = 0; i < numEnemies; i++)
            //{
            //    PawnKindDef enemyKind = possibleEnemies.RandomElement();
            //    Pawn enemy = PawnGenerator.GeneratePawn(enemyKind, faction);
            //    GenSpawn.Spawn(enemy, roomRect.Cells.RandomElement(), map);
            //    spawn.Add(enemy);
            //}
            //LordJob_DefendBase lordJob = new LordJob_DefendBase(faction, roomRect.CenterCell);
            //Lord enemyLord = LordMaker.MakeNewLord(faction, lordJob, map, spawn);
            //map.GetComponent<MapComponent_DungeonEnemies>().AddLord(map.uniqueID, enemyLord);
        }
    }
}
