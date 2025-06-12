using System;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CAWorker_CaveDecorator : CellularAutomataWorker
    {
        private float randomChance = 0.2f;

        public CAWorker_CaveDecorator()
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

                if (!currentState[cell] && Dungeon.SpatialAnalyzer.IsWallNearFloor(cell, map, currentState))
                {
                    if (Rand.Chance(randomChance))
                    {
                        IntVec3 nearestFloor = Dungeon.SpatialAnalyzer.FindNearestFloor(cell, map, currentState);
                        if (nearestFloor.IsValid)
                        {
                            IntVec3 direction = new IntVec3(
                                Math.Sign(nearestFloor.x - cell.x),
                                0,
                                Math.Sign(nearestFloor.z - cell.z));

                            IntVec3 nextCell = cell + direction;

                            if (!CanAffectCell(map, Dungeon, nextCell))
                            {
                                continue;
                            }

                            if (nextCell.InBounds(map) && !currentState[nextCell])
                            {
                                dungeonGrid[nextCell] = true;
                            }
                        }
                    }
                }
            }
        }


    }
}
