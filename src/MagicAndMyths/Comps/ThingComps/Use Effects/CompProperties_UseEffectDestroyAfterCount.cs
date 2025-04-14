using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_UseEffectDestroyAfterCount : CompProperties_UseEffect
    {
        public int maxUses = 1;

        public CompProperties_UseEffectDestroyAfterCount()
        {
            compClass = typeof(Comp_UseEffectDestroyAfterCount);
        }
    }

    public class Comp_UseEffectDestroyAfterCount : CompUseEffect
    {
        CompProperties_UseEffectDestroyAfterCount Props => (CompProperties_UseEffectDestroyAfterCount)props;

        protected int currentUseCount = 0;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            currentUseCount++;
            if (currentUseCount >= Props.maxUses)
            {
                this.parent.Destroy();
            }
        }


        public override string CompInspectStringExtra()
        {
            return base.CompInspectStringExtra() + $"Charges : {Props.maxUses - currentUseCount}";
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentUseCount, "currentUseCount", 0);
        }
    }


}