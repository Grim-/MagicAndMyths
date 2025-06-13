using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class SidePathProcessor
    {
        private readonly DungeonGenerationContext context;

        public SidePathProcessor(DungeonGenerationContext context)
        {
            this.context = context;
        }

        public void ProcessSidePaths()
        {
            var criticalPathRooms = context.Dungeon.GetAllRooms()
                .Where(r => r.IsOnCriticalPath)
                .OrderBy(r => r.CriticalPathIndex)
                .ToList();

            var sidePathNodes = context.Dungeon.GetAllRooms()
                .Where(r => !r.IsOnCriticalPath)
                .ToList();

            var sidePathRooms = new List<DungeonRoom>();

            foreach (var room in sidePathNodes)
            {
                //DungeonRoom room = context.Dungeon.GetRoom(node);
                if (room != null)
                {
                    room.AddTag("side_path");
                    sidePathRooms.Add(room);
                }
            }

            if (sidePathRooms.Count == 0)
                return;

            var chainBuilder = new SidePathChainBuilder(context);
            var sidePathChains = chainBuilder.CreateSidePathChains(sidePathRooms, criticalPathRooms);

            var chainConnector = new SidePathChainConnector(context);
            chainConnector.ConnectSidePathChainsToMainPath(criticalPathRooms, sidePathChains);

            if (context.Def.allowHiddenSidePaths && context.Def.hiddenSidePathChance > 0)
            {
                HideRandomSidePathChains(sidePathChains, context.Def.hiddenSidePathChance);
            }
        }

        public void EnsureAllRoomsConnected()
        {
            foreach (var room in context.Dungeon.GetAllRooms())
            {
                if (room.connectedRooms == null || room.connectedRooms.Count == 0)
                {
                    var otherRoom = context.Dungeon.GetAllRooms()
                        .Where(r => r != room && !r.IsOnCriticalPath)
                        .RandomElement();
                    room.connectedRooms = new List<DungeonRoom> { otherRoom };
                    otherRoom.connectedRooms.Add(room);
                }
            }
        }


        public void HideRandomSidePathChains(List<List<DungeonRoom>> chains, float hiddenChance)
        {
            foreach (var chain in chains)
            {
                if (Rand.Value < hiddenChance)
                {
                    foreach (var room in chain)
                    {
                        context.Dungeon.MarkRoomAsHidden(room);
                    }
                }
            }
        }
    }

    public class SidePathChainBuilder
    {
        private readonly DungeonGenerationContext context;

        public SidePathChainBuilder(DungeonGenerationContext context)
        {
            this.context = context;
        }

        public List<List<DungeonRoom>> CreateSidePathChains(List<DungeonRoom> sidePathRooms, List<DungeonRoom> criticalPathRooms)
        {
            var chains = new List<List<DungeonRoom>>();
            var usedRooms = new HashSet<DungeonRoom>();

            while (sidePathRooms.Any(r => !usedRooms.Contains(r)))
            {
                var availableRooms = sidePathRooms.Where(r => !usedRooms.Contains(r)).ToList();
                var chainLength = DetermineChainLength();
                var chain = BuildSidePathChain(availableRooms, chainLength, usedRooms, criticalPathRooms);

                if (chain.Count > 0)
                {
                    chains.Add(chain);
                    foreach (var room in chain)
                    {
                        usedRooms.Add(room);
                    }
                }
                else
                {
                    break;
                }
            }

            return chains;
        }

        private int DetermineChainLength()
        {
            if (Rand.Value < context.Def.longSidePathChance)
            {
                return context.Def.sidePathLength.RandomInRange;
            }
            return 1;
        }

        private List<DungeonRoom> BuildSidePathChain(List<DungeonRoom> availableRooms, int targetLength,
            HashSet<DungeonRoom> usedRooms, List<DungeonRoom> criticalPathRooms)
        {
            var chain = new List<DungeonRoom>();

            if (availableRooms.Count == 0) return chain;

            var startRoom = availableRooms.RandomElement();
            chain.Add(startRoom);
            var currentRoom = startRoom;

            for (int i = 1; i < targetLength && chain.Count < availableRooms.Count; i++)
            {
                var nextRoom = SelectNextRoomInChain(currentRoom, availableRooms, usedRooms, chain, criticalPathRooms);
                if (nextRoom == null) break;

                context.Dungeon.ConnectRooms(currentRoom, nextRoom);
                chain.Add(nextRoom);
                currentRoom = nextRoom;

                if (context.Def.allowBranchingSidePaths && Rand.Chance(context.Def.branchingChance))
                {
                    CreateSidePathBranch(currentRoom, availableRooms, usedRooms, chain, criticalPathRooms);
                }
            }

            return chain;
        }

        private DungeonRoom SelectNextRoomInChain(DungeonRoom currentRoom, List<DungeonRoom> availableRooms,
            HashSet<DungeonRoom> usedRooms, List<DungeonRoom> chainSoFar, List<DungeonRoom> criticalPathRooms)
        {
            var candidates = availableRooms
                .Where(r => !usedRooms.Contains(r) && !chainSoFar.Contains(r))
                .ToList();

            if (candidates.Count == 0) return null;

            if (Rand.Value < context.Def.meanderingChance)
            {
                return candidates.RandomElement();
            }
            else
            {
                return candidates
                    .OrderBy(r => (r.Center - currentRoom.Center).LengthHorizontalSquared)
                    .FirstOrDefault();
            }
        }

        private void CreateSidePathBranch(DungeonRoom branchPoint, List<DungeonRoom> availableRooms,
            HashSet<DungeonRoom> usedRooms, List<DungeonRoom> mainChain, List<DungeonRoom> criticalPathRooms)
        {
            var branchLength = Rand.Range(1, Math.Max(2, context.Def.sidePathLength.max / 2));
            var branch = BuildSidePathChain(availableRooms, branchLength, usedRooms, criticalPathRooms);

            if (branch.Count > 0)
            {
                context.Dungeon.ConnectRooms(branchPoint, branch[0]);
                foreach (var room in branch)
                {
                    usedRooms.Add(room);
                    room.AddTag("side_path_branch");
                }
            }
        }
    }

    public class SidePathChainConnector
    {
        private readonly DungeonGenerationContext context;

        public SidePathChainConnector(DungeonGenerationContext context)
        {
            this.context = context;
        }

        public void ConnectSidePathChainsToMainPath(List<DungeonRoom> criticalPathRooms, List<List<DungeonRoom>> chains)
        {
            if (!chains.Any() || !criticalPathRooms.Any()) return;

            foreach (var chain in chains)
            {
                if (chain.Count == 0) continue;

                var chainEntrance = SelectChainEntrance(chain, criticalPathRooms);
                var nearestMainRoom = criticalPathRooms
                    .OrderBy(r => (r.Center - chainEntrance.Center).LengthHorizontalSquared)
                    .First();

                context.Dungeon.ConnectRooms(chainEntrance, nearestMainRoom);

                PreventForwardConnectionsForChain(chain, nearestMainRoom, criticalPathRooms);
            }
        }

        private DungeonRoom SelectChainEntrance(List<DungeonRoom> chain, List<DungeonRoom> criticalPathRooms)
        {
            return chain
                .OrderBy(r => criticalPathRooms.Min(cr => (cr.Center - r.Center).LengthHorizontalSquared))
                .First();
        }

        private void PreventForwardConnectionsForChain(List<DungeonRoom> chain, DungeonRoom connectedMainRoom,
            List<DungeonRoom> criticalPathRooms)
        {
            var forwardRooms = criticalPathRooms
                .Where(r => r.CriticalPathIndex > connectedMainRoom.CriticalPathIndex)
                .ToList();

            foreach (var chainRoom in chain)
            {
                foreach (var forwardRoom in forwardRooms)
                {
                    if (chainRoom.IsConnectedTo(forwardRoom))
                    {
                        chainRoom.connectedRooms.Remove(forwardRoom);
                        forwardRoom.connectedRooms.Remove(chainRoom);

                        var connection = chainRoom.connections.FirstOrDefault(c => c.DestinationRoom == forwardRoom);
                        if (connection != null)
                        {
                            chainRoom.connections.Remove(connection);
                        }

                        connection = forwardRoom.connections.FirstOrDefault(c => c.DestinationRoom == chainRoom);
                        if (connection != null)
                        {
                            forwardRoom.connections.Remove(connection);
                        }
                    }
                }
            }
        }
    }
}