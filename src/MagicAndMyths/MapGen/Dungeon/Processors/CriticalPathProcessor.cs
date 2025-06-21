using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{

    /// <summary>
    /// Designates the dungeons critical path - the path between the start node and end node
    /// </summary>
    public class CriticalPathProcessor
    {
        private readonly Dungeon dungeon;
        private readonly DungeonGenerationContext context;
        public CriticalPathProcessor(DungeonGenerationContext context, Dungeon dungeon)
        {
            this.context = context;
            this.dungeon = dungeon;
        }

        public void DesignateCriticalPath(DungeonGenerationContext context, Dungeon dungeon)
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
                path[i].RemoveTag("side_path");
                path[i].AddTag("main_path");
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

    public class CriticalPathProcessorMainPathLimited
    {
        private readonly Dungeon dungeon;
        private readonly DungeonGenerationContext context;

        public CriticalPathProcessorMainPathLimited(DungeonGenerationContext context, Dungeon dungeon)
        {
            this.context = context;
            this.dungeon = dungeon;
        }
        //get path between a random node and the furthest node from that
        //take the first mainPathLength of nodes, the first is the start, the last is the end, the rooms in between all should still equal a valid traversable start -> end critical path
        public void DesignateCriticalPath(DungeonGenerationContext context, Dungeon dungeon)
        {
            int mainPathLength = context.MainPathLength;
            DungeonRoom firstRoom = dungeon.GetAllRooms().RandomElement();
            DungeonRoom furthestFromFirst = dungeon.GetFurthestRoom(firstRoom);
            DungeonRoom startRoom = furthestFromFirst;
            DungeonRoom endRoom = dungeon.GetFurthestRoom(startRoom);


            startRoom.def = MagicAndMythDefOf.StartRoom;
            startRoom.AddTag("start");
            startRoom.SetCriticalPathIndex(0);

            BspNode startNode = dungeon.GetNode(startRoom);

            List<DungeonRoom> path = dungeon.FindPathBetween(startRoom, endRoom);
            path = path.Take(mainPathLength).ToList();


            for (int i = 0; i < path.Count; i++)
            {
                path[i].SetCriticalPathIndex(i);
                path[i].RemoveTag("side_path");
                path[i].AddTag("main_path");
            }

            DungeonRoom actualEndRoom = path.Last();
            actualEndRoom.def = MagicAndMythDefOf.EndRoom;
            actualEndRoom.AddTag("end");
            actualEndRoom.SetCriticalPathIndex(path.Count - 1);

            BspNode endNode = dungeon.GetNode(actualEndRoom);
            dungeon.SetCriticalPathEndpoints(startNode, endNode);

            for (int i = 0; i < path.Count - 1; i++)
            {
                var a = path[i];
                var b = path[i + 1];
                dungeon.ConnectRooms(a, b);
            }

        }

    }

    public class CriticalPathProcessorBestForSideRooms
    {
        private readonly Dungeon dungeon;
        private readonly DungeonGenerationContext context;

        public CriticalPathProcessorBestForSideRooms(DungeonGenerationContext context, Dungeon dungeon)
        {
            this.context = context;
            this.dungeon = dungeon;
        }

        public void DesignateCriticalPath(DungeonGenerationContext context, Dungeon dungeon)
        {
            int mainPathLength = context.MainPathLength;
            var allRooms = dungeon.GetAllRooms().ToList();

            List<List<DungeonRoom>> candidatePaths = new List<List<DungeonRoom>>();

            // Try multiple different start points
            for (int attempt = 0; attempt < 10; attempt++)
            {
                DungeonRoom startCandidate = allRooms.RandomElement();
                DungeonRoom endCandidate = dungeon.GetFurthestRoom(startCandidate);

                List<DungeonRoom> fullPath = dungeon.FindPathBetween(startCandidate, endCandidate);

                if (fullPath.Count >= mainPathLength)
                {
                    List<DungeonRoom> truncatedPath = fullPath.Take(mainPathLength).ToList();
                    candidatePaths.Add(truncatedPath);
                }
            }

            if (candidatePaths.Count == 0)
            {
                // Fallback to original approach
                var fallback = new CriticalPathProcessor(context, dungeon);
                fallback.DesignateCriticalPath(context, dungeon);
                return;
            }

            // Score each path by how many unused rooms it leaves
            var bestPath = candidatePaths
                .OrderByDescending(path => allRooms.Count - path.Count)
                .ThenByDescending(path => CountPotentialSideConnections(path, allRooms))
                .First();

            ApplyPath(bestPath, dungeon);
        }

        private int CountPotentialSideConnections(List<DungeonRoom> mainPath, List<DungeonRoom> allRooms)
        {
            var sideRooms = allRooms.Where(r => !mainPath.Contains(r)).ToList();
            int connections = 0;

            foreach (var mainRoom in mainPath)
            {
                connections += mainRoom.connectedRooms.Count(r => sideRooms.Contains(r));
            }

            return connections;
        }

        private void ApplyPath(List<DungeonRoom> path, Dungeon dungeon)
        {
            DungeonRoom startRoom = path.First();
            DungeonRoom endRoom = path.Last();

            startRoom.def = MagicAndMythDefOf.StartRoom;
            startRoom.AddTag("start");
            startRoom.SetCriticalPathIndex(0);

            endRoom.def = MagicAndMythDefOf.EndRoom;
            endRoom.AddTag("end");

            for (int i = 0; i < path.Count; i++)
            {
                path[i].SetCriticalPathIndex(i);
                path[i].RemoveTag("side_path");
                path[i].AddTag("main_path");
            }

            endRoom.SetCriticalPathIndex(path.Count - 1);

            // Connect the path rooms
            for (int i = 0; i < path.Count - 1; i++)
            {
                dungeon.ConnectRooms(path[i], path[i + 1]);
            }

            BspNode startNode = dungeon.GetNode(startRoom);
            BspNode endNode = dungeon.GetNode(endRoom);
            dungeon.SetCriticalPathEndpoints(startNode, endNode);
        }
    }

    /// <summary>
    /// Build path by extending from both ends until you hit target length
    /// </summary>
    public class CriticalPathProcessorBidirectional
    {
        private readonly Dungeon dungeon;
        private readonly DungeonGenerationContext context;

        public CriticalPathProcessorBidirectional(DungeonGenerationContext context, Dungeon dungeon)
        {
            this.context = context;
            this.dungeon = dungeon;
        }

        public void DesignateCriticalPath(DungeonGenerationContext context, Dungeon dungeon)
        {
            int mainPathLength = context.MainPathLength;
            var allRooms = dungeon.GetAllRooms().ToList();

            // Start with two far apart rooms
            DungeonRoom firstRoom = allRooms.RandomElement();
            DungeonRoom lastRoom = dungeon.GetFurthestRoom(firstRoom);

            List<DungeonRoom> path = new List<DungeonRoom> { firstRoom, lastRoom };
            HashSet<DungeonRoom> usedRooms = new HashSet<DungeonRoom> { firstRoom, lastRoom };

            // Extend from both ends alternately
            bool extendFromStart = true;

            while (path.Count < mainPathLength && usedRooms.Count < allRooms.Count)
            {
                DungeonRoom targetRoom = extendFromStart ? path.First() : path.Last();

                var connectedUnused = targetRoom.connectedRooms
                    .Where(r => !usedRooms.Contains(r))
                    .ToList();

                if (connectedUnused.Count > 0)
                {
                    DungeonRoom newRoom = connectedUnused.RandomElement();

                    if (extendFromStart)
                        path.Insert(0, newRoom);
                    else
                        path.Add(newRoom);

                    usedRooms.Add(newRoom);
                }

                extendFromStart = !extendFromStart;

                // Safety break if we can't extend from either end
                if (path.First().connectedRooms.All(r => usedRooms.Contains(r)) &&
                    path.Last().connectedRooms.All(r => usedRooms.Contains(r)))
                {
                    break;
                }
            }

            ApplyPath(path, dungeon);
        }

        private void ApplyPath(List<DungeonRoom> path, Dungeon dungeon)
        {
            DungeonRoom startRoom = path.First();
            DungeonRoom endRoom = path.Last();

            startRoom.def = MagicAndMythDefOf.StartRoom;
            startRoom.AddTag("start");
            startRoom.SetCriticalPathIndex(0);

            endRoom.def = MagicAndMythDefOf.EndRoom;
            endRoom.AddTag("end");

            for (int i = 0; i < path.Count; i++)
            {
                path[i].SetCriticalPathIndex(i);
                path[i].RemoveTag("side_path");
                path[i].AddTag("main_path");
            }

            endRoom.SetCriticalPathIndex(path.Count - 1);

            BspNode startNode = dungeon.GetNode(startRoom);
            BspNode endNode = dungeon.GetNode(endRoom);
            dungeon.SetCriticalPathEndpoints(startNode, endNode);
        }
    }

    /// <summary>
    /// Pick any X connected rooms that form a valid path
    /// </summary>
    public class CriticalPathProcessorFlexibleLength
    {
        private readonly Dungeon dungeon;
        private readonly DungeonGenerationContext context;

        public CriticalPathProcessorFlexibleLength(DungeonGenerationContext context, Dungeon dungeon)
        {
            this.context = context;
            this.dungeon = dungeon;
        }

        public void DesignateCriticalPath(DungeonGenerationContext context, Dungeon dungeon)
        {
            int mainPathLength = context.MainPathLength;
            var allRooms = dungeon.GetAllRooms().ToList();

            // Try different starting points to find a good path
            List<DungeonRoom> bestPath = null;
            int bestSideRoomCount = 0;

            for (int attempt = 0; attempt < 5; attempt++)
            {
                var path = BuildPathOfLength(allRooms, mainPathLength);
                if (path != null)
                {
                    int sideRoomCount = allRooms.Count - path.Count;
                    if (sideRoomCount > bestSideRoomCount)
                    {
                        bestPath = path;
                        bestSideRoomCount = sideRoomCount;
                    }
                }
            }

            if (bestPath == null)
            {
                // Fallback - just take any valid path and truncate
                DungeonRoom start = allRooms.RandomElement();
                DungeonRoom end = dungeon.GetFurthestRoom(start);
                bestPath = dungeon.FindPathBetween(start, end).Take(mainPathLength).ToList();
            }

            ApplyPath(bestPath, dungeon);
        }

        private List<DungeonRoom> BuildPathOfLength(List<DungeonRoom> allRooms, int targetLength)
        {
            DungeonRoom start = allRooms.RandomElement();
            List<DungeonRoom> path = new List<DungeonRoom> { start };
            HashSet<DungeonRoom> used = new HashSet<DungeonRoom> { start };

            while (path.Count < targetLength)
            {
                DungeonRoom current = path.Last();
                var nextOptions = current.connectedRooms.Where(r => !used.Contains(r)).ToList();

                if (nextOptions.Count == 0)
                    break;

                DungeonRoom next = nextOptions.RandomElement();
                path.Add(next);
                used.Add(next);
            }

            return path.Count == targetLength ? path : null;
        }

        private void ApplyPath(List<DungeonRoom> path, Dungeon dungeon)
        {
            DungeonRoom startRoom = path.First();
            DungeonRoom endRoom = path.Last();

            startRoom.def = MagicAndMythDefOf.StartRoom;
            startRoom.AddTag("start");
            startRoom.SetCriticalPathIndex(0);

            endRoom.def = MagicAndMythDefOf.EndRoom;
            endRoom.AddTag("end");

            for (int i = 0; i < path.Count; i++)
            {
                path[i].SetCriticalPathIndex(i);
                path[i].RemoveTag("side_path");
                path[i].AddTag("main_path");
            }

            endRoom.SetCriticalPathIndex(path.Count - 1);

            // Ensure the path is connected
            for (int i = 0; i < path.Count - 1; i++)
            {
                if (!path[i].connectedRooms.Contains(path[i + 1]))
                {
                    dungeon.ConnectRooms(path[i], path[i + 1]);
                }
            }

            BspNode startNode = dungeon.GetNode(startRoom);
            BspNode endNode = dungeon.GetNode(endRoom);
            dungeon.SetCriticalPathEndpoints(startNode, endNode);
        }
    }
}