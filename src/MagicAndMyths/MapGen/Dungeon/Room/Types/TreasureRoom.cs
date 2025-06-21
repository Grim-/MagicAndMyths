using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class TreasureRoomDef : RoomTypeDef
    {
        public List<TreasureDrop> potentialTreasure = new List<TreasureDrop>();
        public int rollsPerTier = 5;
        public TreasureRoomDef()
        {
            this.roomTypeWorker = typeof(TreasureRoom);
        }
    }

    public class TreasureRoom : RoomTypeWorker
    {
        TreasureRoomDef Def => (TreasureRoomDef)def;


        public override bool CanApply(DungeonGenerationContext dungeonGenerationContext, DungeonRoom DungeonRoom)
        {

            if (DungeonRoom.HasAnyForwardConnections())
            {
                Log.Message($"Cannot apply {this.Def.defName} to room, can only be applied to leaf nodes");
                return false;
            }

            return base.CanApply(dungeonGenerationContext, DungeonRoom);
        }


        public override void ApplyRoom(DungeonGenerationContext dungeonGenerationContext, DungeonRoom Room)
        {
            base.ApplyRoom(dungeonGenerationContext, Room);

            if (!Def.potentialTreasure.Any())
            {
                return;
            }


            for (int i = 0; i < Def.rollsPerTier; i++)
            {
                TreasureDrop treasureDrop = Def.potentialTreasure.RandomElement();

                if (treasureDrop != null)
                {
                    Thing thing = ThingMaker.MakeThing(treasureDrop.thingDef, treasureDrop.thingStuffDef);
                    thing.stackCount = treasureDrop.count.RandomInRange;
                    if (GenPlace.TryPlaceThing(thing, Room.RoomCellRect.CenterCell, dungeonGenerationContext.Map, ThingPlaceMode.Direct))
                    {

                    }
                }
            }

     
        }
    }

    public class TreasureDrop
    {
        public ThingDef thingDef;
        public ThingDef thingStuffDef;
        public IntRange count = new IntRange(1, 1);
        public FloatRange chance = new FloatRange(0.2f, 0.2f);
        public float minProgression = 0f;
    }
}
