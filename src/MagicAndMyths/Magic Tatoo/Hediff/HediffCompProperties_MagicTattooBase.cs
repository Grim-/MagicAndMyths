using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_MagicTattooBase : HediffCompProperties
    {
        public TattooDef tattooDef;

        public HediffCompProperties_MagicTattooBase()
        {
            compClass = typeof(HediffComp_MagicTattooBase);
        }
    }

    public abstract class HediffComp_MagicTattooBase : HediffComp
    {
        public HediffCompProperties_MagicTattooBase Props => (HediffCompProperties_MagicTattooBase)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
        }

        public virtual void OnTattooApplied()
        {

        }

        public virtual void OnTattooRemoved()
        {

        }
    }
}
