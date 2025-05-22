using RimWorld;
using System.Collections.Generic;
using System.Linq;
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


            List<IntVec3> cells = GenRadial.RadialCellsAround(this.parent.pawn.Position, Props.radius, true).ToList();

            StageVisualEffect.CreateStageEffect(cells, map, 8, (IntVec3 cell, Map targetMap, int currentSection) =>
            {
                EffecterDefOf.ImpactSmallDustCloud.Spawn(cell, map);

                List<Thing> things = cell.GetThingList(map).ToList();

                foreach (var t in things)
                {
                    if (t is Pawn || t is Building building)
                    {
                        if (t != this.parent.pawn)
                        {
                            DamageInfo damage = t.def.mineable ? new DamageInfo(DamageDefOf.Mining, 344 * 2, 1) : new DamageInfo(DamageDefOf.Blunt, 15, 1);
                            t.TakeDamage(damage);

                            if (t is Pawn)
                            {
                                float distance = t.Position.DistanceTo(this.parent.pawn.Position);
                                float pushFactor = 1f - (distance / Props.radius);
                                int pushDistance = Mathf.RoundToInt(Props.minTilesToPush + pushFactor * (Props.maxTilesToPush - Props.minTilesToPush));
                                IntVec3 direction = (t.Position - this.parent.pawn.Position);
                                IntVec3 destination = t.Position + (direction * pushDistance);
                                ThingFlyer thingFlyer = ThingFlyer.MakeFlyer(MagicAndMythDefOf.MagicAndMyths_ThingFlyer, t, destination, map, null, null, this.parent.pawn, t.DrawPos, false);
                                ThingFlyer.LaunchFlyer(thingFlyer, t, t.Position, map);
                            }
                        }

                    }
                }

            }, 5);
        }
    }
}
