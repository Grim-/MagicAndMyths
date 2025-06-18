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
                DungeonRoom lastRoomInChain = chain.Last();
                if (lastRoomInChain != null)
                {
                    if (Rand.Value < hiddenChance)
                    {
                        context.Dungeon.MarkRoomAsHidden(lastRoomInChain);
                    }
                }
            }
        }
    }
}