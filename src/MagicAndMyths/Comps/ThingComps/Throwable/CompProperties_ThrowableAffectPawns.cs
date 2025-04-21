using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThrowableAffectPawns : CompProperties_Throwable
    {
        public float radius = 3f;
        public bool destroyOnImpact = true;

        public CompProperties_ThrowableAffectPawns()
        {
            compClass = typeof(Comp_ThrowableAffectPawns);
        }
    }

    public class Comp_ThrowableAffectPawns : Comp_Throwable
    {
        public CompProperties_ThrowableAffectPawns Props => (CompProperties_ThrowableAffectPawns)props;

        public override void OnRespawn(IntVec3 position, Thing thing, Map map, Pawn throwingPawn)
        {
            base.OnRespawn(position, thing, map, throwingPawn);

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, Props.radius, true))
            {
                if (cell.InBounds(map))
                {
                    List<Thing> thingList = cell.GetThingList(map);
                    foreach (Thing t in thingList)
                    {
                        if (CanAffectThing(t, throwingPawn))
                        {
                            AffectThing(t, throwingPawn);
                        }
                    }
                }
            }

            if (Props.destroyOnImpact)
            {
                this.parent.Destroy();
            }
        }

        protected virtual void AffectThing(Thing thing, Pawn throwingPawn)
        {

        }


        protected virtual bool CanAffectThing(Thing thing, Pawn ThrowingPawn)
        {
            return thing != null && thing is Pawn;
        }
    }
}