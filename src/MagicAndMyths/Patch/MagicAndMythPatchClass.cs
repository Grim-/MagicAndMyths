using HarmonyLib;
using RimWorld;
using RimWorld.Utility;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    [StaticConstructorOnStartup]
    public static partial class MagicAndMythPatchClass
    {
        static MagicAndMythPatchClass()
        {
            var harmony = new Harmony("com.emo.magicandmyths");
            harmony.PatchAll();
        }



        [HarmonyPatch(typeof(ReloadableUtility), "FindSomeReloadableComponent")]
        public class ReloadableUtility_Patch
        {
            static void Postfix(Pawn pawn, bool allowForcedReload, ref IReloadableComp __result)
            {
                //dont know why this would eve rbe null, whatever
                if (pawn.health != null)
                {
                    foreach (var item in pawn.health.hediffSet.hediffs)
                    {
                        if (item is IReloadableComp reloadableComp)
                        {
                            if (reloadableComp != null && reloadableComp.NeedsReload(allowForcedReload))
                            {
                                __result = reloadableComp;
                                break;
                            }

                        }

                        if (item is HediffWithComps withComps)
                        {
                            foreach (var comp in withComps.comps)
                            {
                                if (comp is IReloadableComp abilitycompreloadableComp)
                                {
                                    if (abilitycompreloadableComp != null && abilitycompreloadableComp.NeedsReload(allowForcedReload))
                                    {
                                        __result = abilitycompreloadableComp;
                                        break;
                                    }

                                }

                            }
                        }

                    }
                }

                if (pawn.abilities != null)
                {
                    foreach (var item in pawn.abilities.abilities)
                    {
                        if (item is IReloadableComp reloadableComp)
                        {
                            if (reloadableComp != null  && reloadableComp.NeedsReload(allowForcedReload))
                            {
                                __result = reloadableComp;
                                break;
                            }
                 
                        }

                        foreach (var comp in item.comps)
                        {
                            if (comp is IReloadableComp abilitycompreloadableComp)
                            {
                                if (abilitycompreloadableComp != null && abilitycompreloadableComp.NeedsReload(allowForcedReload))
                                {
                                    __result = abilitycompreloadableComp;
                                    break;
                                }

                            }

                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PawnRenderer), "CurRotDrawMode",  MethodType.Getter)]
        public class CurRotDrawMode_Patch
        {
            static bool Prefix(PawnRenderer __instance, Pawn ___pawn, ref RotDrawMode __result)
            {  
                if (___pawn != null && ___pawn.health != null)
                {
                    //check hediffs first
                    foreach (var item in ___pawn.health.hediffSet.hediffs)
                    {
                        if (item is IRotDrawOverrider drawOverrider)
                        {
                            if (drawOverrider != null)
                            {
                                __result = drawOverrider.OverridenRotDrawMode;
                                return false;
                            }
                        }
                    }


                    if (___pawn.health.hediffSet.hediffs.Any(x => x is HediffWithComps withComps && withComps.comps.Any(y => y is IRotDrawOverrider)))
                    {
                        //checks comps if not
                        foreach (var item in ___pawn.health.hediffSet.GetAllComps())
                        {
                            if (item is IRotDrawOverrider drawOverrider)
                            {
                                if (drawOverrider != null)
                                {
                                    __result = drawOverrider.OverridenRotDrawMode;
                                    return false;
                                }
                            }
                        }         
                    }
                }

                return true;
            }
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
