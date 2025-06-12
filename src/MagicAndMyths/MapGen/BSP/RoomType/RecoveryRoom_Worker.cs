using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class RecoveryRoomDef : RoomTypeDef
    {
        public List<ThingDef> healingItems = new List<ThingDef>();
        public int bedCount = 2;
        public bool addFood = true;
        public bool addMedicine = true;

        public RecoveryRoomDef()
        {
            roomTypeWorker = typeof(RecoveryRoom_Worker);
        }
    }

    public class RecoveryRoom_Worker : RoomTypeWorker
    {
        RecoveryRoomDef Def => (RecoveryRoomDef)def;

        public override void ApplyRoom(Map map, Dungeon dungeon, DungeonRoom room)
        {
            base.ApplyRoom(map, dungeon, room);

            var roomRect = room.roomCellRect;
            var innerRect = roomRect.ContractedBy(1);

            SpawnBeds(map, room, innerRect);
            SpawnShelves(map, room, roomRect, innerRect);
        }

        private void SpawnBeds(Map map, DungeonRoom room, CellRect innerRect)
        {
            var bedSize = ThingDefOf.Bed.Size;

            for (int i = 0; i < Def.bedCount; i++)
            {
                CellRect bedRect;
                if (innerRect.TryFindRandomInnerRect(bedSize, out bedRect, (x) => room.CanBuildHere(x)))
                {
                    Thing bed = ThingMaker.MakeThing(ThingDefOf.Bed, GenStuff.DefaultStuffFor(ThingDefOf.Bed));
                    GenSpawn.Spawn(bed, bedRect.CenterCell, map);

                    innerRect = innerRect.ContractedBy(1);
                }
            }
        }
        private void SpawnShelves(Map map, DungeonRoom room, CellRect roomRect, CellRect innerRect)
        {
            var shelfCells = roomRect.Cells.Where(c => !innerRect.Contains(c) && c.Standable(map) && room.CanBuildHere(c)).ToList();

            foreach (var cell in shelfCells)
            {
                if (Rand.Value < 0.3f)
                {
                    Thing shelf = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("Shelf"), GenStuff.DefaultStuffFor(DefDatabase<ThingDef>.GetNamed("Shelf"))); ;
                    GenSpawn.Spawn(shelf, cell, map);

                    SpawnItemsOnShelf(map, cell);
                }
            }
        }

        private void SpawnItemsOnShelf(Map map, IntVec3 shelfCell)
        {
            if (Def.addMedicine && Rand.Value < 0.5f)
            {
                Thing medicine = ThingMaker.MakeThing(ThingDefOf.MedicineIndustrial);
                medicine.stackCount = Rand.Range(3, 8);
                GenPlace.TryPlaceThing(medicine, shelfCell, map, ThingPlaceMode.Direct);
            }

            if (Def.addFood && Rand.Value < 0.5f)
            {
                Thing food = ThingMaker.MakeThing(ThingDefOf.MealSurvivalPack);
                food.stackCount = Rand.Range(5, 12);
                GenPlace.TryPlaceThing(food, shelfCell, map, ThingPlaceMode.Direct);
            }

            if (Def.healingItems.Count > 0 && Rand.Value < 0.3f)
            {
                var itemDef = Def.healingItems.RandomElement();
                Thing item = ThingMaker.MakeThing(itemDef);
                GenPlace.TryPlaceThing(item, shelfCell, map, ThingPlaceMode.Direct);
            }
        }
    }
}
