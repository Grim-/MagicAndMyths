using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{

    public class HediffCompProperties_BioShield : HediffCompProperties_EnergyComp
    {
        public float energyCostPerDamage = 2f;
        public float damageReductionFactor = 1f;

        public List<DamageDef> damageTypes;
        public HediffCompProperties_BioShield()
        {
            compClass = typeof(HediffComp_BioShield);
        }
    }
    public class HediffComp_BioShield : HediffComp_EnergyComp
    {
        new public HediffCompProperties_BioShield Props => (HediffCompProperties_BioShield)props;


        public bool CanMitigate(DamageInfo dInfo)
        {
            return Props.damageTypes == null ? true : Props.damageTypes.Any(x => x == dInfo.Def);
        }

        public float MitigateDamage(DamageInfo dInfo)
        {
            return dInfo.Amount * Props.damageReductionFactor;
        }

        public float EnergyCost(float dInfo)
        {
            return Mathf.Min(1f, dInfo * Props.energyCostPerDamage);
        }
    }
}