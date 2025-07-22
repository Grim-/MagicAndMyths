using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace MagicAndMyths
{
    public class ProtectionOverlay : DungeonOverlay
    {
        public override string Label => "Show Protection Grid";
        public override string Symbol => "P";
        public override Color Color => Color.blue;

        public ProtectionOverlay(MapComp_DungeonGenDebugger debugger) : base(debugger) { }

        public override IEnumerable<IntVec3> GetCells()
        {
            if (dungeon?.GridManager?.ProtectionGrid == null)
                yield break;

            foreach (IntVec3 cell in map.AllCells)
            {
                if (cell.InBounds(map) && dungeon.GridManager.ProtectionGrid[cell])
                    yield return cell;
            }
        }
    }
}