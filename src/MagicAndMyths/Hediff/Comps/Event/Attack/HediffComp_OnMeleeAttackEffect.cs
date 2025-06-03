using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public abstract class HediffComp_OnMeleeAttackEffect : HediffComp
    {
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            EventManager.Instance.OnBeforeMeleeDamageInfo += Instance_OnBeforeMeleeDamageInfo;
            EventManager.Instance.OnVerbUsed += Instance_OnVerbUsed;
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            EventManager.Instance.OnBeforeMeleeDamageInfo -= Instance_OnBeforeMeleeDamageInfo;
            EventManager.Instance.OnVerbUsed -= Instance_OnVerbUsed;
        }

        protected bool Instance_OnBeforeMeleeDamageInfo(Pawn attacker, LocalTargetInfo target, ref DamageInfo damageInfo)
        {
            if (attacker != this.parent.pawn)
            {
                return false;
            }

            OnBeforeMeleeDamage(attacker, target, ref damageInfo);
            return true;
        }

  

        private void Instance_OnVerbUsed(Pawn arg1, Verb arg2)
        {
            if (arg1 == this.parent.pawn)
            {
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

        protected virtual void OnBeforeMeleeDamage(Pawn attacker, LocalTargetInfo target, ref DamageInfo damageInfo)
        {

        }
    }
}