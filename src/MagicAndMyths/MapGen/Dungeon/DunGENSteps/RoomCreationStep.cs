//using System.Collections.Generic;
//using Verse;

//namespace MagicAndMyths
//{
//    public class RoomCreationStep : IDungeonGenerationStep
//    {
//        public void Execute(DungeonGenerationContext context)
//        {
//            CreateRoomsFromBspNodes(context, context.Dungeon.Def.availableRoomTypes);
//        }


//        public void CreateRoomsFromBspNodes(DungeonGenerationContext context, List<RoomLayoutData> plannedRooms)
//        {
//            foreach (var node in context.Dungeon.LeafNodes)
//            {
//                if (context.Dungeon.HasMapping(node))
//                    continue;

//                DungeonRoom room = DungeonRoom.FromBspNode(context.Dungeon, node, context, plannedRooms.RandomElement());
//                context.Dungeon.AddRoom(node, room);
//            }
//        }
//    }
//}
