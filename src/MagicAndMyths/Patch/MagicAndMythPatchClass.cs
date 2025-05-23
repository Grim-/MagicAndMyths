﻿using HarmonyLib;
using RimWorld;
using RimWorld.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI;

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

        [HarmonyPatch(typeof(ThingSelectionUtility))]
        [HarmonyPatch("SelectableByHotkey")]
        [HarmonyPatch(new[] { typeof(Thing) })]
        public static class Patch_ThingSelectionUtility_SelectableByHotkey
        {
            public static bool Prefix(Thing t, ref bool __result)
            {
                if (t.IsInvisible())
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(ThingSelectionUtility))]
        [HarmonyPatch("SelectableByMapClick")]
        [HarmonyPatch(new[] { typeof(Thing) })]
        public static class Patch_ThingSelectionUtility_SelectableByMapClick
        {
            public static bool Prefix(Thing t, ref bool __result)
            {
                if (t.IsInvisible())
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(Pawn_EquipmentTracker))]
        [HarmonyPatch("TryDropEquipment")]
        public static class Patch_TryDropEquipment
        {
            [HarmonyPrefix]
            public static bool Prefix(ThingWithComps eq)
            {
                var lockComp = eq.GetComp<Comp_CursedEquipment>();
                if (lockComp != null && lockComp.IsSlotLocked)
                {
                    Messages.Message($"Cannot remove {eq.Label}: it is locked to the equipment slot.",
                        MessageTypeDefOf.RejectInput, false);
                    return false;
                }
                return true;
            }
        }

        //[HarmonyPatch(typeof(Pawn_ApparelTracker))]
        //[HarmonyPatch("TryDrop")]
        //public static class Patch_TryDrop
        //{
        //    [HarmonyPrefix]
        //    public static bool Prefix(Apparel apparel, ref bool __result)
        //    {
        //        var lockComp = apparel.GetComp<Comp_CursedEquipment>();
        //        if (lockComp != null && lockComp.IsSlotLocked)
        //        {
        //            Messages.Message($"Cannot remove {apparel.Label}: it is locked to the apparel slot.",
        //                MessageTypeDefOf.RejectInput, false);
        //            __result = false;
        //            return false;
        //        }
        //        return true;
        //    }
        //}

        [HarmonyPatch(typeof(Graphic_Single))]
        [HarmonyPatch("MatAt")]
        [HarmonyPatch(new[] { typeof(Rot4), typeof(Thing) })]
        public static class Patch_Graphic_Single_MatAt
        {

            public static Dictionary<Material, Material> InvisMaterialCache = new Dictionary<Material, Material>();

            public static bool Prefix(Graphic_Single __instance, ref Material __result, Rot4 rot, Thing thing)
            {
                if (__instance == null)
                    return true;

                if (thing != null && thing.Spawned && thing.IsInvisible())
                {

                    if (InvisMaterialCache.ContainsKey(__result))
                    {
                        __result = InvisMaterialCache[__result];
                        return false;
                    }
                    else
                    {
                        Material originalMaterial = __result;
                        Material invisMat = new Material(ShaderDatabase.Invisible);
                        if (thing.def.graphicData != null && thing.def.graphicData.shaderParameters != null)
                        {
                            foreach (var item in thing.def.graphicData.shaderParameters)
                            {
                                if (item != null)
                                {
                                    item.Apply(invisMat);
                                }
                            }
                        }

                        Texture2D graphicMainTex = ContentFinder<Texture2D>.Get(thing.def.graphicData.texPath);

                        if (graphicMainTex != null)
                        {
                            invisMat.SetTexture("_MainTex", graphicMainTex);
                        }

                        if (TexGame.InvisDistortion != null)
                        {
                            invisMat.SetTexture("_NoiseTex", TexGame.InvisDistortion);
                        }

                        invisMat.color = new Color(0.75f, 0.93f, 0.98f, 0.5f);

                        InvisMaterialCache.Add(originalMaterial, invisMat);
                        __result = invisMat;
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PawnRenderUtility), "DrawEquipmentAiming")]
        public static class Patch_PawnRenderUtility_DrawEquipmentAiming
        {
            [HarmonyPrefix]
            public static void Prefix(Thing eq, ref Vector3 drawLoc, ref float aimAngle)
            {
                if (eq != null && eq.def != null && eq.def.HasModExtension<DrawOffsetExt>())
                {
                    drawLoc += eq.def.GetModExtension<DrawOffsetExt>().GetOffsetForRot(eq.Rotation);
                    //aimAngle = 270f;
                }
            }
        }



        [HarmonyPatch(typeof(ColonistBarColonistDrawer))]
        [HarmonyPatch("HandleGroupFrameClicks")]
        public static class Patch_ColonistBarColonistDrawer_HandleGroupFrameClicks
        {
            [HarmonyPrefix]
            public static bool Prefix(ColonistBarColonistDrawer __instance, int group)
            {
                Rect rect = __instance.GroupFrameRect(group);

                if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && Mouse.IsOver(rect))
                {
                    Event.current.Use();

                    ColonistBar.Entry entry = Find.ColonistBar.Entries.Find(x => x.group == group);
                    Map map = entry.map;

                    if (map != null)
                    {
                        ExtendedMapParent dungeonParent = map.Parent as ExtendedMapParent;
                        if (dungeonParent != null)
                        {
                            HandleDungeonMapRightClick(dungeonParent, map);
                            return false;
                        }
                    }
                }

                return true; 
            }

            private static void HandleDungeonMapRightClick(ExtendedMapParent dungeonParent, Map map)
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                options.AddRange(dungeonParent.GetFloatMenuOptions());

                if (options.Count > 0)
                {
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }
        }

        [HarmonyPatch(typeof(ApparelGraphicRecordGetter))]
        [HarmonyPatch("TryGetGraphicApparel")]
        public static class Patch_ApparelGraphicRecordGetter_TryGetGraphicApparel
        {
            [HarmonyPostfix]
            public static void Postfix(Apparel apparel, BodyTypeDef bodyType, ref ApparelGraphicRecord rec)
            {
                if (apparel.def.graphicData is GraphicDataWithShader graphicDataWithShader)
                {
                    Graphic_MultiWithShader graphic = (Graphic_MultiWithShader)GraphicDatabase.Get<Graphic_MultiWithShader>(graphicDataWithShader.texPath, AssetBundleShaderManager.GetShaderByAssetName(graphicDataWithShader.customShaderName), apparel.def.graphicData.drawSize, apparel.DrawColor, apparel.DrawColor, graphicDataWithShader);
                    rec = new ApparelGraphicRecord(graphic, apparel);
  
                }
            }
        }

        [HarmonyPatch(typeof(Thing), "TakeDamage")]
        public static class Thing_TakeDamage_Patch
        {
            public static void Prefix(Thing __instance, ref DamageInfo dinfo)
            {
                if (__instance is Pawn pawnTakingDamage && !pawnTakingDamage.Dead)
                {
                    List<HediffComp_BioShield> bioShield = pawnTakingDamage.health.hediffSet.GetHediffComps<HediffComp_BioShield>().ToList();
                    foreach (var item in bioShield)
                    {
                        if (!item.CanMitigate(dinfo))
                        {
                            continue;
                        }

                        float mitigatedAmount = item.MitigateDamage(dinfo);
                        float cost = item.EnergyCost(mitigatedAmount);
                        if (item.HasEnough(cost))
                        {
                            Log.Message($"Mitigated {mitigatedAmount} cost {cost} - {item.Props.energyCostPerDamage} per damage point");
                            dinfo.SetAmount(dinfo.Amount - mitigatedAmount);
                            item.TryUseEnergy(cost);
                            break;
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

        [HarmonyPatch(typeof(BodyPartDef), "GetMaxHealth")]
        public class GetMaxHealth_Patch
        {
            [HarmonyPriority(0)]
            private static void Postfix(BodyPartDef __instance, Pawn pawn, ref float __result)
            {
                if (pawn != null)
                {
                    __result *= Mathf.Clamp(pawn.GetStatValue(MagicAndMythDefOf.Stat_LimbMaxHP), 0.01f, 100000f);
                }
            }
        }
        [HarmonyPatch(typeof(VerbProperties), "IsMeleeAttack", MethodType.Getter)]
        public class VerbProperties_Patch
        {
            static bool Prefix(VerbProperties __instance, ref bool __result)
            {
                if (__instance.verbClass.IsAssignableFrom(typeof(Verb_LaserBeam)))
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }
        [HarmonyPatch(typeof(VerbProperties), "Ranged", MethodType.Getter)]
        public class VerbProperties_RangedPatch
        {
            static bool Prefix(VerbProperties __instance, ref bool __result)
            {
                if (__instance.verbClass.IsAssignableFrom(typeof(Verb_LaserBeam)))
                {
                    __result = true;
                    return false;
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

        //public class Patch_EquipmentUtility_CanEquip
        //{
        //    public static bool Prefix(Thing thing, Pawn pawn, ref string cantReason, bool checkBonded, ref bool __result)
        //    {
        //        CompSelectiveBiocodable compSelective = thing.TryGetComp<CompSelectiveBiocodable>();

        //        if (compSelective != null)
        //        {
        //            if (!compSelective.CanBeBiocodedFor(pawn))
        //            {
        //                cantReason = "You cannot equip this, dont meet requirements";
        //                __result = false;
        //                return false;
        //            }
        //        }

        //        return true;
        //    }
        //}
    }
}
