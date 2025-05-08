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
        public static List<Comp_EnchantProvider> GetAllEquippedOrWornMateriaComps(this Pawn pawn)
        {
            List<Comp_EnchantProvider> materiacomps = new List<Comp_EnchantProvider>();


            if (pawn.equipment != null)
            {
                foreach (Thing thing in pawn.equipment.AllEquipmentListForReading)
                {
                    if (thing is ThingWithComps withComps)
                    {
                        foreach (var item in withComps.AllComps)
                        {
                            if (item is Comp_EnchantProvider materia)
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
                        if (item is Comp_EnchantProvider materia)
                        {
                            materiacomps.Add(materia);
                        }
                    }
                }

            }

            return materiacomps;
        }
    }


}
