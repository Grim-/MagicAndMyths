using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_Explosive : CompProperties
    {
        public float radius = 2f;
        public DamageDef damageDef;
        public List<DamageDef> detonatingDamageTypes;
        public FloatRange damageAmount = new FloatRange(10, 10);

        public CompProperties_Explosive()
        {
            compClass = typeof(Comp_Explosive);
        }
    }


    public class Comp_Explosive : ThingComp
    {
        CompProperties_Explosive Props => (CompProperties_Explosive)props;

        public void Detonate(Thing Instigator = null)
        {
            if (!this.parent.Spawned)
            {
                return;
            }

            GenExplosion.DoExplosion(
            this.parent.Position,
            this.parent.Map,
            Props.radius,
            Props.damageDef != null ? Props.damageDef : DamageDefOf.Bomb,
            Instigator,
            Mathf.RoundToInt(Props.damageAmount.RandomInRange));


            if (!this.parent.Destroyed)
            {
                this.parent.Destroy();
            }
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);

            if (Props.detonatingDamageTypes != null && Props.detonatingDamageTypes.Contains(dinfo.Def) || Props.detonatingDamageTypes == null)
            {
                Detonate(dinfo.Instigator);
            }
        }
    }
}