using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_MapWideApplyHediff : CompProperties_MapWideEffect
    {
        public HediffDef hediff;
        public float severityPerTick = 0.1f;

        public CompProperties_MapWideApplyHediff()
        {
            compClass = typeof(Comp_MapWideApplyHediff);
        }
    }

    public class Comp_MapWideApplyHediff : Comp_MapWideEffect
    {
        private CompProperties_MapWideApplyHediff Props => (CompProperties_MapWideApplyHediff)props;
        protected override void DoMapWideEffect()
        {
            base.DoMapWideEffect();

            if (this.parent.Map != null)
            {
                foreach (var item in this.parent.Map.mapPawns.AllPawns)
                {
                    if (item.Faction == Faction.OfPlayer)
                    {
                        Hediff hediff = item.health.GetOrAddHediff(Props.hediff);
                        hediff.Severity += Props.severityPerTick;
                    }
                }
            }
        }
    }

}