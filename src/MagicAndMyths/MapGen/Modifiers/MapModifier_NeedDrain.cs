using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class MapModifier_NeedDrain : MapModifier
    {
        public override int MinTicksBetweenEffects => 1000;
        public override Color ModifierColor => new Color(0.8f, 0.4f, 0.0f, 0.4f);

        public MapModifier_NeedDrain(Map map) : base(map) { }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void ApplyEffect()
        {
            foreach (var item in this.map.mapPawns.FreeColonistsSpawned)
            {




            }
        }

        public override Texture2D GetModifierTexture()
        {
            return ContentFinder<Texture2D>.Get("UI/Icons/ThingCategories/FoodMeals", true);
        }

        public override string GetModifierExplanation()
        {
            return "Food Decay Curse: Food in this area is rotting at an accelerated rate.\n" +
                   $"Next decay effect in: {ticksUntilNext} ticks";
        }
    }
}

