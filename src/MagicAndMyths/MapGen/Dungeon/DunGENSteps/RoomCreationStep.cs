using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class RoomCreationStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            AssignRoomTypes(context, context.Def.availableRoomTypes);
        }

        private void AssignRoomTypes(DungeonGenerationContext context, List<RoomLayoutData> availableRoomTypes)
        {
            if (availableRoomTypes == null || availableRoomTypes.Count == 0)
                return;

            SortNodesBySize(context);
            var assignedNodes = new HashSet<BspNode>();
            var roomTypeCount = InitializeRoomTypeCounts(availableRoomTypes);

            AssignRoomsWithRequirements(context, availableRoomTypes, assignedNodes, roomTypeCount);
            AssignRemainingRooms(context, availableRoomTypes, assignedNodes, roomTypeCount);
        }

        private void SortNodesBySize(DungeonGenerationContext context)
        {
            context.Dungeon.LeafNodes.Sort((a, b) =>
                (b.rect.Width * b.rect.Height).CompareTo(a.rect.Width * a.rect.Height));
        }

        private Dictionary<RoomLayoutData, int> InitializeRoomTypeCounts(List<RoomLayoutData> availableRoomTypes)
        {
            var roomTypeCount = new Dictionary<RoomLayoutData, int>();
            foreach (var roomType in availableRoomTypes)
            {
                roomTypeCount[roomType] = 0;
            }
            return roomTypeCount;
        }

        private void AssignRoomsWithRequirements(DungeonGenerationContext context, List<RoomLayoutData> availableRoomTypes,
            HashSet<BspNode> assignedNodes, Dictionary<RoomLayoutData, int> roomTypeCount)
        {
            var roomTypesWithRequirements = availableRoomTypes.Where(r => r.minSizeRequired != IntVec2.Invalid).ToList();

            foreach (var roomType in roomTypesWithRequirements)
            {
                if (roomTypeCount[roomType] >= roomType.def.maxRoomTypeCount)
                    continue;

                var bestNode = FindSuitableNode(context, assignedNodes, roomType);
                if (bestNode != null)
                {
                    CreateAndAddRoom(context, bestNode, roomType, assignedNodes, roomTypeCount);
                }
            }
        }

        private void AssignRemainingRooms(DungeonGenerationContext context, List<RoomLayoutData> availableRoomTypes,
            HashSet<BspNode> assignedNodes, Dictionary<RoomLayoutData, int> roomTypeCount)
        {
            var flexibleRoomTypes = availableRoomTypes.Where(r => r.minSizeRequired == IntVec2.Invalid).ToList();

            foreach (var node in context.Dungeon.LeafNodes)
            {
                if (assignedNodes.Contains(node))
                    continue;

                var roomTypeToUse = SelectRoomType(context, availableRoomTypes, flexibleRoomTypes, roomTypeCount);
                CreateAndAddRoom(context, node, roomTypeToUse, assignedNodes, roomTypeCount);
            }
        }

        private BspNode FindSuitableNode(DungeonGenerationContext context, HashSet<BspNode> assignedNodes, RoomLayoutData roomType)
        {
            return context.Dungeon.LeafNodes.FirstOrDefault(node =>
                !assignedNodes.Contains(node) &&
                node.rect.Width >= roomType.minSizeRequired.x + (context.Def.minRoomPadding * 2) &&
                node.rect.Height >= roomType.minSizeRequired.z + (context.Def.minRoomPadding * 2));
        }

        private RoomLayoutData SelectRoomType(DungeonGenerationContext context, List<RoomLayoutData> availableRoomTypes, List<RoomLayoutData> flexibleRoomTypes,
            Dictionary<RoomLayoutData, int> roomTypeCount)
        {
            var availableFlexibleTypes = flexibleRoomTypes.Where(r => roomTypeCount[r] < r.def.maxRoomTypeCount).ToList();
            if (availableFlexibleTypes.Any())
                return availableFlexibleTypes.RandomElement();

            var availableTypes = availableRoomTypes.Where(r => roomTypeCount[r] < r.def.maxRoomTypeCount).ToList();
            if (availableTypes.Any())
                return availableTypes.RandomElement();

            return availableRoomTypes.RandomElement();
        }

        private void CreateAndAddRoom(DungeonGenerationContext context, BspNode node, RoomLayoutData roomType,
            HashSet<BspNode> assignedNodes, Dictionary<RoomLayoutData, int> roomTypeCount)
        {
            var room = DungeonRoom.FromBspNode(context.Dungeon, node, context, roomType);
            context.Dungeon.AddRoom(node, room);
            assignedNodes.Add(node);
            roomTypeCount[roomType]++;
        }
    }
}