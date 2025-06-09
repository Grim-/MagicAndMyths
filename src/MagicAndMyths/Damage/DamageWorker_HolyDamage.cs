using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class DamageWorker_HolyDamage : DamageWorker_AddInjury
    {
        public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            Pawn pawn = thing as Pawn;
            if (pawn != null)
            {
                if (IsUndead(pawn))
                {
                    dinfo.SetAmount(dinfo.Amount * 3f);
                    return base.Apply(dinfo, thing);
                }
                else
                {
                    Hediff censure = pawn.health.GetOrAddHediff(def.hediff, null, dinfo);
                    censure.Severity += Mathf.Min(dinfo.Amount * 0.02f, 0.3f);

                    DamageWorker.DamageResult result = new DamageWorker.DamageResult();
                    result.totalDamageDealt = 0f;
                    return result;
                }
            }
            return base.Apply(dinfo, thing);
        }

        //TODO
        private bool IsUndead(Pawn pawn)
        {
            return false;
        }
    }
}
