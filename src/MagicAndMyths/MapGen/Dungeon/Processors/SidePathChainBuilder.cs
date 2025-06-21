using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
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

            if (availableRooms.Count == 0) 
                return chain;

            var startRoom = availableRooms.Where(x=> x.def.roomType != RoomType.End).RandomElement();
            chain.Add(startRoom);
            var currentRoom = startRoom;

            for (int i = 1; i < targetLength && chain.Count < availableRooms.Count; i++)
            {
                var nextRoom = SelectNextRoomInChain(currentRoom, availableRooms, usedRooms, chain, criticalPathRooms);
                if (nextRoom == null) 
                    break;

                context.Dungeon.ConnectRooms(currentRoom, nextRoom);
                chain.Add(nextRoom);
                currentRoom = nextRoom;

                if (context.Def.allowBranchingSidePaths && Rand.Chance(context.Def.branchingChance))
                {
                    if (currentRoom.def.roomType != RoomType.End)
                    {
                        CreateSidePathBranch(currentRoom, availableRooms, usedRooms, chain, criticalPathRooms);
                    }
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

            if (candidates.Count == 0) 
                return null;

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
}