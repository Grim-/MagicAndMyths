using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class JobDriver_UnlockDoor : JobDriver
    {
        private const int UnlockDuration = 50;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => !(job.targetA.Thing is Building_LockableDoor));

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            Toil waitToil = new Toil();
            waitToil.defaultCompleteMode = ToilCompleteMode.Delay;
            waitToil.defaultDuration = UnlockDuration;
            waitToil.WithProgressBarToilDelay(TargetIndex.A);
            yield return waitToil;

            Toil unlockDoor = new Toil();
            unlockDoor.initAction = () =>
            {
                Pawn actor = unlockDoor.actor;
                Building_LockableDoor door = (Building_LockableDoor)job.targetA.Thing;

                if (door.TryFindAndConsumeKey(actor))
                {
                    door.Unlock();
                }
                else
                {
                    Messages.Message($"{actor.LabelCap} couldn't unlock {door.LabelCap} - key not found.", MessageTypeDefOf.RejectInput);
                }
            };
            unlockDoor.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return unlockDoor;
        }
    }
}
