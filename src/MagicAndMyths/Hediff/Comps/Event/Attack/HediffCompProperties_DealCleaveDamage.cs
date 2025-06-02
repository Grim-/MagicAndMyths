using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_DealCleaveDamage : HediffCompProperties
    {
        public FloatRange damage;
        public DamageDef damageDef;

        public int length = 6;
        public FloatRange angle = new FloatRange(40, 45);

        public HediffCompProperties_DealCleaveDamage()
        {
            compClass = typeof(HediffComp_DealCleaveDamage);
        }
    }

    public class HediffComp_DealCleaveDamage : HediffComp_OnMeleeAttackEffect
    {
        HediffCompProperties_DealCleaveDamage Props => (HediffCompProperties_DealCleaveDamage)props;
        protected override void OnMeleeAttack(Verb_MeleeAttackDamage MeleeAttackVerb, LocalTargetInfo Target)
        {
            base.OnMeleeAttack(MeleeAttackVerb, Target);

            StageVisualEffect.CreateConalStageEffect(this.parent.pawn.Position, MeleeAttackVerb.CurrentTarget.Cell, Props.length, Props.angle.RandomInRange, this.parent.pawn.Map, 3, (IntVec3 position, Map map, int currentSection) =>
            {
                if (Rand.Value > 0.6f)
                {
                    EffecterDefOf.ImpactSmallDustCloud.Spawn(position, map);
                }

                List<Thing> damageAbleThings = position.GetThingList(map).Where(x => x.def.useHitPoints && x != this.parent.pawn).ToList();
                damageAbleThings.ForEach(x => x.TakeDamage(new DamageInfo(Props.damageDef, Props.damage.RandomInRange)));
            });
        }
    }
}