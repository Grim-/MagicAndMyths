using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class MapModifier_StoneCreep : MapModifier
    {
        private readonly TerrainDef stoneTerrain = TerrainDefOf.GraySurface;
        private List<IntVec3> edgeCells = new List<IntVec3>();

        public override int MinTicksBetweenEffects => 500;

        public MapModifier_StoneCreep(Map map) : base(map)
        {
            for (int i = 0; i < 5; i++)
            {
                edgeCells.Add(CellFinder.RandomCell(map));
            }
        }

        public override void ApplyEffect()
        {
            if (!edgeCells.Any()) return;

            IntVec3 sourceCell = edgeCells.RandomElement();

            List<IntVec3> adjacentCells = GenAdj.CellsAdjacent8Way(new TargetInfo(sourceCell, map))
                .Where(c => c.InBounds(map))
                .ToList();

            if (adjacentCells.Any())
            {
                IntVec3 targetCell = adjacentCells.RandomElement();
                map.terrainGrid.SetTerrain(targetCell, stoneTerrain);
                edgeCells.Add(targetCell);
            }

            edgeCells.Remove(sourceCell);
        }
    }
}

