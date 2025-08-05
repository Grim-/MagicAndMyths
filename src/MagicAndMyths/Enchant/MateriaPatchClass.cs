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
                    })
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

}