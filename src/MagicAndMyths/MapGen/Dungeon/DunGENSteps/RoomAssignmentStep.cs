using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class RoomAssignmentStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            AssignRoomTypes(context, context.Def.availableRoomTypes);
        }

        private void AssignRoomTypes(DungeonGenerationContext context, List<RoomLayoutData> availableRoomTypes)
        {
            if (availableRoomTypes == null || availableRoomTypes.Count == 0)
                return;

            context.Dungeon.LeafNodes.Sort((a, b) =>
                (b.rect.Width * b.rect.Height).CompareTo(a.rect.Width * a.rect.Height));

            HashSet<BspNode> assignedNodes = new HashSet<BspNode>();

            var roomTypesWithRequirements = availableRoomTypes.Where(r => r.minSizeRequired != IntVec2.Invalid).ToList();
            var flexibleRoomTypes = availableRoomTypes.Where(r => r.minSizeRequired == IntVec2.Invalid).ToList();

            foreach (var roomType in roomTypesWithRequirements)
            {
                BspNode bestNode = context.Dungeon.LeafNodes.FirstOrDefault(node =>
                    !assignedNodes.Contains(node) &&
                    node.rect.Width >= roomType.minSizeRequired.x + (context.Def.minRoomPadding * 2) &&
                    node.rect.Height >= roomType.minSizeRequired.z + (context.Def.minRoomPadding * 2));

                if (bestNode != null)
                {
                    DungeonRoom room = DungeonRoom.FromBspNode(context.Dungeon, bestNode, context, roomType);
                    context.Dungeon.AddRoom(bestNode, room);
                    assignedNodes.Add(bestNode);
                }
            }

            foreach (var node in context.Dungeon.LeafNodes)
            {
                if (assignedNodes.Contains(node))
                    continue;

                var roomTypeToUse = flexibleRoomTypes.Any() ? flexibleRoomTypes.RandomElement() : availableRoomTypes.RandomElement();
                DungeonRoom room = DungeonRoom.FromBspNode(context.Dungeon, node, context, roomTypeToUse);
                context.Dungeon.AddRoom(node, room);
            }
        }
    }
}
