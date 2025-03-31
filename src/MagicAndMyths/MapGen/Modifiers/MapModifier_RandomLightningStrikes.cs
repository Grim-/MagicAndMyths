using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class MapModifier_RandomLightningStrikes : MapModifier
    {
        public override int MinTicksBetweenEffects => 2000;
        public override int MaxTicksBetweenEffects => 4000;

        public MapModifier_RandomLightningStrikes(Map map) : base(map) { }

        public override void ApplyEffect()
        {
            IntVec3 cell = CellFinder.RandomCell(map);
            if (!cell.Fogged(map) && map.mapPawns.AnyColonistSpawned)
            {
                map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(map, cell));
            }
        }
    }
}

