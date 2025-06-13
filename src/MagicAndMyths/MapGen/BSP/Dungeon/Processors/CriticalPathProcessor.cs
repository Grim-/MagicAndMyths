using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{

    /// <summary>
    /// Designates the dungeons critical path - the path between the start node and end node
    /// </summary>
    public class CriticalPathProcessor
    {
        private readonly Dungeon dungeon;

        public CriticalPathProcessor(Dungeon dungeon)
        {
            this.dungeon = dungeon;
        }

        public void DesignateCriticalPath()
        {
            DungeonRoom firstRoom = dungeon.GetAllRooms().RandomElement();
            DungeonRoom furthestFromFirst = dungeon.GetFurthestRoom(firstRoom);
            DungeonRoom startRoom = furthestFromFirst;
            DungeonRoom endRoom = dungeon.GetFurthestRoom(startRoom);

            startRoom.def = MagicAndMythDefOf.StartRoom;
            startRoom.AddTag("start");
            startRoom.SetCriticalPathIndex(0);

            endRoom.def = MagicAndMythDefOf.EndRoom;
            endRoom.AddTag("end");

            BspNode startNode = dungeon.GetNode(startRoom);
            BspNode endNode = dungeon.GetNode(endRoom);
            dungeon.SetCriticalPathEndpoints(startNode, endNode);

            List<DungeonRoom> path = dungeon.FindPathBetween(startRoom, endRoom);

            for (int i = 0; i < path.Count; i++)
            {
                path[i].SetCriticalPathIndex(i);

                if (path[i].HasTag("side_path"))
                {
                    path[i].tags.Remove("side_path");
                }
            }

            endRoom.SetCriticalPathIndex(path.Count - 1);

            for (int i = 0; i < path.Count - 1; i++)
            {
                var a = path[i];
                var b = path[i + 1];
                dungeon.ConnectRooms(a, b);
            }
        }
    }
}