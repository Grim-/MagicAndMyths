using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public abstract class HediffComp_OnMeleeAttackEffect : HediffComp
    {
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            EventManager.Instance.OnVerbUsed += Instance_OnVerbUsed;
        }

        private void Instance_OnVerbUsed(Pawn arg1, Verb arg2)
        {
            if (arg1 == this.parent.pawn)
            {

                Log.Message($"{this.parent.pawn.Label} used {arg2.GetType()}");

                if (arg2 is Verb_MeleeAttackDamage meleeAttackVerb)
                {
                    if (meleeAttackVerb.CurrentTarget != null)
                    {
                        OnMeleeAttack(meleeAttackVerb, meleeAttackVerb.CurrentTarget);
                    }
                }
          
            }
        }


        protected virtual void OnMeleeAttack(Verb_MeleeAttackDamage MeleeAttackVerb, LocalTargetInfo Target)
        {

        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            EventManager.Instance.OnVerbUsed -= Instance_OnVerbUsed;
        }
    }
}