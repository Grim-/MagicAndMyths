using HarmonyLib;
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
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (!def.IsWithinCategory(ThingCategoryDefOf.Chunks))
                {
                    if (def.comps != null)
                    {
                        def.comps.Add(new CompProperties_ThingProperties());
                    }
                }
            }

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
            public static void Prefix(Thing eq, ref Vector3 drawLoc, float aimAngle)
            {
                if (eq != null && eq.def != null && eq.def.HasModExtension<DrawOffsetExt>())
                {
                    drawLoc += eq.def.GetModExtension<DrawOffsetExt>().offset;
                }
            }
        }


        [HarmonyPatch(typeof(Pawn_PathFollower), "SetupMoveIntoNextCell")]
        public static class Patch_Pawn_PathFollower_SetupMoveIntoNextCell
        {
            public static void Postfix(Pawn_PathFollower __instance, ref IntVec3 ___nextCell, Pawn ___pawn)
            {
                if (___pawn != null)
                {
                    EventManager.PawnArrivedAtPathDestination(___pawn, ___nextCell);
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



        [HarmonyPatch(typeof(PawnRenderUtility), "DrawEquipmentAndApparelExtras")]
        public class PawnRenderUtility_Patch
        {
            static void Prefix(Pawn pawn, Vector3 drawPos, Rot4 facing, PawnRenderFlags flags)
            {          
                if (pawn != null && pawn.equipment != null)
                {
                    Thing primary = pawn.equipment.Primary;

                    if (primary != null && primary.def.HasModExtension<DrawOffsetExt>())
                    {
                        drawPos += primary.def.GetModExtension<DrawOffsetExt>().offset;
                    }
                }
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
