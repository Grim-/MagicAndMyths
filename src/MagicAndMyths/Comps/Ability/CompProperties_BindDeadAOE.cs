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

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);

            GenDraw.DrawFieldEdges(AffectedCells(target.Cell));
        }

        private List<IntVec3> AffectedCells(IntVec3 origin)
        {
            return GenRadial.RadialCellsAround(origin, Props.radius, true).ToList();
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            List<Thing> thingsInRadius = GenRadial.RadialDistinctThingsAround(target.Cell, this.parent.pawn.Map, Props.radius, true).ToList();

            foreach (var thing in thingsInRadius)
            {
                if (thing is Corpse corwpse && corwpse.InnerPawn != null)
                {
                    Pawn deadPawn = corwpse.InnerPawn;
                    if (ResurrectionUtility.TryResurrect(deadPawn))
                    {
                        IntVec3 position = thing.Position;
                        master.SummonCreature(deadPawn, position);
                    }
                }
            }
        }
    }

}
