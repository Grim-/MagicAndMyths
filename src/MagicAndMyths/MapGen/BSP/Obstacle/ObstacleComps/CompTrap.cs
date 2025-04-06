using Verse;

namespace MagicAndMyths
{

    public class CompProperties_Trap : CompProperties
    {
       
    }

    public abstract class CompTrap : ThingComp
    {
        public bool IsArmed = true;
        public bool IsTriggered = false;
        public bool CanBeRearmed = false;

        public virtual bool CanAttemptDisarm(Pawn pawn)
        {
            return IsArmed && !IsTriggered;
        }

        public abstract bool TryDisarm(Pawn pawn);

        public virtual void Trigger(Pawn target)
        {
            if (!IsArmed || IsTriggered)
                return;

            IsArmed = false;
            IsTriggered = true;
            ApplyTrapEffects(target);
        }

        protected abstract void ApplyTrapEffects(Pawn target);

        public virtual bool TryRearm(Pawn pawn)
        {
            if (IsArmed || !CanBeRearmed)
                return false;

            IsArmed = true;
            IsTriggered = false;
            return true;
        }

        public virtual void Reset()
        {
            IsArmed = true;
            IsTriggered = false;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref IsArmed, "isArmed", true);
            Scribe_Values.Look(ref IsTriggered, "isTriggered", false);
            Scribe_Values.Look(ref CanBeRearmed, "canBeRearmed", false);
        }
    }
}
