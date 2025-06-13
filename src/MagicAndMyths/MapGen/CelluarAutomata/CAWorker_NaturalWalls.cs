using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CAWorker_NaturalWalls : CellularAutomataWorker
    {
        private int birthThreshold = 6;
        private int deathThreshold = 2;
        private float randomChance = 0.3f;


        public CAWorker_NaturalWalls()
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

                if (currentState[cell])
                {
                    if (!Dungeon.QueryManager.IsNearRoomEdge(cell, map, currentState))
                    {
                        continue;
                    }

                    if (IsPathCell(cell, map, currentState))
                    {
                        continue;
                    }
                }

                int wallNeighbors = CountWallNeighbors(cell, map, currentState);

                if (currentState[cell])
                {
                    if (wallNeighbors >= birthThreshold && Rand.Chance(randomChance))
                    {
                        dungeonGrid[cell] = false;
                    }
                }
                else
                {
                    if (wallNeighbors <= deathThreshold && Rand.Chance(randomChance * 0.7f))
                    {
                        if (Dungeon.QueryManager.CountFloorNeighbors(cell, map, currentState) >= 1)
                        {
                            dungeonGrid[cell] = true;
                        }
                    }
                }
            }
        }

       
    }
}
