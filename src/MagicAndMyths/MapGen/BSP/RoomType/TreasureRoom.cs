using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class TreasureRoomDef : RoomTypeDef
    {
        public List<TreasureDrop> potentialTreasure = new List<TreasureDrop>();

        public TreasureRoomDef()
        {
            this.roomTypeWorker = typeof(TreasureRoom);
        }
    }

    public class TreasureRoom : RoomTypeWorker
    {
        TreasureRoomDef Def => (TreasureRoomDef)def;

        public override void ApplyRoom(Map map, Dungeon Dungeon, DungeonRoom Room)
        {
            base.ApplyRoom(map, Dungeon, Room);

            if (!Def.potentialTreasure.Any())
            {
                return;
            }

            TreasureDrop treasureDrop = Def.potentialTreasure.RandomElement();

            if (treasureDrop != null)
            {
                Thing thing = ThingMaker.MakeThing(treasureDrop.thingDef, treasureDrop.thingStuffDef);
                thing.stackCount = treasureDrop.count.RandomInRange;
                if (GenPlace.TryPlaceThing(thing, Room.roomCellRect.CenterCell, map, ThingPlaceMode.Direct))
                {

                }
            }      
        }
    }

    public class TreasureDrop
    {
        public ThingDef thingDef;
        public ThingDef thingStuffDef;
        public IntRange count = new IntRange(1, 1);
    }
}
