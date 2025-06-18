using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class MapModifier_Aging : MapModifier
    {

        public int AgeChangeTicks = 1000;
        public override int MinTicksBetweenEffects => 1000;
        public override Color ModifierColor => new Color(0.8f, 0.4f, 0.0f, 0.4f);

        public MapModifier_Aging(Map map) : base(map) { }


        public override void ApplyEffect()
        {
            foreach (var item in map.mapPawns.FreeColonistsSpawned)
            {
                item.ageTracker.AgeBiologicalTicks += AgeChangeTicks;
            }
        }

        public override Texture2D GetModifierTexture()
        {
            return ContentFinder<Texture2D>.Get("UI/Icons/ThingCategories/FoodMeals", true);
        }

        public override string GetModifierExplanation()
        {
            return "Aging all colonists in this area are aging at an accelerated rate.\n" +
                   $"Next decay effect in: {ticksUntilNext} ticks";
        }
    }
}

