using System.Collections.Generic;
using Verse;
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
            this.FailOn(() =>
            {
                Thing portalThing = job.GetTarget(PortalInd).Thing;
                IPortalProvider portalProvider = GetPortalProvider(portalThing);
                return portalProvider == null || !portalProvider.IsPortalActive;
            });

            yield return Toils_Goto.GotoThing(PortalInd, PathEndMode.InteractionCell)
                .FailOnDespawnedOrNull(PortalInd);

            Toil waitAtPortal = Toils_General.Wait(WaitTicks)
                .FailOnDespawnedOrNull(PortalInd)
                .FailOnCannotTouch(PortalInd, PathEndMode.InteractionCell)
                .WithProgressBarToilDelay(PortalInd);

            waitAtPortal.tickAction = delegate
            {
                //if (pawn.IsHashIntervalTick(15))
                //{
                //    MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_MetaPoof);
                //}
            };

            yield return waitAtPortal;

            Toil teleport = new Toil();
            teleport.initAction = delegate
            {
                Thing portalThing = job.GetTarget(PortalInd).Thing;
                if (portalThing == null || !portalThing.Spawned)
                {
                    return;
                }

                IPortalProvider portalProvider = GetPortalProvider(portalThing);
                if (portalProvider != null)
                {
                    portalProvider.TeleportPawn(pawn);
                }
            };

            teleport.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return teleport;
        }

        private IPortalProvider GetPortalProvider(Thing thing)
        {
            if (thing is IPortalProvider thingPortal)
            {
                return thingPortal;
            }

            CompPortalGenerator portalComp = thing.TryGetComp<CompPortalGenerator>();
            if (portalComp != null)
            {
                return portalComp;
            }

            return null;
        }
    }
}
