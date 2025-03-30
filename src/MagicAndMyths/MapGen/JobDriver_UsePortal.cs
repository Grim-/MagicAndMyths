using System.Collections.Generic;
using Verse.AI;

namespace MagicAndMyths
{
    public class JobDriver_UsePortal : JobDriver
    {
        private const TargetIndex PortalInd = TargetIndex.A;
        private const int WaitTicks = 60;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Building_PortalGate portal = (Building_PortalGate)job.targetA.Thing;

            yield return Toils_Goto.GotoThing(PortalInd, PathEndMode.InteractionCell)
                .FailOnDespawnedOrNull(PortalInd);


            Toil waitAtPortal = Toils_General.Wait(WaitTicks)
                .FailOnDespawnedOrNull(PortalInd)
                .FailOnCannotTouch(PortalInd, PathEndMode.InteractionCell)
                .WithProgressBarToilDelay(PortalInd);


            waitAtPortal.tickAction = delegate
            {

            };

            yield return waitAtPortal;

            Toil teleport = new Toil();
            teleport.initAction = delegate
            {
                Building_PortalGate targetPortal = (Building_PortalGate)job.targetA.Thing;
                if (targetPortal == null || !targetPortal.Spawned)
                {
                    return;
                }
                targetPortal.TeleportPawn(pawn);
            };
            teleport.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return teleport;
        }
    }
}
