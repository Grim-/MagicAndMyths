using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class AbilityRadialPagerSettings : ModSettings
    {
        public bool IsEnabled = true;
        public int itemsPerPage = 8;
        public float minItemSize = 32f;
        public float maxItemSize = 64f;
        public float minSpacePerItem = 4f;
        public float maxSpacePerItem = 12f;
        public float heightOffset = 50f;
        public bool showLabels = true;
        public float hoverSizeIncrease = 1.2f;
        public float backButtonSize = 32f;
        public float navButtonsSize = 20f;
        public int minPageCount = 3;
        public int maxPageCount = 12;
        public KeyBindingDef radialMenuHotKey = KeyBindingDefOf.Misc1;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref IsEnabled, "IsEnabled", true);
            Scribe_Values.Look(ref itemsPerPage, "itemsPerPage", 8);
            Scribe_Values.Look(ref minItemSize, "minItemSize", 32f);
            Scribe_Values.Look(ref maxItemSize, "maxItemSize", 64f);
            Scribe_Values.Look(ref minSpacePerItem, "minSpacePerItem", 4f);
            Scribe_Values.Look(ref maxSpacePerItem, "maxSpacePerItem", 12f);
            Scribe_Values.Look(ref heightOffset, "heightOffset", 50f);
            Scribe_Values.Look(ref showLabels, "showLabels", true);
            Scribe_Values.Look(ref hoverSizeIncrease, "hoverSizeIncrease", 1.2f);
            Scribe_Values.Look(ref backButtonSize, "backButtonSize", 32f);
            Scribe_Values.Look(ref navButtonsSize, "navButtonsSize", 20f);
            Scribe_Values.Look(ref minPageCount, "minPageCount", 3);
            Scribe_Values.Look(ref maxPageCount, "maxPageCount", 12);
            Scribe_Deep.Look(ref radialMenuHotKey, "radialMenuHotKey", KeyBindingDefOf.Misc1);
        }

        public void ResetToDefaults()
        {
            IsEnabled = true;
            itemsPerPage = 8;
            minItemSize = 32f;
            maxItemSize = 64f;
            minSpacePerItem = 4f;
            maxSpacePerItem = 12f;
            heightOffset = 50f;
            showLabels = true;
            hoverSizeIncrease = 1.2f;
            backButtonSize = 32f;
            navButtonsSize = 20f;
            minPageCount = 3;
            maxPageCount = 12;
            radialMenuHotKey = KeyBindingDefOf.Misc1;
        }
    }
}