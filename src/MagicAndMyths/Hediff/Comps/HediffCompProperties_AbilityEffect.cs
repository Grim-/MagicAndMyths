using Verse;

namespace MagicAndMyths
{
    public abstract class HediffCompProperties_AbilityEffect : HediffCompProperties
    {
        public HediffCompProperties_AbilityEffect()
        {
        }
    }

    public abstract class HediffComp_AbilityEffect : HediffComp
    {
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            EventManager.Instance.OnAbilityCompleted += EventManager_OnAbilityCompleted;
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            EventManager.Instance.OnAbilityCompleted -= EventManager_OnAbilityCompleted;
        }

        private void EventManager_OnAbilityCompleted(Pawn arg1, RimWorld.Ability arg2)
        {
            if (arg1 != null && arg1 == this.parent.pawn)
            {
                OnAbilityUsed(arg1, arg2);
            }
        }

        protected abstract void OnAbilityUsed(Pawn pawn, RimWorld.Ability ability);
    }
}