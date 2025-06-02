using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_AddHediffAOEPerInterval : HediffCompProperties_BaseInterval
    {
        public HediffDef hediff;
        public float radius = 5;
        public bool canTargetHostile = true;
        public bool canTargetFriendly = false;
        public bool canTargetNeutral = false;

        public HediffCompProperties_AddHediffAOEPerInterval()
        {
            compClass = typeof(HediffComp_AddHediffAOEPerInterval);
        }
    }

    public class HediffComp_AddHediffAOEPerInterval : HediffComp_BaseInterval
    {
        new public HediffCompProperties_AddHediffAOEPerInterval Props => (HediffCompProperties_AddHediffAOEPerInterval)props;
        protected override void OnInterval()
        {
            base.OnInterval();
            if (Props.hediff != null)
            {
                TargetUtil.ApplyHediffInRadius(Props.hediff,
                    Pawn.Position,
                    Pawn.Map,
                    Props.radius,
                    Pawn.Faction,
                    true,
                    Props.canTargetHostile,
                    Props.canTargetFriendly,
                    Props.canTargetNeutral);
            }
        }
    }

}