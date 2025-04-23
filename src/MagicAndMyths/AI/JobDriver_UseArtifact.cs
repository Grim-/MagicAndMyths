using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class JobDriver_UseArtifact : JobDriver
    {
        private int useDuration = -1;
        private Mote warmupMote;

        protected ThingWithComps TargetThing => (ThingWithComps)this.job.GetTarget(TargetIndex.A);
        protected Comp_Artifact ArtifactComp => TargetThing.TryGetComp<Comp_Artifact>();

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            this.useDuration = ArtifactComp.Props.useDuration;
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed) &&
                  (!this.job.targetB.IsValid || this.pawn.Reserve(this.job.targetB, this.job, 1, -1, null, errorOnFailed));
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            this.FailOn(() => !ArtifactComp.CanBeUsedNow(this.pawn));

            yield return Toils_Goto.GotoThing(TargetIndex.A, base.TargetThingA.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.Touch);

            Toil takeSingleItem = ToilMaker.MakeToil("TakeSingleItem");
            takeSingleItem.initAction = delegate ()
            {
                Thing targetThing = takeSingleItem.actor.CurJob.targetA.Thing;
                if (targetThing.stackCount > 1)
                {
                    Thing singleItem = targetThing.SplitOff(1);
                    IntVec3 spawnCell = targetThing.Position + GenAdj.CardinalDirections.RandomElement();
                    if (!spawnCell.InBounds(targetThing.Map) || spawnCell.Impassable(targetThing.Map))
                    {
                        spawnCell = CellFinder.StandableCellNear(targetThing.Position, targetThing.Map, 1);
                    }

                    GenSpawn.Spawn(singleItem, spawnCell, targetThing.Map);
                    takeSingleItem.actor.CurJob.SetTarget(TargetIndex.A, singleItem);
                }
            };
            yield return takeSingleItem;

            if (ArtifactComp.Props.moveToTarget && this.job.targetB.IsValid)
            {
                yield return Toils_Haul.StartCarryThing(TargetIndex.A);
                yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);

                Toil prepareWhileCarrying = ToilMaker.MakeToil("PrepareWhileCarrying");
                prepareWhileCarrying.defaultDuration = this.useDuration;
                prepareWhileCarrying.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
                prepareWhileCarrying.FailOnDespawnedNullOrForbidden(TargetIndex.B);
                prepareWhileCarrying.handlingFacing = true;
                prepareWhileCarrying.tickAction = delegate ()
                {
                    prepareWhileCarrying.actor.rotationTracker.FaceTarget(base.TargetB);
                };
                yield return prepareWhileCarrying;

                Toil useWhileCarrying = ToilMaker.MakeToil("UseWhileCarrying");
                useWhileCarrying.initAction = delegate ()
                {
                    Pawn actor = useWhileCarrying.actor;
                    Thing item = actor.carryTracker.CarriedThing;
                    LocalTargetInfo target = actor.CurJob.targetB;
                    if (item != null && item.TryGetComp<Comp_Artifact>() is Comp_Artifact comp)
                    {
                        comp.UseEffects(actor, target);
                    }
                };
                useWhileCarrying.defaultCompleteMode = ToilCompleteMode.Instant;
                yield return useWhileCarrying;
            }
            else
            {
                yield return this.PrepareToUse();
                yield return this.Use();
            }
        }

        protected Toil PrepareToUse()
        {
            Toil toil = Toils_General.Wait(this.useDuration, TargetIndex.A);
            toil.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            toil.FailOnCannotTouch(TargetIndex.A, base.TargetThingA.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.Touch);
            toil.handlingFacing = true;
            toil.tickAction = delegate ()
            {
                this.pawn.rotationTracker.FaceTarget(base.TargetA);
            };

            if (this.job.targetB.IsValid)
            {
                toil.FailOnDespawnedOrNull(TargetIndex.B);
                toil.FailOnDestroyedOrNull(TargetIndex.B);
            }
            return toil;
        }

        protected Toil Use()
        {
            Toil use = ToilMaker.MakeToil("Use");
            use.initAction = delegate ()
            {
                Pawn actor = use.actor;
                Thing item = actor.CurJob.targetA.Thing;
                LocalTargetInfo target = actor.CurJob.targetB;

                if (item != null && item.TryGetComp<Comp_Artifact>() is Comp_Artifact comp)
                {
                    comp.UseEffects(actor, target);
                }
            };
            use.defaultCompleteMode = ToilCompleteMode.Instant;
            return use;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.useDuration, "useDuration", 0);
        }
    }
}