using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_UseEffectCooldown : CompProperties_UseEffect
    {
        public int cooldownTicks = 6000;

        public CompProperties_UseEffectCooldown()
        {
            compClass = typeof(Comp_UseEffectCooldown);
        }
    }

    public class Comp_UseEffectCooldown : CompUseEffect
    {
        CompProperties_UseEffectCooldown Props => (CompProperties_UseEffectCooldown)props;

        protected int lastUseTick = -1;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            lastUseTick = Current.Game.tickManager.TicksGame;
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (lastUseTick != -1 && Current.Game.tickManager.TicksGame <= lastUseTick + Props.cooldownTicks)
            {
                return AcceptanceReport.WasRejected;
            }

            return base.CanBeUsedBy(p);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastUseTick, "lastUseTick", -1);
        }
    }


}