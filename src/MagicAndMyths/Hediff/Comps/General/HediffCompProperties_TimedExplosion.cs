using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_TimedExplosion : HediffCompProperties
    {
        public int ticksToDetonation = 60000;
        public float explosionRadius = 3f;
        public int explosionDamage = 30;
        public DamageDef explosionDamageDef;
        public ThingDef postExplosionSpawnThingDef = null;
        public float postExplosionSpawnChance = 0f;
        public int postExplosionSpawnThingCount = 1;
        public bool applyDamageToExplosionCellsNeighbors = false;
        public ThingDef preExplosionSpawnThingDef = null;
        public float preExplosionSpawnChance = 0f;
        public int preExplosionSpawnThingCount = 1;
        public bool damageFalloff = true;

        public bool scaleDamageWithSeverity = true;
        public float minSeverityDamageMod = 1f;
        public float maxSeverityDamageMod = 3f;

        public bool scaleRadiusWithSeverity = true;
        public float minSeverityRangeMod = 1f;
        public float maxSeverityRangeMod = 1.2f;


        public HediffCompProperties_TimedExplosion()
        {
            compClass = typeof(HediffComp_TimedExplosion);
        }
    }


    public class HediffComp_TimedExplosion : HediffComp
    {
        private int ticksRemaining;
        private int lastMoteSpawnTick = -1;

        public HediffCompProperties_TimedExplosion Props => (HediffCompProperties_TimedExplosion)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            ticksRemaining = Props.ticksToDetonation;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (parent.pawn.Map == null)
                return;

            ticksRemaining--;


            if (ticksRemaining <= 1000)
            {
                if (Find.TickManager.TicksGame - lastMoteSpawnTick >= 60)
                {
                    lastMoteSpawnTick = Find.TickManager.TicksGame;
                    DisplayCountdownMote();
                }
            }

            if (ticksRemaining <= 0)
            {
                Explode();
            }
        }

        private void DisplayCountdownMote()
        {
            if (!parent.pawn.Spawned || parent.pawn.Map == null)
                return;

            float secondsRemaining = (float)ticksRemaining / 60f;
            string timeText;

            if (secondsRemaining > 60f)
            {
                int minutes = Mathf.FloorToInt(secondsRemaining / 60f);
                int seconds = Mathf.FloorToInt(secondsRemaining % 60f);
                timeText = minutes + ":" + seconds.ToString("00");
            }
            else
            {
                timeText = secondsRemaining.ToString("0.0") + "s";
            }

            MoteMaker.ThrowText(parent.pawn.DrawPos, parent.pawn.Map, timeText, 1.9f);
        }

        private void Explode()
        {
            float scaledSeverityDamage = Props.explosionDamage + Mathf.Lerp(Props.minSeverityDamageMod, Props.maxSeverityDamageMod, this.parent.Severity / this.parent.def.maxSeverity);

            float scaledRadius = Props.explosionRadius + Mathf.Lerp(Props.minSeverityRangeMod, Props.maxSeverityRangeMod, this.parent.Severity / this.parent.def.maxSeverity);


            GenExplosion.DoExplosion(
                center: parent.pawn.Position,
                map: parent.pawn.Map,
                radius: Props.scaleRadiusWithSeverity ? scaledRadius : Props.explosionRadius,
                damType: Props.explosionDamageDef != null ? Props.explosionDamageDef : DamageDefOf.Bomb,
                instigator: parent.pawn,
                damAmount: Props.scaleDamageWithSeverity ? Mathf.RoundToInt(scaledSeverityDamage) : Props.explosionDamage,
                armorPenetration: -1f,
                explosionSound: null,
                weapon: null,
                projectile: null,
                intendedTarget: null,
                postExplosionSpawnThingDef: Props.postExplosionSpawnThingDef,
                postExplosionSpawnChance: Props.postExplosionSpawnChance,
                postExplosionSpawnThingCount: Props.postExplosionSpawnThingCount,
                applyDamageToExplosionCellsNeighbors: Props.applyDamageToExplosionCellsNeighbors,
                preExplosionSpawnThingDef: Props.preExplosionSpawnThingDef,
                preExplosionSpawnChance: Props.preExplosionSpawnChance,
                preExplosionSpawnThingCount: Props.preExplosionSpawnThingCount,
                chanceToStartFire: 0.0f,
                damageFalloff: Props.damageFalloff
            );

            parent.pawn.health.RemoveHediff(parent);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 0);
            Scribe_Values.Look(ref lastMoteSpawnTick, "lastMoteSpawnTick", -1);
        }
        public override string CompDebugString()
        {
            return "Time until explosion: " + (ticksRemaining / 60f).ToString("0.0") + " seconds";
        }
    }
}