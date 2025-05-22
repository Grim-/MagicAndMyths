using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_AbilityTargetPositionSwap : CompProperties_AbilityEffect
    {
        public int delayTicks = 100;
        public EffecterDef effecter;

        public CompProperties_AbilityTargetPositionSwap()
        {
            compClass = typeof(CompAbilityEffect_AbilityTargetPositionSwap);
        }
    }

    public class CompAbilityEffect_AbilityTargetPositionSwap : CompAbilityEffect
    {
        public new CompProperties_AbilityTargetPositionSwap Props => (CompProperties_AbilityTargetPositionSwap)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            Pawn caster = parent.pawn;
            Map map = caster.Map;

            if (target.Thing != null && !(target.Thing is Building building))
            {
                IntVec3 casterPosition = caster.Position;
                IntVec3 targetPosition = target.Thing.Position;

                if (Props.effecter != null)
                {
                    Props.effecter.Spawn(casterPosition, map);
                    Props.effecter.Spawn(targetPosition, map);
                }

                caster.Position = targetPosition;
                caster.Notify_Teleported(false, true);
                target.Thing.Position = casterPosition;
                //target.Thing.Notify_Teleported(false, true);
            }
        }
    }
}
