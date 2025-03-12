using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class JobDriver_GoToAndTalk : JobDriver
    {
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
            gotoToil.initAction = () =>
            {
                TalkTarget.stances.stunner.StunFor(500, this.pawn, false, false, false);
               TalkTarget.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Wait_MaintainPosture, this.pawn), JobCondition.InterruptForced);
            };
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

            Toil speak = new Toil();
            tickCount = 0;
            speak.tickAction = () =>
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

            speak.FailOnDestroyedOrNull(TargetIndex.A);

            speak.defaultDuration = 300;
            speak.defaultCompleteMode = ToilCompleteMode.Delay;
            yield return speak;


            Toil startQuest = new Toil();
            startQuest.FailOnDestroyedOrNull(TargetIndex.A);

            startQuest.initAction = () =>
            {
                TalkTarget.stances.stunner.StopStun();
                 Slate newSlate = new Slate();
                newSlate.Set<string>("colonistQuestSubject", this.pawn.ThingID);
                newSlate.Set<string>("colonistQuestSubjectName", this.pawn.Label);


                QuestUtility.GenerateQuestAndMakeAvailable(MagicAndMythDefOf.Quest_DeathKnightStartingPath, newSlate);
            };
            startQuest.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return startQuest;
        }
    }

}
