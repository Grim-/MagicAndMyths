using Verse;

namespace MagicAndMyths
{
    public abstract class Comp_BaseAritfactEffect : ThingComp
    {
        public virtual void Apply(Pawn user, LocalTargetInfo target, Thing item)
        {

        }


        public virtual bool ValidateTarget(LocalTargetInfo TargetInfo)
        {
            return true;
        }


        public virtual bool CanApply(Pawn user, LocalTargetInfo target, Thing item, ref string reason)
        {
            reason = string.Empty;
            return true;
        }
    }
}