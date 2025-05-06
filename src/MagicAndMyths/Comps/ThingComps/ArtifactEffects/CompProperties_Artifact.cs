using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace MagicAndMyths
{
    public class CompProperties_Artifact : CompProperties
    {
        public string useLabel = "Activate {0}";
        public JobDef useJob;
        public int useDuration = 100;
        public bool destroyOnUse = true;
        public SoundDef sound = null;

        //target
        public TargetingParameters targetParams;
        public bool moveToTarget = false;
        public bool requiresTarget = false;
        public bool targetSelf = false;

        public bool useCompTargetValidator = false;

        //charges
        public int charges = -1;
        public bool destroyOnChargesDepleted = true;
        public IntRange cooldownTickRange = new IntRange(2400, 2400);

        //coodlnw
        public bool hasCooldown = false;
        public bool cooldownRestoresCharges = false;
        public IntRange chargesRestoredPerCooldown = new IntRange(1, 1);


        public EffecterDef targetEffectDef = null;
        public EffecterDef userEffectDef = null;


        public ThoughtDef userUsedThought;
        public ThoughtDef targetUsedThought;
        public CompProperties_Artifact()
        {
            compClass = typeof(Comp_Artifact);
        }
    }

    public class Comp_Artifact : ThingComp
    {
        private int chargesRemaining;
        private int cooldownTicksRemaining;
        protected bool UsesCharges => Props.charges > 0;

        public CompProperties_Artifact Props => (CompProperties_Artifact)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                chargesRemaining = Props.charges;
                cooldownTicksRemaining = 0;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (cooldownTicksRemaining > 0)
            {
                cooldownTicksRemaining--;
                if (cooldownTicksRemaining <= 0 && Props.cooldownRestoresCharges)
                {
                    if (chargesRemaining < Props.charges)
                    {
                        chargesRemaining += Props.chargesRestoredPerCooldown.RandomInRange;
                    }
                }
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (parent.IsForbidden(selPawn) || !selPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
                yield break;

            if (chargesRemaining <= 0)
                yield break;

            if (cooldownTicksRemaining > 0)
            {
                yield return new FloatMenuOption(Props.useLabel.Translate(this.parent.LabelShort), null);
                yield break;
            }
            Action action = () =>
            {
                if (NeedsTargeting())
                {
                    Find.Targeter.BeginTargeting(Props.targetParams,
                        (LocalTargetInfo target) =>
                        {
                            StartJob(selPawn, target);
                        },
                         null, (LocalTargetInfo targetInfo) =>
                         {
                             if (Props.useCompTargetValidator)
                             {
                                 bool isValid = true;
                                 foreach (var effect in this.parent.GetComps<Comp_BaseAritfactEffect>())
                                 {
                                     isValid = effect.ValidateTarget(targetInfo);
                                 }

                                 return isValid;
                             }
                             else return true;
                         }, null);
                }
                else
                {
                    StartJob(selPawn, new LocalTargetInfo(selPawn));
                }
            };
            yield return new FloatMenuOption(Props.useLabel.Translate(this.parent), action);
        }

        private bool NeedsTargeting()
        {
            return Props.requiresTarget;
        }

        private void StartJob(Pawn user, LocalTargetInfo target)
        {
            bool canApply = true;

            foreach (var effect in this.parent.GetComps<Comp_BaseAritfactEffect>())
            {
                string reason = "";
                if (!effect.CanApply(user, target, this.parent, ref reason))
                {
                    canApply = false;
                    Messages.Message(reason, MessageTypeDefOf.NegativeEvent, false);
                    break;
                }
            }

            if (canApply)
            {
                Job job = JobMaker.MakeJob(Props.useJob, parent, target);
                job.count = 1;
                user.jobs.TryTakeOrderedJob(job, JobTag.DraftedOrder);
            }
        }

        public bool CanBeUsedNow(Pawn pawn)
        {
            return !parent.IsForbidden(pawn) && chargesRemaining > 0 && cooldownTicksRemaining <= 0;
        }


        public void UseEffects(Pawn user, LocalTargetInfo target)
        {
            if (cooldownTicksRemaining > 0 || (UsesCharges && chargesRemaining <= 0))
                return;

            if (Props.sound != null)
            {
                Props.sound.PlayOneShot(new TargetInfo(user.Position, user.Map));
            }

            if (Props.userUsedThought != null)
            {
                if (target.Thing is Pawn targetPawn)
                {
                    user.needs.mood.thoughts.memories.TryGainMemory(Props.userUsedThought, targetPawn);
                }
                else
                {
                    user.needs.mood.thoughts.memories.TryGainMemory(Props.userUsedThought);
                }
               
            }

            if (Props.targetUsedThought != null && target.Thing is Pawn pawn)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(Props.targetUsedThought, user);
            }

            if (Props.userEffectDef != null)
            {
                Props.userEffectDef.Spawn(user.Position, user.Map, 1);
            }

            if (Props.targetEffectDef != null)
            {
                Props.targetEffectDef.Spawn(target.Cell, user.Map, 1);
            }

            foreach (var effect in this.parent.GetComps<Comp_BaseAritfactEffect>())
            {
                effect.Apply(user, target, parent);
            }

            if (Props.destroyOnUse)
            {
                if (!parent.Destroyed)
                {
                    parent.Destroy();
                }
            }

            if (UsesCharges)
            {
                chargesRemaining--;
            }

            if (Props.hasCooldown)
            {
                cooldownTicksRemaining = Props.cooldownTickRange.RandomInRange;
            }

            if (chargesRemaining <= 0 && Props.destroyOnChargesDepleted)
            {
                if (!parent.Destroyed)
                {
                    parent.Destroy();
                }          
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (Props.charges > 1 || chargesRemaining < Props.charges)
            {
                stringBuilder.AppendLine("Uses remaining: " + chargesRemaining);
            }

            if (cooldownTicksRemaining > 0)
            {
                stringBuilder.AppendLine("Cooldown: " + cooldownTicksRemaining.ToStringTicksToPeriod());
            }

            return stringBuilder.Length > 0 ? stringBuilder.ToString().TrimEndNewlines() : base.CompInspectStringExtra();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref chargesRemaining, "usesRemaining", Props.charges);
            Scribe_Values.Look(ref cooldownTicksRemaining, "cooldownTicksRemaining", 0);
        }
    }
}