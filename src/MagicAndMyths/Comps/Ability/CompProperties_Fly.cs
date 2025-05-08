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


}
