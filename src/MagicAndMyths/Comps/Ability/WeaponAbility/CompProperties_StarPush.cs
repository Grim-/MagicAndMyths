using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_StarPush : CompProperties_AbilityEffect
    {
        public int minTilesToPush = 1;
        public int maxTilesToPush = 5;
        public int radius = 15;
        public EffecterDef effecterDef;
        public CompProperties_StarPush()
        {
            compClass = typeof(CompAbilityEffect_StarPush);
        }
    }

    public class CompAbilityEffect_StarPush : CompAbilityEffect
    {
        CompProperties_StarPush Props => (CompProperties_StarPush)props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = this.parent.pawn.Map;

            if (Props.effecterDef != null)
            {
                Props.effecterDef.Spawn(this.parent.pawn.Position, this.parent.pawn.Map);
            }

            List<Pawn> pawnsInRange = TargetUtil.GetPawnsInRadius(this.parent.pawn.Position, map, Props.radius, this.parent.pawn.Faction, true, this.parent.pawn, true, false, false);

            foreach (var pawn in pawnsInRange)
            {
                if (pawn == this.parent.pawn)
                {
                    continue;
                }

                float distance = pawn.Position.DistanceTo(this.parent.pawn.Position);
                float pushFactor = 1f - (distance / Props.radius);
                int pushDistance = Mathf.RoundToInt(Props.minTilesToPush + pushFactor * (Props.maxTilesToPush - Props.minTilesToPush));

                IntVec3 direction = (pawn.Position - this.parent.pawn.Position);

                IntVec3 destination = pawn.Position + (direction * pushDistance);

                ThingFlyer thingFlyer = ThingFlyer.MakeFlyer(MagicAndMythDefOf.MagicAndMyths_ThingFlyer, pawn, destination, map, null, null, this.parent.pawn, pawn.DrawPos, false);
                ThingFlyer.LaunchFlyer(thingFlyer, pawn, pawn.Position, map);
            }
        }
    }
}
