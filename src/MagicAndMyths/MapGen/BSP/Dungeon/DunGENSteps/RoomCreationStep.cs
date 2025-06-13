namespace MagicAndMyths
{
    public class RoomCreationStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            CreateRoomsFromBspNodes(context, context.Dungeon);
        }


        public void CreateRoomsFromBspNodes(DungeonGenerationContext context, Dungeon dungeon)
        {
            foreach (var node in dungeon.LeafNodes)
            {
                if (dungeon.HasMapping(node))
                    continue;

                DungeonRoom room = DungeonRoom.FromBspNode(dungeon, node);
                dungeon.AddRoom(node, room);
            }
        }
    }
}
