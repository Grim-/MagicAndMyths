using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
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