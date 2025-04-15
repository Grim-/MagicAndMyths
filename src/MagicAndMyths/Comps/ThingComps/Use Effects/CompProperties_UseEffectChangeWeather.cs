using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_UseEffectChangeWeather : CompProperties_UseEffect
    {
        public WeatherDef weatherDef;

        public CompProperties_UseEffectChangeWeather()
        {
            compClass = typeof(Comp_UseEffectChangeWeather);
        }
    }

    public class Comp_UseEffectChangeWeather : CompUseEffect
    {
        CompProperties_UseEffectChangeWeather Props => (CompProperties_UseEffectChangeWeather)props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            if (usedBy.Map != null && Props.weatherDef != null)
            {
                usedBy.Map.weatherManager.TransitionTo(Props.weatherDef);
            }
        }
    }

}