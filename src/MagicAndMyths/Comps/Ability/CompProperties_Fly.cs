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
            if (parent.pawn?.Map == null)
                return;

            IntVec3 spawnPosition = parent.pawn.Position;
            IntVec3 tagetPosition = target.Cell;
			Map map = parent.pawn.Map;

			PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(MagicAndMythDefOf.MagicAndMyths_SimpleFlyer, parent.pawn, tagetPosition, null, null);
            GenSpawn.Spawn(pawnFlyer, spawnPosition, map);
        }
    }


}
