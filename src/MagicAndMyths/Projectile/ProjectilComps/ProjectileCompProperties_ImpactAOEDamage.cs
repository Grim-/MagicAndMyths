using RimWorld;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class ProjectileCompProperties_ImpactAOEDamage : ProjectileCompProperties
    {
        public DamageDef damageDef;
        public FloatRange damageAmount = new FloatRange(10f, 10f);
        public FloatRange radius = new FloatRange(3f, 3f);

        public ProjectileCompProperties_ImpactAOEDamage()
        {
            compClass = typeof(ProjectileComp_ImpactAOEDamage);
        }
    }

    public class ProjectileComp_ImpactAOEDamage : ProjectileComp
    {
        public ProjectileCompProperties_ImpactAOEDamage Props => (ProjectileCompProperties_ImpactAOEDamage)props;

        public override void PreImpact(Thing hitThing, bool blockedByShield)
        {
            if (blockedByShield)
                return;

            StageVisualEffect.CreateRadialStageEffect(this.parent.Position, Props.radius.RandomInRange, this.parent.Map, 4, (IntVec3 cell, Map map, int currentSection) =>
            {
                EffecterDefOf.Deflect_General.SpawnMaintained(cell, map);

                foreach (var item in cell.GetThingList(map).Where(x => x.def.useHitPoints || x is Pawn).ToList())
                {
                    DamageInfo damage = new DamageInfo(Props.damageDef != null ? Props.damageDef : DamageDefOf.Bomb, 10, 1);
                    item.TakeDamage(damage);
                }
            });
        }
    }
}
