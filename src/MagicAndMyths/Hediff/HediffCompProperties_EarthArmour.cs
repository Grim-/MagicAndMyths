using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_EarthArmour : HediffCompProperties_DamageReduction
    {
        public int stacksToRemove = 1;
        public float minimumDamageToTrigger = 0f;

        public HediffCompProperties_EarthArmour()
        {
            compClass = typeof(HediffComp_EarthArmour);
        }
    }

    public class HediffComp_EarthArmour : HediffComp_DamageReduction
    {
        public new HediffCompProperties_EarthArmour Props => (HediffCompProperties_EarthArmour)props;

        protected override bool OnBeforeThingDamageTaken(ref DamageInfo dinfo)
        {
            bool damageBlocked = false;
            float originalDamage = dinfo.Amount;

            if (!Props.damageReductions.NullOrEmpty())
            {
                foreach (DamageReduction reduction in Props.damageReductions)
                {
                    if (reduction.damageDef == dinfo.Def)
                    {
                        float newAmount = originalDamage * reduction.damageFactor - reduction.flatReduction;
                        newAmount = Mathf.Max(0f, newAmount);

                        dinfo.SetAmount(newAmount);
                        damageBlocked = newAmount <= 0f;
                        break;
                    }
                }
            }

            if (ShouldRemoveStack(originalDamage, dinfo.Def))
            {
                if (parent is HediffWithStacks stackedHediff)
                {
                    stackedHediff.RemoveStack(Props.stacksToRemove);
                }
            }

            return damageBlocked;
        }

        private bool ShouldRemoveStack(float originalDamage, DamageDef damageDef)
        {
            if (originalDamage < Props.minimumDamageToTrigger)
            {
                return false;
            }

            if (Props.damageReductions.NullOrEmpty())
            {
                return false;
            }

            foreach (DamageReduction reduction in Props.damageReductions)
            {
                if (reduction.damageDef == damageDef)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
