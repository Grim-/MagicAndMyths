using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class JobDriver_Jump : JobDriver
    {
        public const int JumpWorkAmount = 180;
        public IntVec3 DestinationCell => job.GetTarget(TargetIndex.A).Cell;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);

            Toil rollCheckToil = new Toil();
            rollCheckToil.defaultCompleteMode = ToilCompleteMode.Instant;
            rollCheckToil.initAction = delegate
            {
                if (!DestinationCell.Walkable(pawn.Map))
                {
                    Log.Message("Invalid jump destination");
                    this.EndJobWith(JobCondition.Incompletable);
                }

            };

            yield return rollCheckToil;

            Toil jumpToil = new Toil();
            jumpToil.defaultCompleteMode = ToilCompleteMode.Delay;
            jumpToil.defaultDuration = JumpWorkAmount;
            jumpToil.WithProgressBarToilDelay(TargetIndex.A);
            jumpToil.initAction = delegate
            {
                FleckMaker.ThrowDustPuff(pawn.DrawPos, pawn.Map, 0.3f);
            };
            jumpToil.tickAction = delegate
            {
                pawn.rotationTracker.FaceCell(DestinationCell);
            };

            jumpToil.AddFinishAction(delegate
            {
                Comp_PawnJumpActions jumpComp = pawn.GetComp<Comp_PawnJumpActions>();

                ThingFlyer thingFlyer = ThingFlyer.MakeFlyer(
                    MagicAndMythDefOf.MagicAndMyths_ThingFlyer, 
                    thing: pawn,
                    destCell: DestinationCell,
                    map: pawn.Map,
                    flightEffecterDef: null,
                    landingSound: null,
                    throwerPawn: null, 
                    overrideStartVec: pawn.DrawPos);

                ThingFlyer.LaunchFlyer(thingFlyer, pawn, DestinationCell, pawn.Map);
            });
            yield return jumpToil;
        }
    }
}