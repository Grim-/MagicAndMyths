using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    //a the empty cells between rooms
    public class Corridoor
    {
        public IntVec3 Start;
        public IntVec3 End;
        public IntVec3 RoomAEntryPoint;
        public IntVec3 RoomBEntryPoint;
        public List<IntVec3> path;

        public Corridoor(IntVec3 start, IntVec3 end)
        {
            Start = start;
            End = end;
            path = new List<IntVec3>();
            RoomAEntryPoint = Start;
            RoomBEntryPoint = End;
        }

        public bool CellOnPath(IntVec3 c)
        {
            return Start == c || End == c || path.Contains(c);
        }

        public void SetPath(List<IntVec3> pathCells)
        {
            path = pathCells;
        }
    }
}
