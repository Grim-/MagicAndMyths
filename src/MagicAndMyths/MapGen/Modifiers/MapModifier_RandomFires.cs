using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    // Example modifiers:
    public class MapModifier_RandomFires : MapModifier
    {
        public override int MinTicksBetweenEffects => 2000;
        public override int MaxTicksBetweenEffects => 4000;

        public MapModifier_RandomFires(Map map) : base(map) { }

        public override void ApplyEffect()
        {
            IntVec3 cell = CellFinder.RandomCell(map);
            if (!cell.Fogged(map) && map.mapPawns.AnyColonistSpawned)
            {

                Log.Message("Starting fire at");
                FireUtility.TryStartFireIn(cell, map, 0.1f, null);
            }
        }

        public override Texture2D GetModifierTexture()
        {
            return ContentFinder<Texture2D>.Get("UI/Icons/ThingCategories/FoodMeals", true);
        }

        public override string GetModifierExplanation()
        {
            return "Fires are started randomly.\n" +
                   $"Next in: {ticksUntilNext} ticks";
        }
    }
}

