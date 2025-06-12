using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CAWorker_SmoothCorners : CellularAutomataWorker
    {
        private float randomChance = 0.7f;

        public CAWorker_SmoothCorners()
        {
        }

        public override void Apply(Map map, Dungeon Dungeon, BoolGrid dungeonGrid, BoolGrid currentState)
        {
            foreach (IntVec3 cell in map.AllCells)
            {
                if (!CanAffectCell(map, Dungeon, cell))
                {
                    continue;
                }

                if (!currentState[cell]) // Wall cell
                {
                    if (Dungeon.SpatialAnalyzer.IsCornerWall(cell, map, currentState) && Rand.Chance(randomChance))
                    {
                        dungeonGrid[cell] = true;
                    }
                }
            }
        }


    }
}
