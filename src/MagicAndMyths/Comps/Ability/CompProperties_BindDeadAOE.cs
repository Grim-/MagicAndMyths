using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_BindDeadAOE : CompProperties_AbilityEffect
    {
        public float radius = 15f;

        public CompProperties_BindDeadAOE()
        {
            compClass = typeof(CompAbilityEffect_BindDeadAOE);
        }
    }
    public class CompAbilityEffect_BindDeadAOE : CompAbilityEffect
    {
        new CompProperties_BindDeadAOE Props => (CompProperties_BindDeadAOE)props;
        Hediff_UndeadMaster master;


        public override void Initialize(AbilityCompProperties props)
        {
            base.Initialize(props);
            master = (Hediff_UndeadMaster)this.parent.pawn.health.GetOrAddHediff(MagicAndMythDefOf.DeathKnight_UndeadMaster);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            List<Thing> thingsInRadius = GenRadial.RadialDistinctThingsAround(this.parent.pawn.Position, this.parent.pawn.Map, Props.radius, true).ToList();


            List<Pawn> rere = new List<Pawn>();

            foreach (var thing in thingsInRadius)
            {
                if (thing is Corpse corwpse && corwpse.InnerPawn != null)
                {
                    Pawn deadPawn = corwpse.InnerPawn;
                    if (ResurrectionUtility.TryResurrect(deadPawn))
                    {
                        IntVec3 position = thing.Position;
                        master.StoreCreature(deadPawn);
                        master.SummonCreature(deadPawn, position);
                    }
                }
            }
        }
    }

}
