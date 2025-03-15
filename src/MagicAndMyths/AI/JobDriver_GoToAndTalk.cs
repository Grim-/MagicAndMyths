using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class JobDriver_GoToAndTalk : JobDriver
    {

        private Pawn Talker => this.pawn;
        private Pawn TalkTarget => TargetPawnA;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (Current.Game.CurrentMap.reservationManager.CanReserve(this.pawn, TalkTarget))
            {
                Current.Game.CurrentMap.reservationManager.Reserve(this.pawn, this.job, TalkTarget);
                return true;
            }

            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch, false);
            yield return gotoToil;

            int tickCount = 0;

            Toil waitFor = Toils_General.Wait(300, TargetIndex.A);
            waitFor.tickAction = () =>
            {
                tickCount++;
                if (tickCount == 60)
                {
                    tickCount = 0;

                    if (Rand.Bool)
                    {
                        MoteMaker.ThrowText(TalkTarget.DrawPos, TalkTarget.Map, $"HAHAHAHAH");
                    }
                    else
                    {
                        MoteMaker.ThrowText(TalkTarget.DrawPos, TalkTarget.Map, $"KILL");
                    }
                }
            };

            yield return waitFor;

            Toil startQuest = new Toil();
            startQuest.FailOnDestroyedOrNull(TargetIndex.A);

            startQuest.initAction = () =>
            {
                DKUtil.StartDKQuest(this.pawn);
            };
            startQuest.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return startQuest;
        }
    }

}
