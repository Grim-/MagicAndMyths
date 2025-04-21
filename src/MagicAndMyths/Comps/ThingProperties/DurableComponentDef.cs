using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class DurableComponentDef : ThingPropertyDef
    {
        public float damageThreshold = 20f;
        public float minimumDamage = 1f;

        public DurableComponentDef()
        {
            workerClass = typeof(DurableComponentWorker);
        }
    }

    public class DurableComponentWorker : ThingPropertyWorker
    {

        public override void PostSpawnSetup(Thing thing, bool respawningAfterLoad)
        {
            base.PostSpawnSetup(thing, respawningAfterLoad);

            Log.Message("Durable post setup");
        }

        public override void PostPreApplyDamage(Thing thing, ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            DurableComponentDef durableDef = def as DurableComponentDef;
            if (durableDef != null && dinfo.Amount < durableDef.damageThreshold)
            {
                absorbed = true;

                dinfo.SetAmount(0);
                MoteMaker.ThrowText(parent.PositionHeld.ToVector3(), parent.Map, "(Durable) Resisted!", 1.9f);
            }
        }

        public override string GetDescription()
        {
            return "Negates all damage under a value";
        }
    }
}
