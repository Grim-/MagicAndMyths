using System.Linq;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace MagicAndMyths
{
    public class BspOverlay : DungeonOverlay
    {
        private List<Color> nodeColors = new List<Color>();

        public override string Label => "Show BSP Nodes";
        public override string Symbol => "B";
        public override Color Color => Color.magenta;

        public BspOverlay(MapComp_DungeonGenDebugger debugger) : base(debugger) { }

        public override IEnumerable<IntVec3> GetCells()
        {
            if (dungeon?.LeafNodes == null) 
                return Enumerable.Empty<IntVec3>();

            var allCells = new List<IntVec3>();
            foreach (var node in dungeon.LeafNodes)
            {
                allCells.AddRange(node.rect.Cells);
            }
            return allCells;
        }

        public override void Draw()
        {
            if (!IsEnabled || dungeon?.LeafNodes == null) return;

            if (nodeColors.Count == 0)
            {
                foreach (var node in dungeon.LeafNodes)
                {
                    nodeColors.Add(new Color(Rand.Value, Rand.Value, Rand.Value));
                }
            }

            for (int i = 0; i < dungeon.LeafNodes.Count; i++)
            {
                DrawBspNode(dungeon.LeafNodes[i], nodeColors[i]);
            }
        }

        private void DrawBspNode(BspNode node, Color nodeColor)
        {
            DrawBspNodeRect(node.rect, nodeColor);

            foreach (var cell in node.rect.Cells)
            {
                if (!cell.InBounds(map)) 
                    continue;

                Vector2 screenPos = GetCellScreenPosition(cell);
                if (!IsPositionVisible(screenPos)) 
                    continue;

                DrawSymbolAtPosition(screenPos, Symbol, nodeColor);
            }
        }

        private void DrawBspNodeRect(CellRect rect, Color color)
        {
            var corners = new[]
            {
                new IntVec3(rect.minX, 0, rect.minZ),
                new IntVec3(rect.maxX, 0, rect.minZ),
                new IntVec3(rect.maxX, 0, rect.maxZ),
                new IntVec3(rect.minX, 0, rect.maxZ)
            };

            for (int i = 0; i < corners.Length; i++)
            {
                var start = corners[i];
                var end = corners[(i + 1) % corners.Length];

                if (start.InBounds(map) && end.InBounds(map))
                {
                    Vector3 startPos = start.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
                    Vector3 endPos = end.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);

                    GenDraw.DrawLineBetween(startPos, endPos, SimpleColor.Magenta);
                }
            }
        }
    }
}