using System.Linq;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace MagicAndMyths
{
    public class CorridorOverlay : DungeonOverlay
    {
        public override string Label => "Show Corridor Cells";
        public override string Symbol => "C";
        public override Color Color => Color.cyan;

        public CorridorOverlay(MapComp_DungeonGenDebugger debugger) : base(debugger) { }

        public override IEnumerable<IntVec3> GetCells()
        {
            if (dungeon?.ConnectionManager?.AllConnections == null)
                return Enumerable.Empty<IntVec3>();

            var corridorCells = new HashSet<IntVec3>();
            foreach (var connection in dungeon.ConnectionManager.AllConnections)
            {
                foreach (var cell in connection.GetAllCells())
                {
                    corridorCells.Add(cell);
                }
            }
            return corridorCells;
        }
    }
}