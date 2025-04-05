using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    // Manager for room connection rules
    public static class RoomConnectionRuleManager
    {
        private static Dictionary<RoomType, RoomConnectionRule> rules = new Dictionary<RoomType, RoomConnectionRule>();

        // Register a rule for a specific room type
        public static void RegisterRule(RoomConnectionRule rule)
        {
            rules[rule.roomType] = rule;
            Log.Message($"Registered connection rule for room type: {rule.roomType}");
        }

        // Get the rule for a room type (or default if none exists)
        public static RoomConnectionRule GetRule(RoomType roomType)
        {
            if (rules.TryGetValue(roomType, out var rule))
                return rule;

            return new RoomConnectionRule { roomType = roomType };
        }

        // Check if a connection is valid according to room rules
        public static bool IsConnectionValid(
            RoomConnection connection,
            Dictionary<BspUtility.BspNode, int> connectionCounts)
        {

            if (connection.roomA.IsOnCriticalPath && connection.roomB.IsOnCriticalPath && Math.Abs(connection.roomA.CriticalPathIndex - connection.roomB.CriticalPathIndex) == 1)
            {
                return true;
            }

            var roomAType = GetRoomType(connection.roomA);
            var roomBType = GetRoomType(connection.roomB);

            var ruleA = GetRule(roomAType);
            var ruleB = GetRule(roomBType);

            // Check connection count limits
            if (connectionCounts.TryGetValue(connection.roomA, out int connectionsA))
            {
                if (connectionsA >= ruleA.maxConnections)
                {
                    Log.Message($"Connection rejected: {roomAType} room already has {connectionsA} connections (max {ruleA.maxConnections})");
                    return false;
                }
            }

            if (connectionCounts.TryGetValue(connection.roomB, out int connectionsB))
            {
                if (connectionsB >= ruleB.maxConnections)
                {
                    Log.Message($"Connection rejected: {roomBType} room already has {connectionsB} connections (max {ruleB.maxConnections})");
                    return false;
                }
            }

            // Check avoided connections
            if (ruleA.avoidedConnectionTypes != null &&
                ruleA.avoidedConnectionTypes.Contains(roomBType))
            {
                Log.Message($"Connection rejected: {roomAType} room avoids connecting to {roomBType}");
                return false;
            }

            if (ruleB.avoidedConnectionTypes != null &&
                ruleB.avoidedConnectionTypes.Contains(roomAType))
            {
                Log.Message($"Connection rejected: {roomBType} room avoids connecting to {roomAType}");
                return false;
            }

            // Check for critical path requirements
            // Start and End rooms on critical path should only connect along the path
            if ((roomAType == RoomType.Start || roomAType == RoomType.End) &&
                connection.roomA.IsOnCriticalPath && connection.roomB.IsOnCriticalPath &&
                Math.Abs(connection.roomA.CriticalPathIndex - connection.roomB.CriticalPathIndex) != 1)
            {
                Log.Message($"Connection rejected: {roomAType} room should only connect along critical path");
                return false;
            }

            // Run custom validators if any
            if (ruleA.customValidator != null &&
                !ruleA.customValidator(connection.roomA, connection))
            {
                Log.Message($"Connection rejected by {roomAType} custom validator");
                return false;
            }

            if (ruleB.customValidator != null &&
                !ruleB.customValidator(connection.roomB, connection))
            {
                Log.Message($"Connection rejected by {roomBType} custom validator");
                return false;
            }

            return true;
        }

        // Apply rules to filter the proposed connections
        public static List<RoomConnection> ApplyRules(List<BspUtility.BspNode> nodes,List<RoomConnection> proposedConnections)
        {

            var connectionCounts = new Dictionary<BspUtility.BspNode, int>();

            foreach (var node in nodes)
            {
                connectionCounts[node] = node.connectedNodes.Count;
            }

            // Sort by priority (critical path, special rooms, etc)
            var sortedConnections = SortConnectionsByPriority(proposedConnections);
            var validConnections = new List<RoomConnection>();

            // First pass: apply rules and accept valid connections
            foreach (var connection in sortedConnections)
            {
                if (IsConnectionValid(connection, connectionCounts))
                {
                    validConnections.Add(connection);

                    connectionCounts[connection.roomA] = connectionCounts[connection.roomA] + 1;
                    connectionCounts[connection.roomB] = connectionCounts[connection.roomB] + 1;
                }
            }

            EnsureMinimumConnections(nodes, validConnections, connectionCounts);

            return validConnections;
        }

        private static void EnsureMinimumConnections(
            List<BspUtility.BspNode> nodes,
            List<RoomConnection> validConnections,
            Dictionary<BspUtility.BspNode, int> connectionCounts)
        {
            foreach (var node in nodes)
            {
                var roomType = GetRoomType(node);
                var rule = GetRule(roomType);

                int currentConnections = connectionCounts[node];
                if (currentConnections >= rule.minConnections)
                    continue;

                // Find additional connections if needed
                var potentialTargets = nodes
                    .Where(n => n != node)
                    .OrderBy(n => {
                        // Prefer connecting to preferred types
                        var targetType = GetRoomType(n);
                        if (rule.preferredConnectionTypes != null &&
                            rule.preferredConnectionTypes.Contains(targetType))
                            return 0;
                        return 1;
                    })
                    .ThenBy(n => Vector3.Distance(
                        node.room.CenterCell.ToVector3(),
                        n.room.CenterCell.ToVector3()))
                    .ToList();

                foreach (var target in potentialTargets)
                {
                    if (currentConnections >= rule.minConnections)
                        break;

                    // Check if target can accept more connections
                    var targetType = GetRoomType(target);
                    var targetRule = GetRule(targetType);

                    if (connectionCounts[target] >= targetRule.maxConnections)
                        continue;

                    // Check if this connection already exists
                    if (validConnections.Any(c =>
                        (c.roomA == node && c.roomB == target) ||
                        (c.roomA == target && c.roomB == node)))
                        continue;

                    // Create a new connection
                    var newConnection = new RoomConnection(node, target);
                    newConnection.corridors = CorridoorUtility.GenerateCorridors(node, target);

                    // Only add if it doesn't violate target's avoided types
                    if ((targetRule.avoidedConnectionTypes == null || !targetRule.avoidedConnectionTypes.Contains(roomType)) && (rule.avoidedConnectionTypes == null || !rule.avoidedConnectionTypes.Contains(targetType)))
                    {
                        validConnections.Add(newConnection);
                        connectionCounts[node]++;
                        connectionCounts[target]++;
                        currentConnections++;

                        Log.Message($"Added minimum connection from {roomType} to {targetType}");
                    }
                }

                if (currentConnections < rule.minConnections)
                {
                    Log.Warning($"Could not satisfy minimum connections for {roomType} room: " +
                               $"has {currentConnections}, needs {rule.minConnections}");
                }
            }
        }

        // Sort connections by rule priority
        private static List<RoomConnection> SortConnectionsByPriority(List<RoomConnection> connections)
        {
            return connections
                .OrderByDescending(c => {
                    // Critical path connections get highest priority
                    if ((c.roomA.IsOnCriticalPath && c.roomB.IsOnCriticalPath) &&
                        Math.Abs(c.roomA.CriticalPathIndex - c.roomB.CriticalPathIndex) == 1)
                        return 1000;

                    // Otherwise use the rule priority
                    var typeA = GetRoomType(c.roomA);
                    var typeB = GetRoomType(c.roomB);

                    var ruleA = GetRule(typeA);
                    var ruleB = GetRule(typeB);

                    return Math.Max(ruleA.priority, ruleB.priority);
                })
                .ToList();
        }

        private static RoomType GetRoomType(BspUtility.BspNode node)
        {
            return node.def.roomType;
        }
    }
}
