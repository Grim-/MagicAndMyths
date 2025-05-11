using RimWorld;
using SquadBehaviour;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_Fly : CompProperties_AbilityEffect
    {
        public CompProperties_Fly()
        {
            compClass = typeof(CompAbilityEffect_Fly);
        }
    }

    public class CompAbilityEffect_Fly : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            Pawn pawn = parent.pawn;
            if (pawn == null)
                return;
            Map map = parent.pawn.Map;
            if (map == null)
                return;

            IntVec3 spawnPosition = pawn.Position;
            IntVec3 tagetPosition = target.Cell;


            if (tagetPosition.IsValid)
            {
                PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(MagicAndMythDefOf.MagicAndMyths_SimpleFlyer, pawn, tagetPosition, null, null, false, null, this.parent, target);
                GenSpawn.Spawn(pawnFlyer, spawnPosition, map);
            }
        }
    }

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
