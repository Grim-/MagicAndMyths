using Verse;

namespace MagicAndMyths
{
    public class ReflectivePropertyWorker : ThingPropertyWorker
    {
        public override void OnThingDamageTaken(Thing target, DamageInfo info)
        {
            base.OnThingDamageTaken(target, info);

            if (info.Instigator != null && info.Instigator.Position.InHorDistOf(this.parent.Position, 1f))
            {
                DamageInfo reflectedDamage = new DamageInfo(info.Def, info.Amount, 0.2f);
                info.Instigator.TakeDamage(reflectedDamage);
            }

        }

        public override string GetDescription()
        {
            return "Any damage recieved in melee range is also reflected back onto the attacker";
        }
    }
}
