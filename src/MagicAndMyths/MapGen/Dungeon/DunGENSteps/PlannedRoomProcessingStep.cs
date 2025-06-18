//using System.Collections.Generic;
//using Verse;

//namespace MagicAndMyths
//{
//    public class PlannedRoomProcessingStep : IDungeonGenerationStep
//    {
//        public void Execute(DungeonGenerationContext context)
//        {
//            ProcessPlannedRooms(context, context.Def.availableRoomTypes);
//        }

//        public void ProcessPlannedRooms(DungeonGenerationContext context, List<RoomLayoutData> plannedRooms)
//        {
//            if (plannedRooms == null || plannedRooms.Count == 0)
//                return;

//            context.Dungeon.LeafNodes.Sort((a, b) =>
//                (b.rect.Width * b.rect.Height).CompareTo(a.rect.Width * a.rect.Height));

//            HashSet<BspNode> assignedNodes = new HashSet<BspNode>();

//            foreach (var roomType in plannedRooms)
//            {
//                if (roomType.minSizeRequired == IntVec2.Invalid)
//                    continue;

//                BspNode bestNode = null;
//                foreach (var node in context.Dungeon.LeafNodes)
//                {
//                    if (assignedNodes.Contains(node))
//                        continue;

//                    if (node.rect.Width >= roomType.minSizeRequired.x + (context.Def.minRoomPadding * 2) &&
//                        node.rect.Height >= roomType.minSizeRequired.z + (context.Def.minRoomPadding * 2))
//                    {
//                        bestNode = node;
//                        break;
//                    }
//                }

//                if (bestNode != null)
//                {
//                    DungeonRoom room = DungeonRoom.FromBspNode(context.Dungeon, bestNode, context, roomType);
//                    room.def = roomType.def;
//                    context.Dungeon.AddRoom(bestNode, room);
//                    assignedNodes.Add(bestNode);
//                }
//            }
//        }
//    }
//}
