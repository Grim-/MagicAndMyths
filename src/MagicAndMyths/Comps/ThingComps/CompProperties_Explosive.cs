using RimWorld;
using System.Collections.Generic;
using System.Linq;
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

            List<IntVec3> cells = GenRadial.RadialCellsAround(this.parent.Position, Props.radius, true).ToList();

            StageVisualEffect.CreateStageEffect(cells, this.parent.Map, 8, (IntVec3 cell, Map targetMap, int currentSection) =>
            {
                EffecterDefOf.ImpactSmallDustCloud.Spawn(cell, targetMap);

                List<Thing> things = cell.GetThingList(targetMap).ToList();

                foreach (var t in things)
                {
                    if (t is Pawn || t is Building building)
                    {
                        DamageInfo damage = new DamageInfo(Props.damageDef != null ? Props.damageDef : DamageDefOf.Bomb, Mathf.RoundToInt(Props.damageAmount.RandomInRange), 1);
                        if (t.def.mineable)
                        {
                            damage = new DamageInfo(DamageDefOf.Mining, Mathf.RoundToInt(Props.damageAmount.RandomInRange) * 12, 1);
                        }
                        t.TakeDamage(damage);
                    }
                }

            }, 5);

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