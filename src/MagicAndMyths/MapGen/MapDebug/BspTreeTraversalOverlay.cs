using System.Linq;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace MagicAndMyths
{
    public class BspTreeTraversalOverlay : DungeonOverlay
    {
        private Dictionary<BspNode, Color> nodeColors = new Dictionary<BspNode, Color>();
        private Dictionary<BspNode, int> nodeDepths = new Dictionary<BspNode, int>();
        private BspNode rootNode;

        public override string Label => "Show BSP Tree Structure";
        public override string Symbol => "T";
        public override Color Color => Color.cyan;

        public BspTreeTraversalOverlay(MapComp_DungeonGenDebugger debugger) : base(debugger) { }

        public override IEnumerable<IntVec3> GetCells()
        {
            return Enumerable.Empty<IntVec3>();
        }

        public override void Draw()
        {
            if (!IsEnabled || dungeon == null) return;

            if (rootNode == null)
            {
                rootNode = FindRootNode();
                if (rootNode == null) return;
            }

            if (nodeColors.Count == 0)
            {
                BuildNodeMaps(rootNode, 0);
            }

            DrawNodeRecursive(rootNode);
        }

        private BspNode FindRootNode()
        {
            return debugger.Dungeon.RootNode;
        }

        private void BuildNodeMaps(BspNode node, int depth)
        {
            if (node == null) return;

            float hue = (depth * 0.15f) % 1f;
            Color color = Color.HSVToRGB(hue, 0.7f, 0.9f);

            nodeColors[node] = color;
            nodeDepths[node] = depth;

            BuildNodeMaps(node.left, depth + 1);
            BuildNodeMaps(node.right, depth + 1);
        }

        private void DrawNodeRecursive(BspNode node)
        {
            if (node == null) return;

            Color nodeColor = nodeColors.ContainsKey(node) ? nodeColors[node] : Color.white;
            int depth = nodeDepths.ContainsKey(node) ? nodeDepths[node] : 0;

            DrawNodeRect(node.rect, nodeColor, depth);

            if (node.left != null)
            {
                DrawConnectionLine(node, node.left);
            }
            if (node.right != null)
            {
                DrawConnectionLine(node, node.right);
            }

            DrawNodeLabel(node, nodeColor, depth);

            DrawNodeRecursive(node.left);
            DrawNodeRecursive(node.right);
        }

        private void DrawNodeRect(CellRect rect, Color color, int depth)
        {
            var corners = new[]
            {
                new IntVec3(rect.minX, 0, rect.minZ),
                new IntVec3(rect.maxX, 0, rect.minZ),
                new IntVec3(rect.maxX, 0, rect.maxZ),
                new IntVec3(rect.minX, 0, rect.maxZ)
            };

            float lineWidth = Mathf.Max(0.1f, 0.5f - (depth * 0.05f));

            for (int i = 0; i < corners.Length; i++)
            {
                var start = corners[i];
                var end = corners[(i + 1) % corners.Length];

                if (start.InBounds(map) && end.InBounds(map))
                {
                    Vector3 startPos = start.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
                    Vector3 endPos = end.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);

                    GenDraw.DrawLineBetween(startPos, endPos);
                }
            }
        }

        private void DrawConnectionLine(BspNode parent, BspNode child)
        {
            Vector3 parentCenter = parent.rect.CenterCell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
            Vector3 childCenter = child.rect.CenterCell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
            Color connectionColor = Color.white * 0.5f;
            GenDraw.DrawLineBetween(parentCenter, childCenter);
        }

        private void DrawNodeLabel(BspNode node, Color color, int depth)
        {
            IntVec3 center = node.rect.CenterCell;
            Vector2 screenPos = GetCellScreenPosition(center);

            if (!IsPositionVisible(screenPos)) return;

            string label = $"D{depth}";
            if (node.IsLeaf())
            {
                label += " [LEAF]";
                if (node.HasTag("keep")) label += "\n[KEEP]";
                if (node.HasTag("side_path")) label += "\n[SIDE]";
            }
            else
            {
                label += $"\n{node.rect.Width}x{node.rect.Height}";
            }

            Vector2 labelSize = Text.CalcSize(label);
            Rect bgRect = new Rect(screenPos.x - labelSize.x / 2f - 2f,
                                  screenPos.y - labelSize.y / 2f - 2f,
                                  labelSize.x + 4f,
                                  labelSize.y + 4f);
            GUI.color = Color.black * 0.8f;
            GUI.DrawTexture(bgRect, BaseContent.WhiteTex);

            GUI.color = color;
            Rect labelRect = new Rect(screenPos.x - labelSize.x / 2f,
                                     screenPos.y - labelSize.y / 2f,
                                     labelSize.x,
                                     labelSize.y);
            Widgets.Label(labelRect, label);
            GUI.color = Color.white;
        }
    }
}