using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_AbilityChangeWeather : CompProperties_AbilityEffect
    {
        public List<WeatherDef> weatherOptions;
        public bool forceTransition = true;

        public CompProperties_AbilityChangeWeather()
        {
            compClass = typeof(CompAbilityEffect_ChangeWeather);
        }
    }


    public class CompAbilityEffect_ChangeWeather : CompAbilityEffect
    {
        CompProperties_AbilityChangeWeather Props => (CompProperties_AbilityChangeWeather)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (this.parent.pawn.Map != null)
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (var item in Props.weatherOptions)
                {

                    options.Add(new FloatMenuOption($"{item.LabelCap}", () =>
                    {
                        this.parent.pawn.Map.weatherManager.TransitionTo(item);

                    }));
                }

                if (options.Count > 0)
                {
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }
        }
    }
}
