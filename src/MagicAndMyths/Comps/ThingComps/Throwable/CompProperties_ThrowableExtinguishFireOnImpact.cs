using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThrowableExtinguishFireOnImpact : CompProperties_Throwable
    {
        public bool completelyExtinguish = true;
        public FloatRange fireReduction = new FloatRange(10f, 10f);
        public EffecterDef extinguishEffect;

        public CompProperties_ThrowableExtinguishFireOnImpact()
        {
            compClass = typeof(Comp_ThrowableExtinguishFireOnImpact);
        }
    }


    public class Comp_ThrowableExtinguishFireOnImpact : Comp_Throwable
    {
        CompProperties_ThrowableExtinguishFireOnImpact Props => (CompProperties_ThrowableExtinguishFireOnImpact)props;

        public override void OnRespawn(IntVec3 position, Thing thing, Map map, Pawn throwingPawn)
        {
            base.OnRespawn(position, thing, map, throwingPawn);

            List<Thing> fires = new List<Thing>();
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, Props.radius, true))
            {
                if (cell.InBounds(map))
                {
                    List<Thing> thingsInCell = cell.GetThingList(map);
                    for (int i = 0; i < thingsInCell.Count; i++)
                    {
                        if (thingsInCell[i] is Fire fire)
                        {
                            fires.Add(fire);
                        }
                    }
                }
            }

            // Apply extinguish effect
            if (Props.extinguishEffect != null)
            {
                Effecter effecter = Props.extinguishEffect.Spawn();
                effecter.Trigger(new TargetInfo(position, map), new TargetInfo(position, map));
                effecter.Cleanup();
            }
            else
            {
                FleckMaker.ThrowSmoke(position.ToVector3Shifted(), map, Props.radius);
            }

            for (int i = 0; i < fires.Count; i++)
            {
                Fire fire = fires[i] as Fire;
                if (fire != null)
                {
                    if (Props.completelyExtinguish)
                    {
                        fire.Destroy();
                    }
                    else
                    {
                        float reduction = Props.fireReduction.RandomInRange;
                        if (fire.fireSize > reduction)
                        {
                            fire.fireSize -= reduction;
                        }
                        else
                        {
                            fire.Destroy();
                        }
                    }
                }
            }

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, Props.radius, true))
            {
                if (cell.InBounds(map))
                {
                    FleckMaker.WaterSplash(cell.ToVector3Shifted(), map, 1f, 2f);
                }
            }

        }
    }
}