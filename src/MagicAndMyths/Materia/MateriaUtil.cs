using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public static class MateriaUtil
    {
        public static bool HasThingCategoryOrParent(this Thing thing, ThingCategoryDef categoryToCheck)
        {
            if (thing.HasThingCategory(categoryToCheck))
                return true;

            foreach (ThingCategoryDef parentCategory in categoryToCheck.Parents)
            {
                if (thing.HasThingCategory(parentCategory))
                    return true;
            }

            foreach (ThingCategoryDef childCategory in categoryToCheck.ThisAndChildCategoryDefs)
            {
                if (thing.HasThingCategory(childCategory))
                    return true;
            }

            return false;
        }
        public static List<Comp_Enchant> GetAllEquippedOrWornMateriaComps(this Pawn pawn)
        {
            List<Comp_Enchant> materiacomps = new List<Comp_Enchant>();


            if (pawn.equipment != null)
            {
                foreach (Thing thing in pawn.equipment.AllEquipmentListForReading)
                {
                    if (thing is ThingWithComps withComps)
                    {
                        foreach (var item in withComps.AllComps)
                        {
                            if (item is Comp_Enchant materia)
                            {
                                materiacomps.Add(materia);
                            }
                        }
                    }
                }
            }

            if (pawn.apparel != null)
            {
                foreach (Apparel thing in pawn.apparel.WornApparel)
                {
                    foreach (var item in thing.AllComps)
                    {
                        if (item is Comp_Enchant materia)
                        {
                            materiacomps.Add(materia);
                        }
                    }
                }

            }

            return materiacomps;
        }
        public static List<FloatMenuOption> GenerateMateriaOptions(Comp_Enchant materiaComp, Pawn usingPawn, EnchantSlot slot)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            if (Prefs.DevMode)
            {
                foreach (EnchantDef materiaDef in DefDatabase<EnchantDef>.AllDefs)
                {
                    var cacheMatdef = materiaDef;
                    if (slot.CanAccept(cacheMatdef))
                    {
                        options.Add(new FloatMenuOption(cacheMatdef.label, () =>
                        {
                            var cacheSlot = slot;
                            materiaComp.EquipMateria(cacheMatdef, cacheSlot);
                        }));
                    }
                }
            }

            if (options.Count == 0)
            {
                options.Add(new FloatMenuOption("No compatible materia available", null));
            }

            options.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.OrdinalIgnoreCase));
            return options;
        }
    }


}
