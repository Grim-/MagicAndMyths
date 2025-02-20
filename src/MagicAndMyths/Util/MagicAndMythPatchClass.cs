using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    [StaticConstructorOnStartup]
    public static class MagicAndMythPatchClass
    {
        static MagicAndMythPatchClass()
        {
            var harmony = new Harmony("com.emo.magicandmyths");
            harmony.PatchAll();
        }


        [HarmonyPatch(typeof(Pawn_EquipmentTracker), "EquipmentTrackerTick")]
        public class CompEquippable_BowModeSwitcher_Tick_Patch
        {
            static void Postfix(Pawn_EquipmentTracker __instance)
            {
                foreach (var eq in __instance.AllEquipmentListForReading)
                {
                    eq.GetComp<CompEquippable_BowModeSwitcher>()?.EquipTick();
                }
            }
        }

        [HarmonyPatch(typeof(Pawn_EquipmentTracker), "GetGizmos")]
        public class EquipmentTracker_GetGizmos_Patch
        {
            static void Postfix(Pawn_EquipmentTracker __instance, ref IEnumerable<Gizmo> __result)
            {
                var originalGizmos = __result.ToList();

                var additionalGizmos = new List<Gizmo>();
                foreach (var eq in __instance.AllEquipmentListForReading)
                {
                    if (eq is IDrawEquippedGizmos equippedGizmos)
                    {
                        additionalGizmos.AddRange(equippedGizmos.GetEquippedGizmos());
                    }

                    foreach (var item in eq.AllComps)
                    {
                        if (item is IDrawEquippedGizmos compEquippedGizmos)
                        {
                            additionalGizmos.AddRange(compEquippedGizmos.GetEquippedGizmos());
                        }
                    }

                }

                __result = originalGizmos.Concat(additionalGizmos);
            }
        }

        public class Patch_EquipmentUtility_CanEquip
        {
            public static bool Prefix(Thing thing, Pawn pawn, ref string cantReason, bool checkBonded, ref bool __result)
            {
                CompSelectiveBiocodable compSelective = thing.TryGetComp<CompSelectiveBiocodable>();

                if (compSelective != null)
                {
                    if (!compSelective.CanBeBiocodedFor(pawn))
                    {
                        cantReason = "You cannot equip this, dont meet requirements";
                        __result = false;
                        return false;
                    }
                }

                return true;
            }
        }
    }

    public class Patch_EquipmentUtility_CanEquip
    {
        public static bool Prefix(Thing thing, Pawn pawn, ref string cantReason, bool checkBonded, ref bool __result)
        {
            CompSelectiveBiocodable compSelective = thing.TryGetComp<CompSelectiveBiocodable>();

            if (compSelective != null)
            {
                if (!compSelective.CanBeBiocodedFor(pawn))
                {
                    cantReason = "You cannot equip this, dont meet requirements";
                    __result = false;
                    return false;
                }
            }

            return true;
        }
    }
}
