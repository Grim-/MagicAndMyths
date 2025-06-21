using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class RoomTypeAssignmentStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            Log.Message("Assigning room types");
            AssignRoomTypes(context);
        }

        public void AssignRoomTypes(DungeonGenerationContext context)
        {
            foreach (var node in context.Dungeon.LeafNodes)
            {
                DungeonRoom room = context.Dungeon.GetRoom(node);
                if (room.def != null)
                    continue;

                if (room.IsOnCriticalPath)
                {
                    AssignCriticalPathRoomType(context, node, room);
                }
                else
                {
                    AssignSidePathRoomType(context, node, room);
                }

                if (context.Dungeon.StartNode != null)
                {
                    float distance = Vector3.Distance(
                        room.Center.ToVector3(),
                        context.Dungeon.GetRoom(context.Dungeon.StartNode).Center.ToVector3());
                    room.distanceFromStart = distance;
                }
            }
        }

        private void AssignCriticalPathRoomType(DungeonGenerationContext context, BspNode node, DungeonRoom room)
        {
            if (!Rand.Chance(context.Def.noRoomChanceCriticalPath))
            {
                var criticalRooms = context.Def.GetRoomTypeDef(context, room);
                if (criticalRooms != null)
                {
                    room.def = criticalRooms.def;
                }
                else
                {
                    Log.Error("No critical path room types found!");
                }
            }
        }

        private void AssignSidePathRoomType(DungeonGenerationContext context, BspNode node, DungeonRoom room)
        {
            if (!Rand.Chance(context.Def.noRoomChanceSidePath))
            {
                var sideRooms = context.Def.GetSideRoomTypeDef(context, room);

                if (sideRooms != null)
                {
                    room.def = sideRooms.def;
                    CreateAndAddRoom(context, node, sideRooms);
                }
                else
                {
                    Log.Error("No side path room types found!");
                }
            }
        }

        private void CreateAndAddRoom(DungeonGenerationContext context, BspNode node, RoomLayoutData roomType)
        {
            var room = DungeonRoom.FromBspNode(context.Dungeon, node, context, roomType);
            context.Dungeon.AddRoom(node, room);
        }
    }
}