using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class RoomConnection
    {
        public DungeonRoom roomA;
        public DungeonRoom roomB;
        public List<Corridoor> corridors = new List<Corridoor>();

        public DungeonRoom DestinationRoom => roomB;

        public RoomConnection(DungeonRoom roomA, DungeonRoom roomB)
        {
            this.roomA = roomA;
            this.roomB = roomB;
        }


        public IEnumerable<IntVec3> GetAllCells()
        {
            return corridors.SelectMany(x => x.path).ToList();
        }

        public bool CellIsOnCorridoor(IntVec3 c)
        {
            return corridors.Any(x => x.CellOnPath(c));
        }
    }
}
