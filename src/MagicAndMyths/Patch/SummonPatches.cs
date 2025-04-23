using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public static class SummonPatches
    {
        // Harmony patches
        [HarmonyPatch(typeof(FloatMenuMakerMap), "CanTakeOrder")]
        public static class FloatMenuMakerMap_CanTakeOrder_Patch
        {
            [HarmonyPostfix]
            public static void MakePawnControllable(Pawn pawn, ref bool __result)
            {
                if (DraftingUtility.IsDraftableCreature(pawn) && pawn.Faction?.IsPlayer == true)
                {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(Pawn), "WorkTypeIsDisabled")]
        public static class Pawn_WorkTypeIsDisabled_Patch
        {
            [HarmonyPostfix]
            public static void DisableDoctorWork(WorkTypeDef w, Pawn __instance, ref bool __result)
            {
                if (w == WorkTypeDefOf.Doctor && DraftingUtility.IsDraftableCreature(__instance)
                    && __instance.RaceProps.IsMechanoid == false)
                {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(SchoolUtility), "CanTeachNow")]
        public static class SchoolUtility_CanTeachNow_Patch
        {
            [HarmonyPrefix]
            public static bool PreventTeaching(Pawn teacher, ref bool __result)
            {
                if (DraftingUtility.IsDraftableCreature(teacher))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Pawn), "IsColonistPlayerControlled", MethodType.Getter)]
        public static class Pawn_IsColonistPlayerControlled_Patch
        {
            [HarmonyPostfix]
            public static void AddDraftableCreatureAsColonist(Pawn __instance, ref bool __result)
            {
                if (DraftingUtility.IsDraftableCreature(__instance))
                {
                    __result = __instance.Spawned && __instance.Faction == Faction.OfPlayer;
                }
            }
        }

        [HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState")]
        public static class Patch_MentalStateHandler_TryStartMentalState
        {
            public static bool Prefix(Pawn ___pawn, ref bool __result)
            {
                if (___pawn.Faction == Faction.OfPlayer && Current.Game.GetComponent<GameComp_Transformation>().IsTransformationPawn(___pawn, out Pawn original))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Pawn_MindState), "StartFleeingBecauseOfPawnAction")]
        public static class Patch_Pawn_MindState_StartFleeingBecauseOfPawnAction
        {
            public static bool Prefix(Pawn ___pawn)
            {
                return !___pawn.IsControlledSummon();
            }
        }

        [HarmonyPatch(typeof(JobGiver_ConfigurableHostilityResponse), "TryGetFleeJob")]
        public static class Patch_JobGiver_ConfigurableHostilityResponse_TryGetFleeJob
        {
            public static bool Prefix(Pawn pawn, ref Job __result)
            {
                if (pawn.Faction == Faction.OfPlayer && pawn.IsControlledSummon())
                {
                    __result = null;
                    return false;
                }
                return true;
            }
        }
        public static class Patch_Pawn_Thinker_ThinkTrees
        {
            [HarmonyPatch(typeof(Pawn_Thinker), "MainThinkTree", MethodType.Getter)]
            public static class Patch_MainThinkTree
            {
                public static bool Prefix(Pawn_Thinker __instance, ref ThinkTreeDef __result)
                {
                    return HandleThinkTreePatch(__instance, ref __result, isConstant: false);
                }
            }

            //[HarmonyPatch(typeof(Pawn_Thinker), "ConstantThinkTree", MethodType.Getter)]
            //public static class Patch_ConstantThinkTree
            //{
            //    public static bool Prefix(Pawn_Thinker __instance, ref ThinkTreeDef __result)
            //    {
            //        return HandleThinkTreePatch(__instance, ref __result, isConstant: true);
            //    }
            //}

            private static bool HandleThinkTreePatch(Pawn_Thinker __instance, ref ThinkTreeDef __result, bool isConstant)
            {
                Pawn pawn = __instance.pawn;

                if (pawn == null || pawn.Faction != Faction.OfPlayer)
                    return true;

                if (!isConstant && pawn.Spawned)
                {
                    if (Current.Game.GetComponent<GameComp_Transformation>().IsTransformationPawn(pawn, out Pawn original))
                    {
                        __result = MagicAndMythDefOf.MagicAndMyths_TransformationTree;
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Pawn_DraftController), "ShowDraftGizmo", MethodType.Getter)]
        public static class Patch_Pawn_DraftController
        {
            public static bool Prefix(Pawn_DraftController __instance, Pawn ___pawn, ref bool __result)
            {
                if (___pawn.Faction == Faction.OfPlayer && Current.Game.GetComponent<GameComp_Transformation>().IsTransformationPawn(___pawn, out Pawn original))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(AutoUndrafter), "ShouldAutoUndraft")]
        public static class Patch_AutoUndrafter
        {
            public static bool Prefix(AutoUndrafter __instance, Pawn ___pawn, ref bool __result)
            {
                if (___pawn.Faction == Faction.OfPlayer && Current.Game.GetComponent<GameComp_Transformation>().IsTransformationPawn(___pawn, out Pawn original))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ITab_Pawn_Gear), "IsVisible", MethodType.Getter)]
        public static class Patch_ITab_Pawn_Gear
        {
            public static bool Prefix(ITab_Pawn_Gear __instance, ref bool __result)
            {
                Thing thing = Find.Selector.SingleSelectedThing;

                if (thing != null && thing is Pawn selectedPawn)
                {
                    if (selectedPawn.Faction == Faction.OfPlayer && !selectedPawn.RaceProps.Humanlike && Current.Game.GetComponent<GameComp_Transformation>().IsTransformationPawn(selectedPawn, out Pawn original))
                    {
                        __result = false;
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Designator_Slaughter), "CanDesignateThing")]
        public static class Patch_Designator_Slaughter
        {
            public static bool Prefix(Designator_Slaughter __instance, Thing t, ref AcceptanceReport __result)
            {
                if (t is Pawn pawn)
                {
                    if (pawn.Faction == Faction.OfPlayer && Current.Game.GetComponent<GameComp_Transformation>().IsTransformationPawn(pawn, out Pawn original))
                    {
                        __result = AcceptanceReport.WasRejected;
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Designator_ReleaseAnimalToWild), "CanDesignateThing")]
        public static class Patch_Designator_ReleaseAnimalToWild
        {
            public static bool Prefix(Designator_ReleaseAnimalToWild __instance, Thing t, ref AcceptanceReport __result)
            {
                if (t is Pawn pawn)
                {
                    if (pawn.Faction == Faction.OfPlayer && Current.Game.GetComponent<GameComp_Transformation>().IsTransformationPawn(pawn, out Pawn original))
                    {
                        __result = AcceptanceReport.WasRejected;
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
