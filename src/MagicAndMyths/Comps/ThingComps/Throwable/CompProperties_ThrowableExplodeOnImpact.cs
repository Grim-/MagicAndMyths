using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThrowableExplodeOnImpact : CompProperties_Throwable
    {
        public float explosionRadius = 3f;
        public DamageDef damageDef;
        public FloatRange damageAmount = new FloatRange(10, 10);
        public bool destroyOnImpact = true;

        public CompProperties_ThrowableExplodeOnImpact()
        {
            compClass = typeof(Comp_ThrowableExplodeOnImpact);
        }
    }
    public class Comp_ThrowableExplodeOnImpact : Comp_Throwable
    {
        CompProperties_ThrowableExplodeOnImpact Props => (CompProperties_ThrowableExplodeOnImpact)props;


        public override void OnLanded(IntVec3 position, Map map, Pawn throwingPawn)
        {
            base.OnLanded(position, map, throwingPawn);

            GenExplosion.DoExplosion(
                position,
                map,
                Props.explosionRadius,
                Props.damageDef != null ? Props.damageDef : DamageDefOf.Bomb,
                throwingPawn,
                Mathf.RoundToInt(Props.damageAmount.RandomInRange));

            if (Props.destroyOnImpact)
            {
                this.parent.Destroy();
            }

        }
    }

}