﻿using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class VerbProperties_Channeled : VerbProperties
    {
        public int channelDurationTicks = 300;
        public JobDef channelJobDef;
    }

    public abstract class Verb_Channeled : Verb
    {
       protected VerbProperties_Channeled Props => (VerbProperties_Channeled)verbProps;

        protected int lastVerbUseTick = 0;
        protected int channelTicks = 0;
        protected int channelStartTick = 0;

        public bool IsOnCooldown
        {
            get => MagicUtil.HasCooldownByTick(lastVerbUseTick, 100);
        }

        public override void WarmupComplete()
        {
            channelTicks = 0;
            base.WarmupComplete();
        }

        public override bool Available()
        {
            return !IsOnCooldown;
        }

        protected override bool TryCastShot()
        {
            if (IsOnCooldown)
            {
                return false;
            }




            TryStartCastOn(currentTarget);
            return true;
        }


        public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
        {
            lastVerbUseTick = Current.Game.tickManager.TicksGame;
            channelTicks = 0;
            return base.TryStartCastOn(castTarg, destTarg, surpriseAttack, canHitNonTargetPawns, preventFriendlyFire, nonInterruptingSelfCast);
        }

        public override bool CanHitTarget(LocalTargetInfo targ)
        {
            return base.CanHitTarget(targ);
        }

        public override void OrderForceTarget(LocalTargetInfo target)
        {
            if (!IsOnCooldown)
            {
                Job job = JobMaker.MakeJob(Props.channelJobDef, target);
                job.verbToUse = this;
                this.CasterPawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
            }
            else
            {
                Log.Message("On cooldown");
            }
        }

        public virtual void OnChannelStart(LocalTargetInfo target)
        {
            channelStartTick = Find.TickManager.TicksGame;
            channelTicks = 0;
        }


        public virtual void OnChannelJobTick(LocalTargetInfo target)
        {
            channelTicks++;
        }


        public virtual bool CanChannel(LocalTargetInfo target)
        {
            return true;
        }


        public virtual void OnChannelEnd(LocalTargetInfo target)
        {
            this.Reset();
        }


        public override void ExposeData()
        {
            base.ExposeData();
           
            Scribe_Values.Look(ref lastVerbUseTick, "lastVerbUseTick");
            Scribe_Values.Look(ref channelStartTick, "channelStartTick");        
            Scribe_Values.Look(ref channelTicks, "channelTicks");
        }
    }
}