using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public struct NetworkMessageCache
    {
        public Vector2 Start;
        public Vector2 End;
        public float TimeStamp;

        public NetworkMessageCache(Vector2 start, Vector2 end, float timeStamp)
        {
            Start = start;
            End = end;
            TimeStamp = timeStamp;
        }
    }


    public class Window_NetworkView : Window
    {
        public override Vector2 InitialSize => new Vector2(800, 800);


        private readonly Comp_NetworkNode NetworkNode;
        private readonly RimNet TargetNetwork;
        private readonly List<NetworkMessageCache> RecentMessages = new List<NetworkMessageCache>();

        private const float NodeBoxSize = 60f;
        private const float CanvasMargin = 40f;
        private const float MessageFadeTime = 2f;
        private Color NodeBackground = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        private Color ActiveConnection = new Color(0, 0.6f, 0, 0.5f);
        private Color NoActiveConnection = new Color(0.6f, 0, 0, 0.5f);


        private Color baseBG = default(Color);

        public Window_NetworkView(RimNet targetNetwork, Comp_NetworkNode networkNode)
        {
            TargetNetwork = targetNetwork;
            NetworkNode = networkNode;
            draggable = true;
            doCloseX = true;
            closeOnClickedOutside = false;
            preventCameraMotion = false;
            absorbInputAroundWindow = false;
            baseBG = this.NodeBackground;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (TargetNetwork?.NetworkNodes == null || TargetNetwork.NetworkNodes.Count == 0)
            {
                Widgets.Label(inRect, "No network or nodes found.");
                return;
            }

            Dictionary<Comp_NetworkNode, NodeVisual> nodeVisuals = CalculateNodePositions(inRect);

            DrawConnections(nodeVisuals);
            DrawNodes(nodeVisuals);
            DrawMessageFlow();
        }

        /// <summary>
        /// Calculates on-screen positions for each network node relative to map bounds.
        /// </summary>
        private Dictionary<Comp_NetworkNode, NodeVisual> CalculateNodePositions(Rect inRect)
        {
            Dictionary<Comp_NetworkNode, NodeVisual> nodeVisuals = new Dictionary<Comp_NetworkNode, NodeVisual>();
            Rect canvas = inRect.ContractedBy(CanvasMargin);

            Vector2 center = canvas.center;
            float angleStep = 137.5f * Mathf.Deg2Rad;

            for (int i = 0; i < TargetNetwork.NetworkNodes.Count; i++)
            {
                Comp_NetworkNode node = TargetNetwork.NetworkNodes[i];

                // Spiral position
                float angle = i * angleStep;
                float radius = 80f + 35f * Mathf.Sqrt(i); // tweakable
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

                Vector2 nodeCenter = center + offset;
                Rect nodeRect = new Rect(
                    nodeCenter.x - NodeBoxSize / 2,
                    nodeCenter.y - NodeBoxSize / 2,
                    NodeBoxSize,
                    NodeBoxSize
                );

                nodeVisuals[node] = new NodeVisual
                {
                    Center = nodeCenter,
                    Rect = nodeRect
                };
            }

            return nodeVisuals;
        
        
        
        }


        /// <summary>
        /// Draws lines between all nodes that are within connection range.
        /// </summary>
        private void DrawConnections(Dictionary<Comp_NetworkNode, NodeVisual> visuals)
        {
            foreach (Comp_NetworkNode a in TargetNetwork.NetworkNodes)
            {
                foreach (Comp_NetworkNode b in TargetNetwork.NetworkNodes)
                {
                    if (a == b) continue;

                    if (a.parent.Position.DistanceTo(b.parent.Position) <= a.Range)
                    {
                        Vector2 start = visuals[a].Center;
                        Vector2 end = visuals[b].Center;
                        Widgets.DrawLine(start, end, ActiveConnection, 1f);
                    }
                }
            }
        }

        /// <summary>
        /// Draws each node box, label, and tooltip.
        /// </summary>
        private void DrawNodes(Dictionary<Comp_NetworkNode, NodeVisual> visuals)
        {
            foreach ((Comp_NetworkNode node, NodeVisual visual) in visuals)
            {
                if (Mouse.IsOver(visual.Rect))
                {
                    Widgets.DrawHighlight(visual.Rect);
                }

                Color color = node.IsConnectedToAnyNetwork ? ActiveConnection : NoActiveConnection;
                Widgets.DrawBoxSolidWithOutline(visual.Rect, NodeBackground, color);

                Rect innerRect = visual.Rect.ContractedBy(2);

                Widgets.DrawBoxSolidWithOutline(innerRect, Color.clear, Color.white);


                if (Widgets.ButtonInvisible(visual.Rect))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (var cmdDef in DefDatabase<NetworkCommandDef>.AllDefs)
                    {
                        var cmd = cmdDef.CreateCommand();
                        var context = new NetworkCommandContext();

                        if (cmd.IsCommandValidFor(node, context))
                        {
                            options.Add(new FloatMenuOption(cmd.def.defName, () =>
                            {
                                NetworkNode.SendMessage(node.NodeID, cmdDef, context);
                            }));                          
                        }
                    }

                    if (options.Count > 0)
                    {
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                }


                Rect iconRect = visual.Rect.ContractedBy(4);
                Widgets.ThingIcon(iconRect, node.parent);
                TooltipHandler.TipRegion(visual.Rect,
                    $"Node: {node.parent.LabelShort}\nStatus: {(node.IsConnectedToAnyNetwork ? "Connected" : "Disconnected")}");
            }
        }

        /// <summary>
        /// Draws recent animated message lines between nodes.
        /// </summary>
        private void DrawMessageFlow()
        {
            float now = Time.realtimeSinceStartup;
            RecentMessages.RemoveAll(m => now - m.TimeStamp > MessageFadeTime);

            foreach (NetworkMessageCache networkMessageCache in RecentMessages)
            {
                float age = now - networkMessageCache.TimeStamp;
                float alpha = 1f - (age / MessageFadeTime);
                Color fadeColor = new Color(1f, 1f, 0f, alpha);
                Widgets.DrawLine(networkMessageCache.Start, networkMessageCache.End, fadeColor, 2f);
            }
        }

        /// <summary>
        /// Call this externally to create an animated message line between two positions.
        /// </summary>
        public void AddMessageFlow(Vector2 start, Vector2 end)
        {
            RecentMessages.Add(new NetworkMessageCache(start, end, Time.realtimeSinceStartup));
        }

        private struct NodeVisual
        {
            public Vector2 Center;
            public Rect Rect;
        }
    }
}
