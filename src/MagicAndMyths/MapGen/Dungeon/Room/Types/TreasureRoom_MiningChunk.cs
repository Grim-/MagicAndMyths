using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class TreasureRoomMiningChunkDef : RoomTypeDef
    {
        public IntRange chunkSize = new IntRange(2, 6);
        public List<ThingDef> mineables = new List<ThingDef>();

        public TreasureRoomMiningChunkDef()
        {

            roomTypeWorker = typeof(TreasureRoom_MiningChunk);
        }
    }

    public class TreasureRoom_MiningChunk : RoomTypeWorker
    {

        TreasureRoomMiningChunkDef Def => (TreasureRoomMiningChunkDef)def;

        public override void ApplyRoom(Map map, Dungeon Dungeon, DungeonRoom Room)
        {
            base.ApplyRoom(map, Dungeon, Room);

            if (Room.RoomCellRect.TryFindRandomInnerRect(new IntVec2(Def.chunkSize.RandomInRange, Def.chunkSize.RandomInRange), out CellRect newCellRect, null))
            {
                int lootTier = Room.DifficultyTier;
                float progressionValue = Room.ProgressionValue;
                ThingDef possibleLoot = GetLootByTier(lootTier);

                foreach (var item in newCellRect.Cells)
                {
                    GenSpawn.Spawn(possibleLoot, item, map);
                }
            }
        }




        private ThingDef GetLootByTier(int tier)
        {
            return Def.mineables.RandomElementByWeight(x => x.generateCommonality * Mathf.Max(1, tier));
        }
    }
}
