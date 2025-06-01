using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_AbilityCancelStances : CompProperties_AbilityEffect
    {

        public CompProperties_AbilityCancelStances()
        {
            compClass = typeof(CompAbilityEffect_AbilityCancelStances);
        }
    }

    public class CompAbilityEffect_AbilityCancelStances : CompAbilityEffect
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

            stanceManager.DeactivateAllStances();
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

            return stanceManager.IsAnyStanceActive();
        }

        private Gene_StanceManager GetStanceManager(Pawn pawn)
        {
            return pawn?.genes?.GetFirstGeneOfType<Gene_StanceManager>();
        }
    }
}
