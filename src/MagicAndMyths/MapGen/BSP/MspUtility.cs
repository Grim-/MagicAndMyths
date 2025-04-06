using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicAndMyths
{
    public static class MspUtility
    {
        // Create a minimum spanning tree of connections between nodes
        public static void CreateMinimumSpanningTree(List<BspNode> nodes)
        {
            if (nodes.Count <= 1)
                return;

            // First, preserve critical path connections and clear other connections
            foreach (var node in nodes)
            {
                if (node.HasTag("critical_path"))
                {
                    // Keep only connections to adjacent nodes on the critical path
                    var criticalConnections = node.connectedNodes
                        .Where(connected =>
                            connected.HasTag("critical_path") &&
                            IsCriticalPathAdjacent(node, connected))
                        .ToList();

                    node.connectedNodes = new List<BspNode>(criticalConnections);
                }
                else
                {
                    // Clear all connections for non-critical path nodes
                    node.connectedNodes = new List<BspNode>();
                }
            }

            // Create a list of nodes not on critical path
            List<BspNode> nonCriticalNodes = nodes
                .Where(n => !n.HasTag("critical_path"))
                .ToList();

            // Start with critical path nodes (if any)
            HashSet<BspNode> connectedNodes = new HashSet<BspNode>(
                nodes.Where(n => n.HasTag("critical_path"))
            );

            // If no critical path exists, start with one random node
            if (connectedNodes.Count == 0 && nodes.Count > 0)
            {
                connectedNodes.Add(nodes[0]);
                nonCriticalNodes.Remove(nodes[0]);
            }

            // Keep adding nodes until all are connected
            while (nonCriticalNodes.Count > 0)
            {
                BspNode bestNode1 = null;
                BspNode bestNode2 = null;
                float shortestDistance = float.MaxValue;

                // Find closest unconnected node to any connected node
                foreach (var connectedNode in connectedNodes)
                {
                    foreach (var unconnectedNode in nonCriticalNodes)
                    {
                        float distance = Vector3.Distance(
                            connectedNode.roomRect.CenterCell.ToVector3(),
                            unconnectedNode.roomRect.CenterCell.ToVector3());

                        if (distance < shortestDistance)
                        {
                            shortestDistance = distance;
                            bestNode1 = connectedNode;
                            bestNode2 = unconnectedNode;
                        }
                    }
                }

                if (bestNode1 != null && bestNode2 != null)
                {
                    bestNode1.connectedNodes.Add(bestNode2);
                    bestNode2.connectedNodes.Add(bestNode1);
                    connectedNodes.Add(bestNode2);
                    nonCriticalNodes.Remove(bestNode2);
                }
                else
                {
                    break;
                }
            }
        }

        // Helper method to check if two nodes are adjacent on the critical path
        private static bool IsCriticalPathAdjacent(BspNode node1, BspNode node2)
        {
            // This would check path index stored in tags
            // For example: "path_index_0", "path_index_1" etc.

            int GetPathIndex(BspNode node)
            {
                foreach (var tag in node.tags)
                {
                    if (tag.StartsWith("path_index_"))
                    {
                        if (int.TryParse(tag.Substring(11), out int index))
                        {
                            return index;
                        }
                    }
                }
                return -1;
            }

            int index1 = GetPathIndex(node1);
            int index2 = GetPathIndex(node2);

            return index1 >= 0 && index2 >= 0 && Math.Abs(index1 - index2) == 1;
        }
    }
}
