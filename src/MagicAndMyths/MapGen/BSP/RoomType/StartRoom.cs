﻿using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class StartRoom : RoomTypeWorker
    {
        public override void ApplyRoom(Map map, DungeonRoom Room)
        {
            foreach (var item in Room.roomCellRect)
            {
                map.terrainGrid.SetTerrain(item, TerrainDefOf.MetalTile);
                map.terrainGrid.SetUnderTerrain(item, TerrainDefOf.MetalTile);
            }

            if (MagicAndMythDefOf.MagicAndMyths_ReturnPortal != null)
            {
                Building_ReturnPortal returnPortal = (Building_ReturnPortal)ThingMaker.MakeThing(MagicAndMythDefOf.MagicAndMyths_ReturnPortal);
                GenSpawn.Spawn(returnPortal, Room.roomCellRect.RandomCell, map);
            }
        }
    }
}
