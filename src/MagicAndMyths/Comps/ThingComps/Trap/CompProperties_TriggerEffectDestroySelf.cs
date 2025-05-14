using Verse;

namespace MagicAndMyths
{
    public class CompProperties_TriggerEffectDestroySelf : CompProperties_TriggerBase
    {
        public CompProperties_TriggerEffectDestroySelf()
        {
            compClass = typeof(CompTrap_TriggerEffectDestroySelf);
        }
    }

    public class CompTrap_TriggerEffectDestroySelf : Comp_TriggerBase
    {
        private CompProperties_TriggerEffectDestroySelf Props => (CompProperties_TriggerEffectDestroySelf)props;

        public override void Trigger(Pawn pawn)
        {
            base.Trigger(pawn);
            this.parent.Destroy();
        }
    }

}