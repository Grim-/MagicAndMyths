using HarmonyLib;
using Verse;

namespace MagicAndMyths
{
    public static class GrowableStructurePatches
    {

        [HarmonyPatch(typeof(ZoneManager))]
        [HarmonyPatch("Notify_NoZoneOverlapThingSpawned")]
        public static class Patch_ZoneManager_Notify_NoZoneOverlapThingSpawned
        {
            [HarmonyPrefix]
            public static bool Prefix(ZoneManager __instance, Thing thing, ref Zone[] ___zoneGrid)
            {
                CellRect cellRect = thing.OccupiedRect();
                for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
                {
                    for (int j = cellRect.minX; j <= cellRect.maxX; j++)
                    {
                        IntVec3 c = new IntVec3(j, 0, i);
                        Zone zone = __instance.ZoneAt(c);
                        if (zone != null && !(zone is Zone_AreaCapture))
                        {
                            zone.RemoveCell(c);
                            zone.CheckContiguous();
                        }
                    }
                }
                return false;
            }
        }
    }
}
