using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    [StaticConstructorOnStartup]
    public static class PortalPatches
    {
        static PortalPatches()
        {
            var harmony = new Harmony("com.emo.portal");
            harmony.PatchAll();
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
    }
}