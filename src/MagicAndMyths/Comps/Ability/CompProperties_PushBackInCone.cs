using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_PushBackInCone : CompProperties_AbilityEffect
    {
        public int range = 5;
        public float angle = 45f;
        public int maxPushDistance = 10;
        public int minPushDistance = 1;
        public CompProperties_PushBackInCone()
        {
            compClass = typeof(CompAbilityEffect_PushBackInCone);
        }
    }


    public class CompAbilityEffect_PushBackInCone : CompAbilityEffect
    {
        CompProperties_PushBackInCone Props => (CompProperties_PushBackInCone)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = this.parent.pawn.Map;
            List<IntVec3> cells = TargetUtil.GetCellsInCone(this.parent.pawn.Position, target.Cell, (int)this.parent.verb.EffectiveRange, Props.angle);
            cells = cells.OrderBy(x => x.DistanceTo(this.parent.pawn.Position)).ToList();

            StageVisualEffect.CreateStageEffect(cells, map, Random.Range(8, 15), (IntVec3 cell, Map targetMap, int sectionIndex) =>
            {
                Pawn attacker = this.parent.pawn;
                EffecterDefOf.PawnEmergeFromWater.Spawn(cell, map);
                Pawn pawn = cell.GetFirstPawn(map);
                if (pawn != null && pawn != attacker)
                {
                    float distance = pawn.Position.DistanceTo(this.parent.pawn.Position);
                    float pushFactor = 1f - (distance / Props.maxPushDistance);
                    int pushDistance = Mathf.RoundToInt(Props.minPushDistance + pushFactor * (Props.maxPushDistance - Props.minPushDistance));
                    IntVec3 direction = (pawn.Position - this.parent.pawn.Position);
                    IntVec3 destination = pawn.Position + (direction * pushDistance);
                    ThingFlyer thingFlyer = ThingFlyer.MakeFlyer(MagicAndMythDefOf.MagicAndMyths_ThingFlyer, pawn, destination, map, null, null, this.parent.pawn, pawn.DrawPos, false);
                    ThingFlyer.LaunchFlyer(thingFlyer, pawn, pawn.Position, map);
                }
            });
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);


            GenDraw.DrawFieldEdges(TargetUtil.GetCellsInCone(this.parent.pawn.Position, target.Cell, (int)this.parent.verb.EffectiveRange, Props.angle));
        }
    }



}
