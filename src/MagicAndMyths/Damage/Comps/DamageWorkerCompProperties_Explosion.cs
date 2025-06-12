using RimWorld;
using Verse;
using static Verse.DamageWorker;

namespace MagicAndMyths
{
    // Example comp: Explosion on damage
    public class DamageWorkerCompProperties_Explosion : DamageWorkerCompProperties
    {
        public float explosionRadius = 2f;
        public DamageDef explosionDamage;
        public int explosionDamageAmount = 10;
        public float chance = 0.1f;

        public DamageWorkerCompProperties_Explosion()
        {
            compClass = typeof(DamageWorkerComp_Explosion);
        }
    }

    public class DamageWorkerComp_Explosion : DamageWorkerComp
    {
        DamageWorkerCompProperties_Explosion Props => (DamageWorkerCompProperties_Explosion)props;

        public override bool ShouldApply(DamageInfo dinfo, Thing thing)
        {
            return Rand.Chance(Props.chance);
        }

        public override DamageResult PostApply(DamageInfo dinfo, Thing thing, DamageResult result)
        {
            if (thing.Spawned)
            {
                GenExplosion.DoExplosion(
                    thing.Position,
                    thing.Map,
                    Props.explosionRadius,
                    Props.explosionDamage ?? DamageDefOf.Bomb,
                    dinfo.Instigator,
                    Props.explosionDamageAmount
                );
            }
            return result;
        }
    }
}