using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_Throwable : CompProperties
    {
        public FleckDef impactFleck;
        public float impactFleckScale = 1f;
        public bool spawnImpactFleck = true;

        public ThingDef impactMote;
        public float impactMoteScale = 1f;
        public bool spawnImpactMote = false;


        public FloatRange baseImpactDamage = new FloatRange(1, 1);
        public DamageDef impactDamageDef;

        public CompProperties_Throwable()
        {
            compClass = typeof(Comp_Throwable);
        }
    }

    public class Comp_Throwable : ThingComp, IThrowableThing
    {
        CompProperties_Throwable Props => (CompProperties_Throwable)props;

        protected virtual FleckDef ActualImpactFleck => Props.impactFleck != null ? Props.impactFleck : FleckDefOf.DustPuffThick;
        protected virtual ThingDef ActualMoteDef => Props.impactMote != null ? Props.impactMote : ThingDefOf.Mote_IncineratorBurst;

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

        public virtual void OnLanded(IntVec3 position, Map map, Pawn throwingPawn)
        {
            MakeImpactEffect(position, map, Color.white);
        }

        public virtual void OnImpactedThing(IntVec3 position, Map map, Pawn throwingPawn, Thing impactedThing)
        {
            ThrowUtility.ApplyDefaultThrowImpactThingBehavior(throwingPawn, parent, position, map, impactedThing);
        }


        protected void MakeImpactEffect(IntVec3 position, Map map, Color? color = null)
        {
            if (ActualImpactFleck != null && Props.spawnImpactFleck)
            {
                FleckMaker.ThrowDustPuff(position, map, Props.impactFleckScale);
            }

            if (ActualMoteDef != null && Props.spawnImpactMote)
            {
                MoteMaker.MakeStaticMote(position, map, ActualMoteDef, Props.impactMoteScale);
            }

        }
    }


}