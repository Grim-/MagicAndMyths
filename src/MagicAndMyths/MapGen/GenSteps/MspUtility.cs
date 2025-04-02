using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicAndMyths
{
    public static class MspUtility
    {
        public static void CreateMinimumSpanningTree(List<BspUtility.BspNode> nodes)
        {
            if (nodes.Count <= 1)
                return;

            foreach (var node in nodes)
            {
                if (node.IsOnCriticalPath)
                {
                    var criticalConnections = node.connectedNodes
                        .Where(connected =>
                            connected.IsOnCriticalPath &&
                            Math.Abs(node.CriticalPathIndex - connected.CriticalPathIndex) == 1)
                        .ToList();

                    node.connectedNodes = new List<BspUtility.BspNode>(criticalConnections);
                }
                else
                {
                    node.connectedNodes = new List<BspUtility.BspNode>();
                }
            }

            // Create a list of nodes not on critical path
            List<BspUtility.BspNode> nonCriticalNodes = nodes
                .Where(n => !n.IsOnCriticalPath)
                .ToList();

            // Add non-critical nodes to the MST one by one
            HashSet<BspUtility.BspNode> connectedNodes = new HashSet<BspUtility.BspNode>(
                nodes.Where(n => n.IsOnCriticalPath)
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
                BspUtility.BspNode bestNode1 = null;
                BspUtility.BspNode bestNode2 = null;
                float shortestDistance = float.MaxValue;

                // Find closest unconnected node to any connected node
                foreach (var connectedNode in connectedNodes)
                {
                    foreach (var unconnectedNode in nonCriticalNodes)
                    {
                        float distance = Vector3.Distance(
                            connectedNode.room.CenterCell.ToVector3(),
                            unconnectedNode.room.CenterCell.ToVector3());

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
    }
}
