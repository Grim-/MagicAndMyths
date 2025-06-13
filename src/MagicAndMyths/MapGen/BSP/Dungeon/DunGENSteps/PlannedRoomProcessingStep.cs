using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class PlannedRoomProcessingStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            ProcessPlannedRooms(context, context.Def.availableRoomTypes);
        }

        public void ProcessPlannedRooms(DungeonGenerationContext context, List<RoomTypeDef> plannedRooms)
        {
            if (plannedRooms == null || plannedRooms.Count == 0)
                return;

            context.Dungeon.LeafNodes.Sort((a, b) =>
                (b.rect.Width * b.rect.Height).CompareTo(a.rect.Width * a.rect.Height));

            HashSet<BspNode> assignedNodes = new HashSet<BspNode>();

            foreach (var roomType in plannedRooms)
            {
                if (roomType.minSize == IntVec2.Invalid)
                    continue;

                BspNode bestNode = null;
                foreach (var node in context.Dungeon.LeafNodes)
                {
                    if (assignedNodes.Contains(node))
                        continue;

                    if (node.rect.Width >= roomType.minSize.x + (context.Def.minRoomPadding * 2) &&
                        node.rect.Height >= roomType.minSize.z + (context.Def.minRoomPadding * 2))
                    {
                        bestNode = node;
                        break;
                    }
                }

                if (bestNode != null)
                {
                    bestNode.roomRect = bestNode.GenerateRoomGeometryWithSize(
                        roomType.minSize.x, roomType.minSize.z, context.Def.minRoomPadding);

                    DungeonRoom room = DungeonRoom.FromBspNode(context.Dungeon, bestNode);
                    room.def = roomType;
                    context.Dungeon.AddRoom(bestNode, room);
                    assignedNodes.Add(bestNode);
                }
            }

            foreach (var node in context.Dungeon.LeafNodes)
            {
                if (assignedNodes.Contains(node) || context.Dungeon.HasMapping(node))
                    continue;
                DungeonRoom room = DungeonRoom.FromBspNode(context.Dungeon, node);
                context.Dungeon.AddRoom(node, room);
            }
        }
    }
}
