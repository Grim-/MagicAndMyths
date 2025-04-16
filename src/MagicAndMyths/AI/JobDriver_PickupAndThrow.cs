using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class JobDriver_PickupAndThrow : JobDriver
    {
        public const int ThrowWorkAmount = 180;
        public Thing ThingToThrow => job.GetTarget(TargetIndex.A).Thing;
        public IntVec3 DestinationCell => job.GetTarget(TargetIndex.B).Cell;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(ThingToThrow, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            Toil rollCheckToil = new Toil();
            rollCheckToil.defaultCompleteMode = ToilCompleteMode.Instant;

            rollCheckToil.initAction = delegate
            {
                if (ThingToThrow == null)
                {
                    Log.Message("nothing to throw");
                    this.EndJobWith(JobCondition.Incompletable);
                }

                if (ThingToThrow is Pawn targetPawn)
                {
                    //failed to throw because didnt meet or beat other pawns strength roll
                    if (!DCUtility.ContestedStatCheck(pawn, targetPawn, MagicAndMythDefOf.Stat_Strength, out ContestedOutcome outcome))
                    {
                        Messages.Message(outcome.ToString(), MessageTypeDefOf.NegativeEvent);
                        Log.Message("failed strength check");
                        this.EndJobWith(JobCondition.Incompletable);
                    }
                }

            };

            yield return rollCheckToil;

           Toil pickupToil = new Toil();
            pickupToil.initAction = delegate
            {
                pawn.carryTracker.TryStartCarry(ThingToThrow, 1);
            };
            yield return pickupToil;


            Toil throwToil = new Toil();
            throwToil.defaultCompleteMode = ToilCompleteMode.Delay;
            throwToil.defaultDuration = ThrowWorkAmount;
            throwToil.WithProgressBarToilDelay(TargetIndex.A);
            throwToil.initAction = delegate
            {
                Job job = this.job;
                Thing thingToThrow = pawn.carryTracker.CarriedThing;
                if (thingToThrow == null)
                {
                    Log.Error(pawn + " threw null.");
                    return;
                }
                FleckMaker.ThrowDustPuff(pawn.DrawPos, pawn.Map, 0.3f);
            };
            throwToil.tickAction = delegate
            {
                pawn.rotationTracker.FaceCell(DestinationCell);
            };
            throwToil.AddFinishAction(delegate
            {
                Thing thingToThrow = pawn.carryTracker.CarriedThing;
                if (thingToThrow != null)
                {
                    pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out Thing droppedThing);
                    if (droppedThing != null)
                    {
                        ThingFlyer thingFlyer = ThingFlyer.MakeFlyer(
                            MagicAndMythDefOf.MagicAndMyths_ThingFlyer,
                            thing: droppedThing,
                            destCell :DestinationCell,
                            map : pawn.Map,
                            flightEffecterDef: null,
                            landingSound: null,
                            throwerPawn: pawn,
                            overrideStartVec: pawn.DrawPos);

                        ThingFlyer.LaunchFlyer(thingFlyer, droppedThing, pawn.Position, DestinationCell, pawn.Map);
                    }
                }
            });
            yield return throwToil;
        }
    }
}