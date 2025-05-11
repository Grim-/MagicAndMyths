using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    [StaticConstructorOnStartup]
    public static class MateriaPatchClass
    {
        static MateriaPatchClass()
        {
            var harmony = new Harmony("com.emo.materiapatches");
            harmony.Patch(
                original: AccessTools.Method(typeof(EquipmentUtility), "CanEquip",
                    new Type[] {
                    typeof(Thing),
                    typeof(Pawn),
                    typeof(string).MakeByRefType(),
                    typeof(bool)
                    }),
                prefix: new HarmonyMethod(typeof(Patch_EquipmentUtility_CanEquip), nameof(Patch_EquipmentUtility_CanEquip.Prefix))
            );

            foreach (var def in DefDatabase<ThingDef>.AllDefs.Where(d => d.IsWeapon || d.IsRangedWeapon || d.IsApparel))
            {
                if (def.comps == null)
                    def.comps = new List<CompProperties>();

                if (def.comps?.OfType<CompProperties_EnchantProvider>().Any() == true || def.HasModExtension<NoMateriaExt>())
                    continue;

                def.comps.Add(new CompProperties_EnchantProvider());
            }


            foreach (var def in DefDatabase<ThingDef>.AllDefs.Where(d => d.race?.Humanlike == true))
            {
                if (def.comps == null)
                    def.comps = new List<CompProperties>();

                if (def.comps?.OfType<CompProperties_PawnEnchant>().Any() == true)
                    continue;

                def.comps.Add(new CompProperties_PawnEnchant());
            }
        }
    }

    #region Equipment Patches

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

    [HarmonyPatch(typeof(Pawn), "TickRare")]
    public class Pawn_TickRare_Patch
    {
        static void Postfix(Pawn __instance)
        {
            var pawnEnchantComp = __instance.GetComp<Comp_PawnEnchant>();
            if (pawnEnchantComp != null)
            {
                pawnEnchantComp.EquipTick();
            }
        }
    }

    //Allows Weapons to call PostPreApplyDamage which is normally only called on apparel.
    [HarmonyPatch(typeof(Pawn_HealthTracker), "PreApplyDamage")]
    public class Pawn_HealthTracker_PreApplyDamage_Patch
    {
        static void Postfix(Pawn_HealthTracker __instance, Pawn ___pawn, ref DamageInfo dinfo, ref bool absorbed)
        {
            if (___pawn == null)
                return;

            var pawnEnchantComp = ___pawn.GetComp<Comp_PawnEnchant>();
            if (pawnEnchantComp != null)
            {
                bool wasAbsorbed = pawnEnchantComp.Notify_PostPreApplyDamage(ref dinfo);
                if (wasAbsorbed)
                    absorbed = true;
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderUtility))]
    [HarmonyPatch("DrawEquipmentAndApparelExtras")]
    [HarmonyPatch(new Type[] { typeof(Pawn), typeof(Vector3), typeof(Rot4), typeof(PawnRenderFlags) })]
    public static class Patch_PawnRenderUtility_DrawEquipmentAndApparelExtras
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, Vector3 drawPos, Rot4 facing, PawnRenderFlags flags)
        {
            if (pawn == null)
                return;

            // Equipment provider comps still handle drawing
            List<Comp_EnchantProvider> materiaComps = pawn.GetAllEquippedOrWornMateriaComps();
            if (materiaComps != null)
            {
                foreach (var item in materiaComps)
                {
                    try
                    {
                        item.PostDraw();
                    }
                    catch (Exception e)
                    {
                        Log.ErrorOnce($"Exception thrown while trying to call EnchantProvider PostDraw {e}", pawn.thingIDNumber * 1000);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public class Pawn_GetGizmos_Patch
    {
        static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            var pawnEnchantComp = __instance.GetComp<Comp_PawnEnchant>();
            if (pawnEnchantComp != null)
            {
                var originalGizmos = __result.ToList();
                var enchantGizmos = pawnEnchantComp.GetGizmos();
                __result = originalGizmos.Concat(enchantGizmos);
            }
        }
    }
    #endregion

    [HarmonyPatch(typeof(Verb_MeleeAttackDamage))]
    [HarmonyPatch("ApplyMeleeDamageToTarget")]
    public static class Patch_Verb_MeleeAttack_ApplyMeleeDamageToTarget
    {
        public static void Postfix(Verb_MeleeAttack __instance, LocalTargetInfo target, ref DamageWorker.DamageResult __result)
        {
            if (__instance.CasterPawn != null)
            {
                var pawnEnchantComp = __instance.CasterPawn.GetComp<Comp_PawnEnchant>();
                if (pawnEnchantComp != null)
                {
                    __result = pawnEnchantComp.Notify_ApplyMeleeDamageToTarget(target, __instance.CasterPawn, __result);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Projectile))]
    [HarmonyPatch("Impact")]
    public class Patch_Projectile_Impact
    {
        public static void Postfix(Projectile __instance, Thing hitThing)
        {
            if (__instance != null && __instance.Launcher != null && __instance.Launcher is Pawn instigator)
            {
                var pawnEnchantComp = instigator.GetComp<Comp_PawnEnchant>();
                if (pawnEnchantComp != null)
                {
                    pawnEnchantComp.Notify_ProjectileImpact(instigator, hitThing, __instance);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("Kill")]
    [HarmonyPatch(new Type[] { typeof(DamageInfo?), typeof(Hediff) })]
    public static class Patch_Pawn_Kill
    {
        public static void Postfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit)
        {
            if (__instance is Pawn pawn && pawn.RaceProps.Humanlike)
            {
                var pawnEnchantComp = pawn.GetComp<Comp_PawnEnchant>();
                if (pawnEnchantComp != null)
                {
                    pawnEnchantComp.Notify_OwnerKilled();
                }
            }
        }
    }

    [HarmonyPatch(typeof(Bullet))]
    [HarmonyPatch("Impact")]
    public class Patch_Bullet_Impact
    {
        private static DamageInfo ProcessDamageInfo(DamageInfo dinfo, Thing hitThing, Projectile __instance)
        {
            if (__instance.Launcher is Pawn attacker)
            {
                var pawnEnchantComp = attacker.GetComp<Comp_PawnEnchant>();
                if (pawnEnchantComp != null)
                {
                    return pawnEnchantComp.Notify_ProjectileApplyDamageToTarget(dinfo, attacker, hitThing, __instance);
                }
            }
            return dinfo;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var takeDamageMethod = AccessTools.Method(typeof(Thing), nameof(Thing.TakeDamage));
            var processMethod = AccessTools.Method(typeof(Patch_Bullet_Impact), nameof(ProcessDamageInfo));

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(takeDamageMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);  // hitThing
                    yield return new CodeInstruction(OpCodes.Ldarg_0);  // Projectile
                    yield return new CodeInstruction(OpCodes.Call, processMethod);
                }
                yield return instruction;
            }
        }
    }

    [HarmonyPatch(typeof(StatWorker))]
    [HarmonyPatch("FinalizeValue")]
    public class Patch_StatWorker_FinalizeValue
    {
        public static void Prefix(StatWorker __instance, StatRequest req, StatDef ___stat, ref float val)
        {
            if (!req.HasThing)
                return;

            // Get pawn's enchant component
            Pawn pawn = req.Thing as Pawn;
            if (pawn != null)
            {
                var pawnEnchantComp = pawn.GetComp<Comp_PawnEnchant>();
                if (pawnEnchantComp != null)
                {
                    // Apply offsets first
                    foreach (var offset in pawnEnchantComp.GetStatOffsets(___stat))
                    {
                        val += offset.value;
                    }
                    // Then factors
                    foreach (var factor in pawnEnchantComp.GetStatFactors(___stat))
                    {
                        val *= factor.value;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(StatWorker))]
    [HarmonyPatch("GetExplanationUnfinalized")]
    public class Patch_StatWorker_GetExplanationUnfinalized
    {
        public static void Postfix(StatRequest req, ref string __result, StatDef ___stat)
        {
            if (!req.HasThing) return;

            StringBuilder explanation = new StringBuilder(__result);

            Pawn pawn = req.Thing as Pawn;
            if (pawn != null)
            {
                var pawnEnchantComp = pawn.GetComp<Comp_PawnEnchant>();
                if (pawnEnchantComp != null)
                {
                    string statExplanation = pawnEnchantComp.GetExplanation(___stat);
                    if (!statExplanation.NullOrEmpty())
                    {
                        explanation.AppendLine($"\nEnchantment effects:");
                        explanation.AppendLine(statExplanation);
                    }
                }
            }

            __result = explanation.ToString();
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), new[] { typeof(Thought_Memory), typeof(Pawn) })]
    [HarmonyPatch("TryGainMemory")]
    public static class Patch_MemoryThoughtHandler_TryGainMemory
    {
        public static void Postfix(MemoryThoughtHandler __instance, Thought_Memory newThought, Pawn otherPawn)
        {
            Pawn pawn = __instance.pawn;
            if (pawn == null)
                return;

            var pawnEnchantComp = pawn.GetComp<Comp_PawnEnchant>();
            if (pawnEnchantComp != null)
            {
                pawnEnchantComp.Notify_OwnerThoughtGained(newThought, otherPawn);
            }
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), new[] { typeof(Thought_Memory) })]
    [HarmonyPatch("RemoveMemory")]
    public static class Patch_MemoryThoughtHandler_RemoveMemory
    {
        public static void Prefix(MemoryThoughtHandler __instance, Thought_Memory th)
        {
            Pawn pawn = __instance.pawn;
            if (pawn == null)
                return;

            var pawnEnchantComp = pawn.GetComp<Comp_PawnEnchant>();
            if (pawnEnchantComp != null)
            {
                pawnEnchantComp.Notify_OwnerThoughtLost(th);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch("AddHediff", new[] {
        typeof(Hediff),
        typeof(BodyPartRecord),
        typeof(DamageInfo?),
        typeof(DamageWorker.DamageResult)
    })]
    public static class Patch_Pawn_HealthTracker_AddHediff
    {
        public static void Postfix(Hediff hediff, BodyPartRecord part, DamageInfo? dinfo, DamageWorker.DamageResult result, Pawn_HealthTracker __instance, Pawn ___pawn)
        {
            Pawn pawn = ___pawn;
            if (pawn == null)
                return;

            var pawnEnchantComp = pawn.GetComp<Comp_PawnEnchant>();
            if (pawnEnchantComp != null)
            {
                pawnEnchantComp.Notify_OwnerHediffGained(hediff, part, dinfo, result);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch("RemoveHediff", new[] {
        typeof(Hediff)
    })]
    public static class Patch_Pawn_HealthTracker_RemoveHediff
    {
        public static void Postfix(
            Hediff hediff,
            Pawn ___pawn)
        {
            Pawn pawn = ___pawn;
            if (pawn == null)
                return;

            var pawnEnchantComp = pawn.GetComp<Comp_PawnEnchant>();
            if (pawnEnchantComp != null)
            {
                pawnEnchantComp.Notify_OwnerHediffRemoved(hediff);
            }
        }
    }
}