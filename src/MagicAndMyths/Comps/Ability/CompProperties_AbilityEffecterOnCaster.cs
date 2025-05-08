using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_AbilityEffecterOnCaster : CompProperties_AbilityEffect
    {
        public EffecterDef effecterDef;
        public int maintainForTicks = -1;
        public float scale = 1f;

        public CompProperties_AbilityEffecterOnCaster()
        {
            compClass = typeof(CompAbilityEffect_AbilityEffecterOnCaster);
        }
    }

    public class CompAbilityEffect_AbilityEffecterOnCaster : CompAbilityEffect
    {
        public new CompProperties_AbilityEffecterOnCaster Props => (CompProperties_AbilityEffecterOnCaster)this.props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Effecter effecter;
            effecter = this.Props.effecterDef.Spawn(this.parent.pawn.Position, this.parent.pawn.Map, this.Props.scale);
            if (this.Props.maintainForTicks > 0)
            {
                this.parent.AddEffecterToMaintain(effecter, target.Cell, this.Props.maintainForTicks, null);
                return;
            }
            effecter.Cleanup();
        }
    }
}
