using RimWorld.Planet;
using Verse;

namespace MagicAndMyths
{
    public static class PortalUtils
    {
        public static bool TransferToMap(this Thing thing, IntVec3 spawnLocation, Map newMap, bool unfog = true, int searchRadius = 5)
        {
            if (thing == null || newMap == null || !spawnLocation.InBounds(newMap))
            {
                Log.Warning($"Invalid transfer attempt to map");
                return false;
            }

            if (!spawnLocation.Walkable(newMap))
            {
                spawnLocation = CellFinder.StandableCellNear(spawnLocation, newMap, searchRadius);
                if (!spawnLocation.IsValid)
                {
                    Log.Warning($"Could not find valid position near target");
                    return false;
                }
            }

            if (thing.Spawned)
            {
                thing.DeSpawn(DestroyMode.Vanish);
            }

            GenSpawn.Spawn(thing, spawnLocation, newMap);
            if (unfog) newMap.fogGrid.Unfog(spawnLocation);
            return true;
        }


        public static IntVec3 FindTeleportLocation(Pawn pawn, Map map)
        {

            for (int i = 0; i < 10; i++)
            {
                IntVec3 testLoc = CellFinderLoose.TryFindCentralCell(map, 10, 1);
                if (TeleportLocationValidator(pawn, map, testLoc))
                {
                    return testLoc;
                }
            }
            return CellFinder.RandomSpawnCellForPawnNear(map.Center, map);
        }

        public static bool TeleportLocationValidator(Pawn pawn, Map map, IntVec3 position)
        {
            return position.Standable(map) &&
                   !position.Filled(map) &&
                   position.GetDangerFor(pawn, map) != Danger.Deadly;
        }

        public static GateAddress GetGateAddress(this Settlement settlement)
        {
            return GateAddress.GetAddressForSettlement(settlement);
        }
    }
}
