using System.Collections.Generic;
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
    }
}
