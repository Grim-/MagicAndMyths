using System.Collections.Generic;
using UnityEngine;
using Verse;
using static Verse.DamageWorker;

namespace MagicAndMyths
{
    // Example comp: Chain lightning effect (OnKilled)
    public class DamageWorkerCompProperties_ChainOnKill : DamageWorkerCompProperties
    {
        public DamageDef chainDamage;
        public int chainDamageAmount = 10;
        public float chainRadius = 5f;
        public int maxTargets = 3;
        public float chance = 0.3f;

        public DamageWorkerCompProperties_ChainOnKill()
        {
            compClass = typeof(DamageWorkerComp_ChainOnKill);
        }
    }

    public class DamageWorkerComp_ChainOnKill : DamageWorkerComp
    {
        DamageWorkerCompProperties_ChainOnKill Props => (DamageWorkerCompProperties_ChainOnKill)props;

        public override bool ShouldApply(DamageInfo dinfo, Thing thing)
        {
            return Rand.Chance(Props.chance);
        }

        public override void OnKilled(DamageInfo dinfo, Thing thing, DamageResult result)
        {
            if (!thing.Spawned || Props.chainDamage == null) return;

            var targets = new List<Thing>();
            foreach (Thing t in GenRadial.RadialDistinctThingsAround(thing.Position, thing.Map, Props.chainRadius, true))
            {
                if (t != thing && t != dinfo.Instigator && (t is Pawn || t.def.useHitPoints))
                {
                    targets.Add(t);
                }
            }

            targets.Shuffle();
            int hitCount = Mathf.Min(targets.Count, Props.maxTargets);

            for (int i = 0; i < hitCount; i++)
            {
                DamageInfo chainDinfo = new DamageInfo(
                    Props.chainDamage,
                    Props.chainDamageAmount,
                    0f,
                    -1f,
                    dinfo.Instigator,
                    null,
                    dinfo.Weapon
                );
                targets[i].TakeDamage(chainDinfo);
            }
        }
    }
}