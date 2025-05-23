﻿using RimWorld;
using SquadBehaviour;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_SummonPawn : CompProperties_AbilityEffect
    {
        public PawnKindDef summonKind;

        public CompProperties_SummonPawn()
        {
            compClass = typeof(CompAbilityEffect_SummonPawn);
        }
    }

    public class CompAbilityEffect_SummonPawn : CompAbilityEffect
    {
        new CompProperties_SummonPawn Props => (CompProperties_SummonPawn)props;
        private Pawn SummonedPawn = null;


        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (SummonedPawn != null)
            {
                if (!SummonedPawn.Destroyed)
                {
                    SummonedPawn.Destroy(DestroyMode.KillFinalize);
                }

                EventManager.Instance.OnThingKilled -= EventManager_OnThingKilled;
                SummonedPawn = null;
            }
            else
            {
                SummonedPawn = PawnGenerator.GeneratePawn(Props.summonKind, parent.pawn.Faction);

                SummonedPawn = (Pawn)GenSpawn.Spawn(SummonedPawn, parent.pawn.Position, parent.pawn.Map);


                if (!SummonedPawn.RaceProps.Humanlike)
                {
                    MagicUtil.TrainPawn(SummonedPawn, parent.pawn);
                }

                if (SummonedPawn != null)
                {
                    if (parent.pawn.TryGetSquadLeader(out Comp_PawnSquadLeader squadLeader))
                    {
                        squadLeader.AddToSquad(SummonedPawn);
                    }


                    EventManager.Instance.OnThingKilled += EventManager_OnThingKilled;
                }


            }
        }

        private void EventManager_OnThingKilled(Pawn arg1, DamageInfo arg2, Hediff arg3)
        {
            if (SummonedPawn != null && arg1 == SummonedPawn)
            {
                EventManager.Instance.OnThingKilled -= EventManager_OnThingKilled;
                if (!SummonedPawn.Destroyed)
                {
                    SummonedPawn.Destroy(DestroyMode.KillFinalize);
                }

                Log.Message($"Summon died, destroying");
                SummonedPawn = null;
            }
        }
    }
}
