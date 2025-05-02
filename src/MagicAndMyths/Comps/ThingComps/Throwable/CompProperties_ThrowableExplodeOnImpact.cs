using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThrowableExplodeOnImpact : CompProperties_Throwable
    {
        public DamageDef damageDef;
        public FloatRange damageAmount = new FloatRange(10, 10);

        public CompProperties_ThrowableExplodeOnImpact()
        {
            compClass = typeof(Comp_ThrowableExplodeOnImpact);
        }
    }
    public class Comp_ThrowableExplodeOnImpact : Comp_Throwable
    {
        CompProperties_ThrowableExplodeOnImpact Props => (CompProperties_ThrowableExplodeOnImpact)props;

        public override void OnRespawn(IntVec3 position, Thing thing, Map map, Pawn throwingPawn)
        {
            base.OnRespawn(position, thing, map, throwingPawn);
            GenExplosion.DoExplosion(
            position,
            map,
            Props.radius,
            Props.damageDef != null ? Props.damageDef : DamageDefOf.Bomb,
            throwingPawn,
            Mathf.RoundToInt(Props.damageAmount.RandomInRange));
        }
    }



}