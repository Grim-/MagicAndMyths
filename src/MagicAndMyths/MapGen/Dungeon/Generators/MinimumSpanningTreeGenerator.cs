namespace MagicAndMyths
{
    public class MinimumSpanningTreeGenerator
    {
        private readonly Dungeon dungeon;

        public MinimumSpanningTreeGenerator(Dungeon dungeon)
        {
            this.dungeon = dungeon;
        }

        public void CreateMinimumSpanningTree()
        {
            MspUtility.CreateMinimumSpanningTree(dungeon.LeafNodes);

            foreach (var node in dungeon.LeafNodes)
            {
                DungeonRoom room = dungeon.GetRoom(node);

                foreach (var connectedNode in node.connectedNodes)
                {
                    DungeonRoom connectedRoom = dungeon.GetRoom(connectedNode);
                    if (!room.connectedRooms.Contains(connectedRoom))
                    {
                        room.connectedRooms.Add(connectedRoom);
                    }
                }
            }
        }
    }
}