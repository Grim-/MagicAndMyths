using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_AbilityToggleStance : CompProperties_AbilityEffect
    {
        public StanceDef stance;
        public bool allowDeactivation = true;

        public CompProperties_AbilityToggleStance()
        {
            compClass = typeof(CompAbilityEffect_ToggleStance);
        }
    }
    public class CompAbilityEffect_ToggleStance : CompAbilityEffect
    {
        public CompProperties_AbilityToggleStance Props => props as CompProperties_AbilityToggleStance;

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return target.Pawn != null && GetStanceManager(target.Pawn) != null;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            var pawn = target.Pawn;
            var stanceManager = GetStanceManager(pawn);

            if (stanceManager == null) 
                return;


            bool isActive = stanceManager.IsStanceActive(Props.stance);

            if (isActive && Props.allowDeactivation)
            {
                stanceManager.DeactivateStance(Props.stance);
            }
            else if (!isActive)
            {
                stanceManager.ActivateStance(Props.stance);
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            var pawn = target.Pawn;
            var stanceManager = GetStanceManager(pawn);

            if (stanceManager == null)
            {
                if (throwMessages)
                {
                    Messages.Message("No stance manager found", MessageTypeDefOf.RejectInput);
                }
                return false;
            }

            bool isActive = stanceManager.IsStanceActive(Props.stance);

            if (isActive)
            {
                return Props.allowDeactivation && stanceManager.CanDeactivateStance(Props.stance);
            }
            else
            {
                return stanceManager.CanActivateStance(Props.stance);
            }
        }

        private Gene_StanceManager GetStanceManager(Pawn pawn)
        {
            return pawn?.genes?.GetFirstGeneOfType<Gene_StanceManager>();
        }
    }
}
