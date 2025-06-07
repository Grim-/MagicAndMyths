using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_StormStep : CompProperties_ConsumeHediffStacks
    {
        public HediffDef hediffToGrant;
        public int stacksToGrant = 1;
        public float baseStunDuration = 60f;
        public float stunDurationPerStack = 30f;
        public float baseDamage = 5f;
        public float damagePerStack = 2f;
        public float effectRadius = 2f;
        public DamageDef damageDef = DamageDefOf.EMP;

        public CompProperties_StormStep()
        {
            compClass = typeof(CompAbilityEffect_StormStep);
        }
    }


    public class CompAbilityEffect_StormStep : CompAbilityEffect_ConsumeHediffStacks
    {
        private ThingFlyer flyer;

        public new CompProperties_StormStep Props => (CompProperties_StormStep)this.props;

        protected override void ApplyWithConsumedStacks(LocalTargetInfo target, LocalTargetInfo dest, int consumedStacks)
        {
            Pawn pawn = parent.pawn;
            Map map = pawn.Map;

            
            PerformDash(pawn, map, target.Cell, consumedStacks);
        }

        private void GrantLightningArmor(Pawn pawn)
        {
            if (Props.hediffToGrant == null)
                return;

            Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffToGrant);
            if (existingHediff != null)
            {
                HediffWithStacks stackHediff = existingHediff as HediffWithStacks;
                if (stackHediff != null)
                {
                    stackHediff.AddStack(Props.stacksToGrant);
                }
            }
            else
            {
                Hediff newHediff = HediffMaker.MakeHediff(Props.hediffToGrant, pawn);
                if (newHediff is HediffWithStacks newStackHediff)
                {
                    newStackHediff.SetStack(Props.stacksToGrant);
                }
                pawn.health.AddHediff(newHediff);
            }

            EffecterDefOf.Skip_EntryNoDelay.Spawn(pawn.Position, pawn.Map, 0.5f);
        }

        private void PerformDash(Pawn pawn, Map map, IntVec3 targetPosition, int consumedStacks)
        {
            if (!targetPosition.IsValid)
                return;

            if (flyer != null)
            {
                flyer.OnRespawn -= Flyer_OnRespawn;
                if (!flyer.Destroyed)
                    flyer.Destroy();
                flyer = null;
            }

            flyer = ThingFlyer.MakeFlyer(pawn, targetPosition, map, null, null, pawn, pawn.DrawPos, false);
            flyer.OnRespawn += Flyer_OnRespawn;
            ThingFlyer.LaunchFlyer(flyer, pawn, pawn.Position, map);
        }

        private void Flyer_OnRespawn(IntVec3 cell, Thing thing, Pawn pawn)
        {
            if (pawn?.Map == null)
                return;

            if (consumedStacks == 0)
            {
                GrantLightningArmor(pawn);
                return;
            }


            Map map = pawn.Map;
            IEnumerable<Pawn> pawnsInRadius = GenRadial.RadialDistinctThingsAround(cell, map, Props.effectRadius, true)
                .OfType<Pawn>()
                .Where(p => p != pawn && p.Faction != pawn.Faction);

            foreach (Pawn target in pawnsInRadius)
            {
                float stunDuration = Props.baseStunDuration + (consumedStacks * Props.stunDurationPerStack);
                if (stunDuration > 0)
                {
                    target.stances.stunner.StunFor(Mathf.RoundToInt(stunDuration), pawn);
                }

                if (consumedStacks > 0)
                {
                    float damage = Props.baseDamage + (consumedStacks * Props.damagePerStack);
                    DamageInfo damageInfo = new DamageInfo(Props.damageDef, damage, 1f, -1f, pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown, target);
                    target.TakeDamage(damageInfo);
                }
            }

            //EffecterDefOf.Skip_EntryNoDelay.Spawn(cell, map, 1f);
            //if (consumedStacks > 0)
            //{
            //    for (int i = 0; i < Math.Min(consumedStacks, 5); i++)
            //    {
            //        EffecterDefOf.Lightning.Spawn(cell, map, 0.5f);
            //    }
            //}

            flyer.OnRespawn -= Flyer_OnRespawn;
            flyer = null;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref flyer, "flyer");
        }
    }
}
