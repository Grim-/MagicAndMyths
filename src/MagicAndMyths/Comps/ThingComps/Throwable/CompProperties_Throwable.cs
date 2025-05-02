using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_Throwable : CompProperties
    {
        public EffecterDef impactEffectDef;
        public bool destroyOnThrow = true;

        public float radius = 1f;

        public FloatRange baseImpactDamage = new FloatRange(1, 1);
        public DamageDef impactDamageDef;

        public CompProperties_Throwable()
        {
            compClass = typeof(Comp_Throwable);
        }
    }

    public class Comp_Throwable : ThingComp, IThrowableThing
    {
        public CompProperties_Throwable Props => (CompProperties_Throwable)props;

        public bool IsThrowableAtAll => this.parent.def.building == null && !this.parent.def.IsBuildingArtificial && this.parent.GetType() != typeof(Pawn) && this.parent.def.mote == null;

        public virtual DamageDef ImpactDamageType
        {
            get
            {
                return ThrowUtility.GetImpactDamageDefFor(this.parent);
            }
        }

        public virtual void OnThrown(IntVec3 position, Map map, Pawn throwingPawn)
        {

        }

        public virtual void OnBeforeRespawn(IntVec3 position, Map map, Pawn throwingPawn)
        {
            MakeImpactEffect(position, map, Color.white);
        }


        public void Respawn(IntVec3 position, Thing thing, Map map, Pawn throwingPawn)
        {
            OnRespawn(position, thing, map, throwingPawn);
            PostRespawn();
        }

        public virtual void OnRespawn(IntVec3 position, Thing thing, Map map, Pawn throwingPawn)
        {
            
        }

        public virtual void PostRespawn()
        {
            if (Props.destroyOnThrow)
            {
                if (!this.parent.Destroyed)
                {
                    this.parent.Destroy();
                }
            }
        }

        protected void MakeImpactEffect(IntVec3 position, Map map, Color? color = null)
        {
            if (Props.impactEffectDef != null)
            {
                Props.impactEffectDef.Spawn(position, map);
            }

        }
    }


}