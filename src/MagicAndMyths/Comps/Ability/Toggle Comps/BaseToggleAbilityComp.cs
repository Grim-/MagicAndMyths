using RimWorld;

namespace MagicAndMyths
{
    public abstract class BaseToggleAbilityComp : CompAbilityEffect
    {
        public virtual void OnParentActivated()
        {

        }

        public virtual void OnParentDeactivated()
        {

        }

        public virtual float GetExtraResourceCost()
        {
            return 0;
        }
    }
}
