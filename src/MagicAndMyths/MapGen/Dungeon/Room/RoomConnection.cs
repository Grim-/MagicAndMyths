using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class RoomConnection
    {
        public DungeonRoom roomA;
        public DungeonRoom roomB;
        //public List<Corridoor> corridors = new List<Corridoor>();
        private Corridoor _Corridoor = null;
        public Corridoor Corridoor
        {
            get => _Corridoor;
        }

        public DungeonRoom SourceRoom => roomA;
        public DungeonRoom DestinationRoom => roomB;

        public RoomConnection(DungeonRoom roomA, DungeonRoom roomB)
        {
            this.roomA = roomA;
            this.roomB = roomB;
        }


        public void SetCorridoor(Corridoor corridoor)
        {
            this._Corridoor = corridoor;
        }

        public IEnumerable<IntVec3> GetAllCells()
        {
            return _Corridoor.path;
        }

        public bool CellIsOnCorridoor(IntVec3 c)
        {
            return _Corridoor.path.Contains(c);
        }
    }
}
