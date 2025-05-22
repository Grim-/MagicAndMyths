using RimWorld;
using SquadBehaviour;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

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
        private ThingFlyer flyer;


        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            Pawn pawn = parent.pawn;
            if (pawn == null)
                return;
            Map map = parent.pawn.Map;
            if (map == null)
                return;


            if (flyer != null)
            {
                flyer.OnFlightTick -= Flyer_OnFlightTick;
                if(!flyer.Destroyed) flyer.Destroy();
                flyer = null;
            }

            IntVec3 spawnPosition = pawn.Position;
            IntVec3 tagetPosition = target.Cell;


            if (tagetPosition.IsValid)
            {

                flyer = ThingFlyer.MakeFlyer(pawn, tagetPosition, map, null, null, this.parent.pawn, this.parent.pawn.DrawPos, false);
                flyer.OnFlightTick += Flyer_OnFlightTick;
                ThingFlyer.LaunchFlyer(flyer, pawn, tagetPosition, map);
            }
        }

        private void Flyer_OnFlightTick(int tick, IntVec3 cell, Map map, Thing arg3)
        {
            List<Thing> things = cell.GetThingList(map).ToList();

            foreach (var t in things)
            {
                if (t is Pawn || t is Building building)
                {
                    if (t != this.parent.pawn)
                    {
                        DamageInfo damage = t.def.mineable ? new DamageInfo(DamageDefOf.Mining, 344 * 2, 1) : new DamageInfo(DamageDefOf.Bomb, 150, 1);
                        t.TakeDamage(damage);
                    }

                }

                EffecterDefOf.ImpactSmallDustCloud.Spawn(t.Position, map);
            }
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref flyer, "flyer");
        }
    }

}
