using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_AbilityTeleport : CompProperties_AbilityEffect
    {
        public int delayTicks = 100;
        public EffecterDef originEffecter;
        public EffecterDef destinationEffecter;

        public CompProperties_AbilityTeleport()
        {
            compClass = typeof(CompAbilityEffect_Teleport);
        }
    }

    public class CompAbilityEffect_Teleport : CompAbilityEffect
    {
        public new CompProperties_AbilityTeleport Props => (CompProperties_AbilityTeleport)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            Pawn caster = parent.pawn;
            Map map = caster.Map;

            if (caster != null && map != null)
            {
                IntVec3 originPos = caster.Position;
                IntVec3 destPos = target.Cell;

                FancyTeleporter.Launch(
                    originPos,
                    map,
                    destPos,
                    map,
                    caster,
                    Props.delayTicks,
                    Props.originEffecter,
                    Props.destinationEffecter
                );
            }
        }
    }

}
