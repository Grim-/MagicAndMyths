using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class ProjectileCompProperties_ImpactStartFires : ProjectileCompProperties
    {
        public FloatRange radius = new FloatRange(3f, 3f);
        public FloatRange fireAmount = new FloatRange(1, 1);
        public IntRange sections = new IntRange(4, 4);
        public EffecterDef effecterDef = null;
        public int ticksBetweenSections = 15;

        public ProjectileCompProperties_ImpactStartFires()
        {
            compClass = typeof(ProjectileComp_ImpactStartFires);
        }
    }

    public class ProjectileComp_ImpactStartFires : ProjectileComp
    {
        public ProjectileCompProperties_ImpactStartFires Props => (ProjectileCompProperties_ImpactStartFires)props;

        public override void PreImpact(Thing hitThing, bool blockedByShield)
        {
            if (blockedByShield)
                return;

            StageVisualEffect.CreateRadialStageEffect(this.parent.Position, Props.radius.RandomInRange, this.parent.Map, Props.sections.RandomInRange, (IntVec3 cell, Map map, int currentSection) =>
            {
                if (Props.effecterDef != null)
                {
                    Props.effecterDef.Spawn(cell, map);
                }

                if (cell.IsValid)
                {
                    FireUtility.TryStartFireIn(cell, map, Props.fireAmount.RandomInRange, null);
                }
            }, Props.ticksBetweenSections);
        }
    }
}
