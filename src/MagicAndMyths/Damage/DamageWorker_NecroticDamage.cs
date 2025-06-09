using Verse;

namespace MagicAndMyths
{
    public class DamageWorker_NecroticDamage : DamageWorker_AddInjury
    {
        public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            return base.Apply(dinfo, thing);


            //TODO
            if (thing is Pawn pawn && pawn.Dead)
            {
                //add zombie hediff?

            }
        }
    }
}
