using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class MapModifier_Temperature : MapModifier
    {
        public float targetTemperature = 100f; // Target temperature in Celsius
        public float temperaturePushAmount = 5f; // How much to push temperature per effect
        public float temperatureRadius = 10f; // Radius of effect for each push
        public override int MinTicksBetweenEffects => 250; // More frequent updates for smoother transition
        public override Color ModifierColor => new Color(0.2f, 0.6f, 0.8f, 0.4f); // Blue-ish tint

        public MapModifier_Temperature(Map map) : base(map) { }



        public override void ApplyEffect()
        {
            // Sample the current average temperature of the map
            float currentAvgTemp = 0f;
            int cellCount = 0;

            foreach (Room room in map.regionGrid.allRooms)
            {
                if (!room.TouchesMapEdge)
                {
                    currentAvgTemp += room.Temperature;
                    cellCount++;
                }
            }

            if (cellCount > 0)
            {
                currentAvgTemp /= cellCount;

                bool needsHeating = currentAvgTemp < targetTemperature;

                float pushMagnitude = Mathf.Min(temperaturePushAmount, Mathf.Abs(targetTemperature - currentAvgTemp));

                int pushPoints = Mathf.CeilToInt(map.Size.x * map.Size.z / (temperatureRadius * temperatureRadius * 4));

                for (int i = 0; i < pushPoints; i++)
                {
                    IntVec3 cell = CellFinder.RandomCell(map);

                    if (cell.InBounds(map) && cell.Walkable(map))
                    {
                        if (needsHeating)
                        {
                            GenTemperature.PushHeat(cell, map, pushMagnitude * temperatureRadius);
                        }
                        else
                        {
                            GenTemperature.PushHeat(cell, map, -pushMagnitude * temperatureRadius);
                        }
                    }
                }
            }
        }

        public override Texture2D GetModifierTexture()
        {
            return ContentFinder<Texture2D>.Get("UI/Icons/ThingCategories/FoodMeals", true);
        }

        public override string GetModifierExplanation()
        {
            float currentAvgTemp = 0f;
            int cellCount = 0;

            foreach (Room room in map.regionGrid.allRooms)
            {
                if (!room.TouchesMapEdge)
                {
                    currentAvgTemp += room.Temperature;
                    cellCount++;
                }
            }

            if (cellCount > 0)
            {
                currentAvgTemp /= cellCount;
            }

            string direction = currentAvgTemp < targetTemperature ? "warming" :
                              (currentAvgTemp > targetTemperature ? "cooling" : "maintaining");

            return $"Temperature control active: {direction} to reach {targetTemperature}°C (current avg: {currentAvgTemp:F1}°C).\n" +
                   $"Next temperature adjustment in: {ticksUntilNext} ticks";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref targetTemperature, "targetTemperature", 21f);
            Scribe_Values.Look(ref temperaturePushAmount, "temperaturePushAmount", 5f);
            Scribe_Values.Look(ref temperatureRadius, "temperatureRadius", 10f);
        }
    }
}

