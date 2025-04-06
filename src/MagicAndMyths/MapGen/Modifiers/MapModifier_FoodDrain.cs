using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class MapModifier_FoodDrain : MapModifier
    {
        public override int MinTicksBetweenEffects => 1000;

        public MapModifier_FoodDrain(Map map) : base(map) { }

        public override void ApplyEffect()
        {
            List<Thing> foodItems = map.listerThings.ThingsInGroup(ThingRequestGroup.FoodSource);

            foreach (Thing food in foodItems)
            {
                CompRottable rottable = food.TryGetComp<CompRottable>();
                if (rottable != null)
                {
                    rottable.RotProgress += 2000;
                }
            }
        }
    }
}

