using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class EnchantDef : Def
    {
        public float commonality = 1f;
        public bool isUnique = true;
        public string texPath;

        public int slotLevel = 1;

        public MateriaTypeDef SlotType;
        public List<EnchantEffectDef> effects;

        public List<EnchantCategoryDef> allowedCategories;
        public Color textColor = Color.white;

        private Texture2D _IconTex;
        public Texture2D IconTex
        {
            get
            {
                if (_IconTex == null)
                {
                    _IconTex = ContentFinder<Texture2D>.Get(texPath);
                }

                return _IconTex;
            }
        }


        public string GetFullDescription()
        {
            string fullDescription = description + "\n\n";

            foreach (var item in effects)
            {
                fullDescription += item.EffectDescription + "\n\n";
            }

            return fullDescription;
        }

        public bool IsValidEquipmentType(Thing thing)
        {
            if (allowedCategories == null ||
                allowedCategories.Contains(MagicAndMythDefOf.EnchantCategory_Universal) ||
                allowedCategories.Count == 0)
            {
                return true;
            }

            if (thing.def.IsMeleeWeapon && allowedCategories.Contains(MagicAndMythDefOf.EnchantCategory_Melee) || allowedCategories.Contains(MagicAndMythDefOf.EnchantCategory_Weapon))
            {
                return true;
            }

            if (thing.def.IsRangedWeapon && allowedCategories.Contains(MagicAndMythDefOf.EnchantCategory_Ranged) || allowedCategories.Contains(MagicAndMythDefOf.EnchantCategory_Weapon))
            {
                return true;
            }

            if (thing.def.IsApparel && allowedCategories.Contains(MagicAndMythDefOf.EnchantCategory_Armor))
            {
                return true;
            }


            return false;
        }
        public string GetColouredLabel()
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(textColor)}>{label}</color>";
        }
    }
}
