using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class DamageWorker_CleaveDamage : DamageWorker
    {
        public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            if (thing != null && thing.Map != null)
            {
                List<Thing> targetsInCleaveRange = GenRadial.RadialDistinctThingsAround(thing.Position, thing.Map, 2, true).Where(x => FriendlyFireSettings.HostileOnly().CanTargetThing(x, dinfo.Instigator.Faction)).ToList();

                DamageInfo cleaveDamage = new DamageInfo(dinfo);
                cleaveDamage.SetAmount(cleaveDamage.Amount * 0.2f);
                foreach (var item in targetsInCleaveRange)
                {
                    item.TakeDamage(cleaveDamage);
                }
            }
            return base.Apply(dinfo, thing);
        }
    }
}
