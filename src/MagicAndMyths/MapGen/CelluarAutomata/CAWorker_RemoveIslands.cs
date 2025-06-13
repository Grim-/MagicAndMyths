using Verse;

namespace MagicAndMyths
{
    public class CAWorker_RemoveIslands : CellularAutomataWorker
    {

        private int neighbourKillCount = 2;

        public CAWorker_RemoveIslands()
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

                if (!currentState[cell])
                {
                    int wallNeighborCount = Dungeon.QueryManager.CountCardinalWallNeighbors(cell, map, currentState);
                    if (wallNeighborCount < neighbourKillCount)
                    {
                        dungeonGrid[cell] = true;
                    }
                }
            }
        }


    }
}
