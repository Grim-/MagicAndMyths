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

            StageVisualEffect.CreateStageEffect(cells, map, Random.Range(8, 15), (IntVec3 cell, Map targetMap, int sectionIndex) =>
            {
                Pawn attacker = this.parent.pawn;

                if (sectionIndex % 2 == 0)
                {
                    EffecterDefOf.PawnEmergeFromWaterLarge.Spawn(cell, map);
                }


                Pawn pawn = cell.GetFirstPawn(map);


                if (FireUtility.NumFiresAt(cell, targetMap) > 0)
                {
                    foreach (var item in cell.GetFiresNearCell(map))
                    {
                        item.TakeDamage(new DamageInfo(DamageDefOf.Extinguish, 100f, 0f, -1f, this.parent.pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true));
                    }
                }



                if (pawn != null && pawn != attacker && pawn.CanTargetThing(attacker.Faction, Props.friendlyFireSettings))
                {
                    pawn.health.GetOrAddHediff(MagicAndMythDefOf.MagicAndMyths_Wet);

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
