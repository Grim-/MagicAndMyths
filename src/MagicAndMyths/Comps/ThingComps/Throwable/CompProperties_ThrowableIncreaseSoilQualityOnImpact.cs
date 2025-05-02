using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThrowableIncreaseSoilQualityOnImpact : CompProperties_Throwable
    {
        public FloatRange healAmount = new FloatRange(10, 10);
        public CompProperties_ThrowableIncreaseSoilQualityOnImpact()
        {
            compClass = typeof(Comp_ThrowableIncreaseSoilQualityOnImpact);
        }
    }

    public class Comp_ThrowableIncreaseSoilQualityOnImpact : Comp_Throwable
    {
        CompProperties_ThrowableIncreaseSoilQualityOnImpact Props => (CompProperties_ThrowableIncreaseSoilQualityOnImpact)props;

        public override void OnRespawn(IntVec3 position, Thing thing, Map map, Pawn throwingPawn)
        {
            base.OnRespawn(position, thing, map, throwingPawn);
            foreach (var item in GenRadial.RadialCellsAround(position, Props.radius, true))
            {
                if (item.InBounds(map))
                {
                    TerrainDef currentTerrain = item.GetTerrain(map);
                    float currentFertility = map.fertilityGrid.FertilityAt(item);
                    float targetFertility = currentFertility + Props.healAmount.RandomInRange;
                    TerrainDef betterTerrain = DefDatabase<TerrainDef>.AllDefsListForReading
                        .Where(t => t.fertility > currentFertility && t.fertility <= targetFertility)
                        .OrderBy(t => Mathf.Abs(t.fertility - targetFertility))
                        .FirstOrDefault();

                    if (betterTerrain != null)
                    {
                        map.terrainGrid.SetTerrain(item, betterTerrain);
                        FleckMaker.ThrowDustPuff(item.ToVector3Shifted(), map, 0.5f);
                    }
                }
            }
        }
    }
}