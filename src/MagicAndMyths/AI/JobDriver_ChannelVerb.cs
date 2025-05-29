using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class JobDriver_ChannelVerb : JobDriver
    {
        protected Verb_Channeled Verb_Channeled => (Verb_Channeled)this.pawn.jobs.curJob.verbToUse;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);

            yield return Toils_Combat.GotoCastPosition(TargetIndex.A, TargetIndex.B, false, 1f);
            yield return Toils_General.StopDead();

            Toil castToil = CastChanneledVerb(true);

            castToil.FailOn((x) =>
            {
                return Verb_Channeled == null || !Verb_Channeled.CanChannel(TargetA);
            });

            yield return castToil;

            yield break;
        }


        public Toil CastChanneledVerb(bool canHitNonTargetPawns = true)
        {
            Toil toil = ToilMaker.MakeToil("CastChanneledVerb");
            toil.initAction = delegate ()
            {
                if (!Verb_Channeled.TryStartCastOn(TargetA, TargetA, false, canHitNonTargetPawns, toil.actor.jobs.curJob.preventFriendlyFire, false))
                {
                    this.EndJobWith(JobCondition.Incompletable);
                }
                else
                {
                    Verb_Channeled.OnChannelStart(TargetA);
                }       
            };
            toil.handlingFacing = true;

            toil.tickAction = () =>
            {
                if (Verb_Channeled != null)
                {
                    Verb_Channeled.OnChannelJobTick(TargetA);
                }

                toil.actor.rotationTracker.FaceTarget(TargetA);
            };

            toil.AddFinishAction(() =>
            {
                if (Verb_Channeled != null)
                {
                    Verb_Channeled.OnChannelEnd(TargetA);
                };
            });
            toil.AddEndCondition(() =>
            {
                if (Verb_Channeled == null)
                {
                    return JobCondition.Incompletable;
                }

                if (!Verb_Channeled.CanChannel(TargetA))
                {
                    return JobCondition.Succeeded;
                }

                return JobCondition.Ongoing;
            });
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.activeSkill = (() => Toils_Combat.GetActiveSkillForToil(toil));
            return toil;
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
    }


}