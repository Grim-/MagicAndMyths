﻿using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MagicAndMyths
{
    [StaticConstructorOnStartup]
    public class LightningRingBehavior : Ticker
    {
        private List<LightningRingConfig> rings;
        private int currentRing = 0;
        private float explosionRadius = 1.9f;
        private IntVec3 center;
        private Map map;

        public LightningRingBehavior(List<LightningRingConfig> rings, IntVec3 center, Map map, int delayTicks)
            : base(delayTicks, null, null, true, rings.Count)
        {
            this.rings = rings;
            this.center = center;
            this.map = map;
            _OnTick = DoLightningRing;
        }

        public LightningRingBehavior()
        {
        }

        private void DoLightningRing()
        {
            if (currentRing >= rings.Count)
            {
                this.Stop();
                return;
            }
            SoundDefOf.Thunder_OffMap.PlayOneShotOnCamera(map);


            var ring = rings[currentRing];
            float angleStep = 360f / ring.Strikes;
            for (int i = 0; i < ring.Strikes; i++)
            {
                float angle = i * angleStep + Rand.Range(-10f, 10f);
                float rad = ring.Radius + Rand.Range(-0.5f, 0.5f);
                IntVec3 strikePos = center + GetStrikeOffset(angle, rad);
                LightningStrike.GenerateLightningStrike(map, strikePos, explosionRadius, out IEnumerable<IntVec3> affectedCells);
            }
            currentRing++;
        }




        private IntVec3 GetStrikeOffset(float angle, float radius)
        {
            int x = Mathf.RoundToInt(radius * Mathf.Cos(angle * Mathf.Deg2Rad));
            int z = Mathf.RoundToInt(radius * Mathf.Sin(angle * Mathf.Deg2Rad));
            return new IntVec3(x, 0, z);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentRing, "currentRing", 0);
            Scribe_Values.Look(ref center, "center");
            Scribe_References.Look(ref map, "map");
        }
    }
}
