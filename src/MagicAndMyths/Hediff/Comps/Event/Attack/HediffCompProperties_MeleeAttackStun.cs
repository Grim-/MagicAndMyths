using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_MeleeAttackStun : HediffCompProperties
    {
        public FloatRange chance;
        public IntRange stunTicks = new IntRange(200, 200);

        public HediffCompProperties_MeleeAttackStun()
        {
            compClass = typeof(HediffComp_MeleeAttackStun);
        }
    }

    public class HediffComp_MeleeAttackStun : HediffComp_OnMeleeAttackEffect
    {
        HediffCompProperties_MeleeAttackStun Props => (HediffCompProperties_MeleeAttackStun)props;
        protected override void OnMeleeAttack(Verb_MeleeAttackDamage MeleeAttackVerb, LocalTargetInfo Target)
        {
            base.OnMeleeAttack(MeleeAttackVerb, Target);

            if (Target.Pawn != null && Target.Pawn.stances?.stunner != null)
            {
                Target.Pawn.stances?.stunner.StunFor(Props.stunTicks.RandomInRange, this.parent.pawn);
            }
        }
    }
}