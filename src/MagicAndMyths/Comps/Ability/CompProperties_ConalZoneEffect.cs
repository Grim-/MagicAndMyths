using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ConalZoneEffect : CompProperties_AbilityZoneEffect
    {
        public int length = 10;
        public float angle = 90f;

        public CompProperties_ConalZoneEffect()
        {
            compClass = typeof(CompAbilityEffect_ConalZoneEffect);
        }
    }

    public class CompAbilityEffect_ConalZoneEffect : AbilityZoneEffect
    {
        public CompProperties_ConalZoneEffect Props => (CompProperties_ConalZoneEffect)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            ActiveZone zone = SpawnZone(this.parent.pawn.Position, TargetUtil.GetCellsInCone(this.parent.pawn.Position, target.Cell, Props.length, Props.angle), this.parent.pawn.Map);
            zone.SetDamage(DamageDefOf.Cut, new FloatRange(1, 5));
        }


        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);
            GenDraw.DrawFieldEdges(TargetUtil.GetCellsInCone(this.parent.pawn.Position, target.Cell, Props.length, Props.angle), Color.cyan);
        }
    }
}
