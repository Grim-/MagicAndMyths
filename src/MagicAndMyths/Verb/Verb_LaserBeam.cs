using RimWorld;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class Verb_LaserBeam : Verb_Channeled
    {
        private LaserBeamEffect currentBeam;
        protected override bool TryCastShot()
        {
            if (!this.currentTarget.HasThing)
                return false;



            return true;
        }


        public override void OnChannelStart(LocalTargetInfo target)
        {
            base.OnChannelStart(target);
            channelTicks = 0;


            Thing targetThing = target.Thing;

            currentBeam = new LaserBeamEffect(
                caster.Map,
                caster,
                targetThing,
                Props.channelDurationTicks);

            currentBeam.StartBeam();
            ApplyDamageToTarget(targetThing, 0.25f);
        }


        public override void OnChannelJobTick(LocalTargetInfo target)
        {
            base.OnChannelJobTick(target);
            if (currentBeam != null)
            {
                currentBeam.Tick();
            }

            if (Find.TickManager.TicksGame % 15 == 0 && this.currentTarget.HasThing)
                ApplyDamageToTarget(this.currentTarget.Thing, 0.1f);

            if (channelTicks % 60 == 0)
                Log.Message("Channel tick: " + channelTicks + " / " + Props.channelDurationTicks);
        }

        public override bool CanChannel(LocalTargetInfo target)
        {
            bool channelTicksValid = channelTicks < Props.channelDurationTicks;
            bool casterValid = CasterCanStillChannel();
            bool targetValid = IsTargetStillValid(target);

            if (!channelTicksValid)
                Log.Message("Beam ended: channelTicks (" + channelTicks + ") >= channelDurationTicks (" + Props.channelDurationTicks + ")");
            if (!casterValid)
                Log.Message("Beam ended: Caster cannot still channel");
            if (!targetValid)
                Log.Message("Beam ended: Target is no longer valid");

            return channelTicksValid && targetValid;
        }

        public override void OnChannelEnd(LocalTargetInfo target)
        {
            base.OnChannelEnd(target);
            EndBeam();
        }

        private bool CasterCanStillChannel()
        {
            if (CasterPawn == null) 
                return false;

            return !CasterPawn.Downed &&
                   !CasterPawn.Dead &&
                   CasterPawn.Awake() &&
                   !CasterPawn.InMentalState &&
                   !CasterPawn.stances.stunner.Stunned;
        }

        private void ApplyDamageToTarget(Thing targetThing, float damageMultiplier)
        {
            float baseDamage = 10f;

            DamageInfo dinfo = new DamageInfo(
                DamageDefOf.Burn,
                baseDamage * damageMultiplier,
                0.5f,
                -1,
                this.caster);

            targetThing.TakeDamage(dinfo);
        }

        private bool IsTargetStillValid(LocalTargetInfo target)
        {
            return target.HasThing &&
                   !target.Thing.Destroyed &&
                   target.Thing.Spawned;
        }


        private void EndBeam()
        {
            if (currentBeam != null)
            {
                currentBeam.Stop();
                currentBeam = null;
            }
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!target.HasThing)
            {
                if (showMessages)
                    Messages.Message("Cannot use beam: Must target an entity.", MessageTypeDefOf.RejectInput);
                return false;
            }

            return base.ValidateTarget(target, showMessages);
        }

        public override void Reset()
        {
            EndBeam();
            channelTicks = 0;
            base.Reset();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref channelStartTick, "channelStartTick", -1);
        }
    }


}