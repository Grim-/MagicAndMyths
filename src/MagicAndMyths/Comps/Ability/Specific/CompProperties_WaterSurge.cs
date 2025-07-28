using EMF;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_WaterSurge : CompProperties_AbilityEffect
    {
        public int range = 5;
        public float angle = 45f;
        public int maxPushDistance = 10;
        public int minPushDistance = 1;
        public int sections = 4;
        public int ticksBetweenSections = 2;

        public EffecterDef splashEffect;


        public FriendlyFireSettings friendlyFireSettings = FriendlyFireSettings.HostileOnly();
        public CompProperties_WaterSurge()
        {
            compClass = typeof(CompAbilityEffect_WaterSurge);
        }
    }


    public class CompAbilityEffect_WaterSurge : CompAbilityEffect
    {
        CompProperties_WaterSurge Props => (CompProperties_WaterSurge)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = this.parent.pawn.Map;
            List<IntVec3> cells = TargetUtil.GetCellsInCone(this.parent.pawn.Position, target.Cell, (int)this.parent.verb.EffectiveRange, Props.angle);
            cells = cells.OrderBy(x => x.DistanceTo(this.parent.pawn.Position)).ToList();

            StageVisualEffect.CreateStageEffect(cells, map, Props.sections, (IntVec3 cell, Map targetMap, int sectionIndex) =>
            {
                Pawn attacker = this.parent.pawn;
                if (sectionIndex % 2 == 0)
                {
                    if (Props.splashEffect != null)
                    {
                        Props.splashEffect.Spawn(cell, map);
                    }
                    else
                        EffecterDefOf.PawnEmergeFromWater.Spawn(cell, map, 0.5f);
                }

                Pawn pawn = cell.GetFirstPawn(map);

                MagicUtil.TryExtinguishFireAt(cell, map);

                if (pawn != null && pawn != attacker && pawn.CanTargetThing(attacker.Faction, Props.friendlyFireSettings))
                {
                    pawn.health.GetOrAddHediff(MagicAndMythDefOf.MagicAndMyths_Wet);
                    IntVec3 destination = MagicUtil.CalculatePushDirection(this.parent.pawn.Position, pawn.Position, Props.minPushDistance, Props.maxPushDistance);
                    ThingFlyer thingFlyer = ThingFlyer.MakeFlyer(MagicAndMythDefOf.MagicAndMyths_ThingFlyer, pawn, destination, map, null, null, this.parent.pawn, pawn.DrawPos, false);
                    ThingFlyer.LaunchFlyer(thingFlyer, pawn, pawn.Position, map);
                }
            }, Props.ticksBetweenSections);
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);
            GenDraw.DrawFieldEdges(TargetUtil.GetCellsInCone(this.parent.pawn.Position, target.Cell, (int)this.parent.verb.EffectiveRange, Props.angle));
        }
    }
}
