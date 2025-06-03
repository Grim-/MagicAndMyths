using RimWorld;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_AbilityDamageInRadius : CompProperties_AbilityEffect
    {
        public float radius = 2f;
        public FloatRange damage;
        public DamageDef damageDef;

        public FriendlyFireSettings friendlyFireParms = FriendlyFireSettings.HostileOnly();

        public CompProperties_AbilityDamageInRadius()
        {
            compClass = typeof(CompAbilityEffect_AbilityDamageInRadius);
        }
    }
    public class CompAbilityEffect_AbilityDamageInRadius : CompAbilityEffect
    {
        public new CompProperties_AbilityDamageInRadius Props => (CompProperties_AbilityDamageInRadius)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = this.parent.pawn.Map;
            IntVec3 startcell = this.parent.pawn.Position;
            IntVec3 targetCell = target.Cell;

            StageVisualEffect.CreateRadialStageEffect(startcell, Props.radius, map, 3, (IntVec3 cell, Map targetMap, int currentSection) =>
            {
                Pawn pawn = cell.GetFirstPawn(map);

                if (pawn != null && pawn != this.parent.pawn)
                {
                    pawn.TakeDamage(new DamageInfo(Props.damageDef, Props.damage.RandomInRange));
                }
            });
        }


        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);

            GenDraw.DrawFieldEdges(GenRadial.RadialCellsAround(this.parent.pawn.Position, Props.radius, true).ToList());
        }
    }
}
