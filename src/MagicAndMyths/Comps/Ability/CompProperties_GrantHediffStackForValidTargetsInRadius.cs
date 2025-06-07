using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_GrantHediffStackForValidTargetsInRadius : CompProperties_AbilityEffect
    {
        public HediffDef hediffDef;
        public int stacksPerTarget = 1;
        public float radius = 3f;
        public FriendlyFireSettings fireSettings = FriendlyFireSettings.HostileOnly();

        public CompProperties_GrantHediffStackForValidTargetsInRadius()
        {
            compClass = typeof(CompAbilityEffect_GrantHediffStackForValidTargetsInRadius);
        }
    }

    public class CompAbilityEffect_GrantHediffStackForValidTargetsInRadius : CompAbilityEffect
    {
        CompProperties_GrantHediffStackForValidTargetsInRadius Props => (CompProperties_GrantHediffStackForValidTargetsInRadius)props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = this.parent.pawn.Map;

            if (Props.hediffDef != null && Props.hediffDef.hediffClass == typeof(HediffWithStacks))
            {
                List<Pawn> targets = TargetUtil.GetPawnsInRadius(this.parent.pawn.Position, map, Props.radius, this.parent.pawn.Faction, Props.fireSettings, true);
                HediffWithStacks hediffWithStacks = (HediffWithStacks)this.parent.pawn.health.GetOrAddHediff(Props.hediffDef);

                if (hediffWithStacks != null)
                {
                    hediffWithStacks.AddStack(Props.stacksPerTarget * targets.Count);
                }
            }
        }
    }

}
