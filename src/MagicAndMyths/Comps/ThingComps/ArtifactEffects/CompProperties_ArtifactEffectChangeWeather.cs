using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ArtifactEffectChangeWeather : CompProperties
    {
        public WeatherDef weatherDef;

        public CompProperties_ArtifactEffectChangeWeather()
        {
            compClass = typeof(Comp_ArtifactEffecChangeWeather);
        }
    }

    public class Comp_ArtifactEffecChangeWeather : Comp_BaseAritfactEffect
    {
        private CompProperties_ArtifactEffectChangeWeather Props => (CompProperties_ArtifactEffectChangeWeather)props;

        public override void Apply(Pawn user, LocalTargetInfo target, Thing item)
        {
            if (user.Map != null && Props.weatherDef != null)
            {
                user.Map.weatherManager.TransitionTo(Props.weatherDef);
            }
        }
    }
}