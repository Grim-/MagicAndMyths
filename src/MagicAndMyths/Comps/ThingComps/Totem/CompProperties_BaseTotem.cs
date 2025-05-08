using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_BaseTotem : CompProperties
    {
        public int tickInterval = 500;
        public bool canTargetHostile = true;
        public bool canTargetFriendly = false;
        public bool canTargetNeutral = false;



        public CompProperties_BaseTotem()
        {
            compClass = typeof(Comp_BaseTotem);
        }
    }

    public class Comp_BaseTotem : ThingComp
    {
        public Building_Totem Parent => this.parent as Building_Totem;
        protected int tickCount = 0;
        protected CompProperties_BaseTotem Props => (CompProperties_BaseTotem)props;

        public override void CompTick()
        {
            base.CompTick();
            tickCount++;

            if (tickCount >= Props.tickInterval)
            {
                OnTotemTick();
                tickCount = 0;
            }

        }

        public virtual void OnTotemTick()
        {

        }


        protected virtual List<Pawn> GetPawnsInRange() 
        {
            return GenRadial.RadialDistinctThingsAround(this.parent.Position, this.parent.Map, Parent.EffectRadius, true)
                .Where(x => x is Pawn pawn && AOEUtil.ShouldTarget(pawn.Faction, Parent.owner.Faction, Props.canTargetHostile, Props.canTargetFriendly, Props.canTargetNeutral))
                .Cast<Pawn>()
                .ToList();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref tickCount, "tickCount");
        }
    }


}
