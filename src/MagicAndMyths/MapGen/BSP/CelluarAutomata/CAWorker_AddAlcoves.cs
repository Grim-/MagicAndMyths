using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{


    public class CAWorker_AddAlcoves : CellularAutomataWorker
    {
        private float randomChance = 0.15f;

        public CAWorker_AddAlcoves()
        {

        }

        public override void Apply(Map map, Dictionary<BspNode, DungeonRoom> nodeToRoomMap, BoolGrid dungeonGrid, BoolGrid currentState)
        {
            foreach (IntVec3 cell in map.AllCells)
            {
                if (cell.x <= 3 || cell.z <= 3 || cell.x >= map.Size.x - 4 || cell.z >= map.Size.z - 4)
                {
                    continue;
                }

                if (!currentState[cell])
                {
                    if (IsSuitableForAlcove(cell, map, currentState) && Rand.Chance(randomChance))
                    {
                        dungeonGrid[cell] = true;
                    }
                }
            }
        }

        private bool IsSuitableForAlcove(IntVec3 cell, Map map, BoolGrid grid)
        {
            if (grid[cell]) return false;

            IntVec3[] cardinals = {
                new IntVec3(0, 0, 1),  // North
                new IntVec3(1, 0, 0),  // East
                new IntVec3(0, 0, -1), // South
                new IntVec3(-1, 0, 0)  // West
            };

            foreach (var dir in cardinals)
            {
                IntVec3 neighbor = cell + dir;
                IntVec3 twoAway = cell + dir + dir;

                if (neighbor.InBounds(map) && twoAway.InBounds(map) &&
                    grid[neighbor] && grid[twoAway])
                {
                    return true;
                }
            }

            return false;
        }
    }
}
