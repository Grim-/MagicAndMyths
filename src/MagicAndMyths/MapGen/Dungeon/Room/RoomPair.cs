namespace MagicAndMyths
{
    public struct RoomPair
    {
        public DungeonRoom RoomA;
        public DungeonRoom RoomB;

        public RoomPair(DungeonRoom roomA, DungeonRoom roomB)
        {
            RoomA = roomA;
            RoomB = roomB;
        }


        public bool IsValid()
        {
            return RoomA != null && RoomB != null;
        }
    }
}
