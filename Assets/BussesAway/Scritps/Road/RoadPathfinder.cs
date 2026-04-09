using System.Collections.Generic;
using UnityEngine;

namespace BussesAway
{
    public static class RoadPathfinder
    {
        private class Node
        {
            public Road Road;
            public RoadDirection EntryDir;
            public Node Parent;
            public bool ArrivedFromTunnel;
        }

        private readonly struct NodeKey
        {
            public readonly Road Road;
            public readonly RoadDirection EntryDir;

            public NodeKey(Road road, RoadDirection entryDir)
            {
                Road = road;
                EntryDir = entryDir;
            }
        }

        public static bool TryFindPath(
            Road start,
            RoadDirection startEntryDir,
            Road destination,
            out List<Vector3> waypoints)
        {
            return TryFindPath(start, startEntryDir, destination, out waypoints, out _, out _);
        }

        public static bool TryFindPath(
            Road start,
            RoadDirection startEntryDir,
            Road destination,
            out List<Vector3> waypoints,
            out RoadDirection destinationEntryDir)
        {
            return TryFindPath(start, startEntryDir, destination, out waypoints, out destinationEntryDir, out _);
        }

        public static bool TryFindPath(
            Road start,
            RoadDirection startEntryDir,
            Road destination,
            out List<Vector3> waypoints,
            out RoadDirection destinationEntryDir,
            out List<int> teleportWaypointIndices)
        {
            return TryFindPathInternal(
                start,
                startEntryDir,
                destination,
                out waypoints,
                out destinationEntryDir,
                out teleportWaypointIndices,
                appendDestinationWaypoints: true,
                logWarningOnFailure: true);
        }

        public static bool TryFindPathToRoadEntry(
            Road start,
            RoadDirection startEntryDir,
            Road destination,
            out List<Vector3> waypoints,
            out RoadDirection destinationEntryDir,
            out List<int> teleportWaypointIndices)
        {
            return TryFindPathInternal(
                start,
                startEntryDir,
                destination,
                out waypoints,
                out destinationEntryDir,
                out teleportWaypointIndices,
                appendDestinationWaypoints: false,
                logWarningOnFailure: true);
        }

        private static bool TryFindPathInternal(
            Road start,
            RoadDirection startEntryDir,
            Road destination,
            out List<Vector3> waypoints,
            out RoadDirection destinationEntryDir,
            out List<int> teleportWaypointIndices,
            bool appendDestinationWaypoints,
            bool logWarningOnFailure)
        {
            waypoints = null;
            destinationEntryDir = startEntryDir;
            teleportWaypointIndices = new List<int>();

            if (start == null || destination == null)
            {
                Debug.LogWarning("[RoadPathfinder] start or destination is null.");
                return false;
            }

            if (start == destination)
            {
                destinationEntryDir = startEntryDir;
                return TryFindCircularPath(start, startEntryDir, out waypoints);
            }

            var queue = new Queue<Node>();
            var visited = new HashSet<NodeKey>();

            queue.Enqueue(new Node
            {
                Road = start,
                EntryDir = startEntryDir,
                Parent = null,
            });
            visited.Add(new NodeKey(start, startEntryDir));

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var conn in current.Road.Connections)
                {
                    if (conn.neighbor == null) continue;
                    if (conn.direction == current.EntryDir) continue;

                    var nextEntryDir = Opposite(conn.direction);
                    var nextKey = new NodeKey(conn.neighbor, nextEntryDir);
                    if (visited.Contains(nextKey)) continue;

                    var nextNode = new Node
                    {
                        Road = conn.neighbor,
                        EntryDir = nextEntryDir,
                        Parent = current,
                    };

                    visited.Add(nextKey);

                    if (conn.neighbor == destination)
                    {
                        destinationEntryDir = nextEntryDir;
                        waypoints = BuildWaypoints(nextNode, teleportWaypointIndices, appendDestinationWaypoints);
                        return true;
                    }

                    queue.Enqueue(nextNode);
                }

                if (current.Road.TryGetTunnelTeleportTarget(out var tunnelExitRoad, out var tunnelExitEntryDir))
                {
                    var tunnelKey = new NodeKey(tunnelExitRoad, tunnelExitEntryDir);
                    if (!visited.Contains(tunnelKey))
                    {
                        var tunnelNode = new Node
                        {
                            Road = tunnelExitRoad,
                            EntryDir = tunnelExitEntryDir,
                            Parent = current,
                            ArrivedFromTunnel = true,
                        };

                        visited.Add(tunnelKey);

                        if (tunnelExitRoad == destination)
                        {
                            destinationEntryDir = tunnelExitEntryDir;
                            waypoints = BuildWaypoints(tunnelNode, teleportWaypointIndices, appendDestinationWaypoints);
                            return true;
                        }

                        queue.Enqueue(tunnelNode);
                    }
                }
            }

            if (logWarningOnFailure)
                Debug.LogWarning($"[RoadPathfinder] Cannot find path from '{start.name}' to '{destination.name}'.");
            return false;
        }

        public static bool CanReach(
            Road start,
            RoadDirection startEntryDir,
            Road destination)
        {
            return TryFindPathInternal(
                start,
                startEntryDir,
                destination,
                out _,
                out _,
                out _,
                appendDestinationWaypoints: true,
                logWarningOnFailure: false);
        }

        private static bool TryFindCircularPath(
            Road start,
            RoadDirection startEntryDir,
            out List<Vector3> waypoints)
        {
            waypoints = null;

            var startWaypoints = start.GetWorldWaypointsFrom(startEntryDir);

            Node firstNode = null;
            foreach (var conn in start.Connections)
            {
                if (conn.direction == startEntryDir) continue;
                if (conn.neighbor == null) continue;

                firstNode = new Node
                {
                    Road = conn.neighbor,
                    EntryDir = Opposite(conn.direction),
                    Parent = null,
                };
                break;
            }

            if (firstNode == null)
            {
                waypoints = startWaypoints;
                Debug.LogWarning($"[RoadPathfinder] '{start.name}' has no loop exit.");
                return true;
            }

            var queue = new Queue<Node>();
            var visited = new HashSet<NodeKey>();

            queue.Enqueue(firstNode);
            visited.Add(new NodeKey(firstNode.Road, firstNode.EntryDir));

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var conn in current.Road.Connections)
                {
                    if (conn.neighbor == null) continue;
                    if (conn.direction == current.EntryDir) continue;

                    var nextEntryDir = Opposite(conn.direction);
                    var nextNode = new Node
                    {
                        Road = conn.neighbor,
                        EntryDir = nextEntryDir,
                        Parent = current,
                    };

                    if (conn.neighbor == start)
                    {
                        var loopPath = BuildWaypoints(current, new List<int>(), appendDestinationWaypoints: true);

                        waypoints = new List<Vector3>(startWaypoints);
                        for (var i = 1; i < loopPath.Count; i++)
                            waypoints.Add(loopPath[i]);

                        var finalPass = start.GetWorldWaypointsFrom(startEntryDir);
                        for (var i = 1; i < finalPass.Count; i++)
                            waypoints.Add(finalPass[i]);

                        return true;
                    }

                    var nextKey = new NodeKey(conn.neighbor, nextEntryDir);
                    if (visited.Contains(nextKey)) continue;

                    visited.Add(nextKey);
                    queue.Enqueue(nextNode);
                }

                if (current.Road.TryGetTunnelTeleportTarget(out var tunnelExitRoad, out var tunnelExitEntryDir))
                {
                    var tunnelKey = new NodeKey(tunnelExitRoad, tunnelExitEntryDir);
                    if (!visited.Contains(tunnelKey))
                    {
                        visited.Add(tunnelKey);
                        queue.Enqueue(new Node
                        {
                            Road = tunnelExitRoad,
                            EntryDir = tunnelExitEntryDir,
                            Parent = current,
                            ArrivedFromTunnel = true,
                        });
                    }
                }
            }

            Debug.LogWarning($"[RoadPathfinder] '{start.name}' cannot form a loop.");
            waypoints = startWaypoints;
            return true;
        }

        private static List<Vector3> BuildWaypoints(Node endNode, List<int> teleportWaypointIndices, bool appendDestinationWaypoints)
        {
            var nodes = new List<Node>();
            for (var node = endNode; node != null; node = node.Parent)
                nodes.Add(node);
            nodes.Reverse();

            var path = new List<Vector3>();
            for (var i = 0; i < nodes.Count; i++)
            {
                var isDestination = i == nodes.Count - 1;
                if (isDestination)
                {
                    if (!appendDestinationWaypoints)
                        continue;

                    if (nodes[i].ArrivedFromTunnel)
                    {
                        AppendTunnelExitWaypoints(path, nodes[i].Road, teleportWaypointIndices);
                        continue;
                    }

                    AppendDestinationWaypoints(path, nodes[i].Road);
                    continue;
                }

                if (nodes[i].ArrivedFromTunnel)
                {
                    AppendTunnelExitWaypoints(path, nodes[i].Road, teleportWaypointIndices, skipDuplicateStart: i != 0);
                    continue;
                }

                var roadWaypoints = nodes[i].Road.GetWorldWaypointsFrom(nodes[i].EntryDir);
                var startIndex = i == 0 ? 0 : 1;
                for (var j = startIndex; j < roadWaypoints.Count; j++)
                    path.Add(roadWaypoints[j]);
            }

            return path;
        }

        private static void AppendDestinationWaypoints(List<Vector3> path, Road destinationRoad)
        {
            var destinationWaypoints = destinationRoad.GetWorldWaypoints();
            if (destinationWaypoints.Count == 0)
                return;

            if (path.Count == 0)
            {
                path.AddRange(destinationWaypoints);
                return;
            }

            var previousPoint = path[^1];
            var closestIndex = 0;
            var closestDistance = Vector3.Distance(previousPoint, destinationWaypoints[0]);

            for (var i = 1; i < destinationWaypoints.Count; i++)
            {
                var distance = Vector3.Distance(previousPoint, destinationWaypoints[i]);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }

            for (var i = closestIndex; i < destinationWaypoints.Count; i++)
            {
                if (i == closestIndex && Vector3.Distance(path[^1], destinationWaypoints[i]) < 0.001f)
                    continue;

                path.Add(destinationWaypoints[i]);
            }
        }

        private static void AppendTunnelExitWaypoints(
            List<Vector3> path,
            Road tunnelExitRoad,
            List<int> teleportWaypointIndices,
            bool skipDuplicateStart = false)
        {
            var tunnelWaypoints = tunnelExitRoad.GetWorldWaypointsFromTunnelInterior();
            if (tunnelWaypoints.Count == 0)
                return;

            var firstAddedIndex = path.Count;
            if (path.Count == 0)
            {
                path.AddRange(tunnelWaypoints);
                teleportWaypointIndices.Add(firstAddedIndex);
                return;
            }

            var startIndex = skipDuplicateStart ? 1 : 0;
            for (var i = startIndex; i < tunnelWaypoints.Count; i++)
            {
                if (i == 0 && Vector3.Distance(path[^1], tunnelWaypoints[i]) < 0.001f)
                    continue;

                path.Add(tunnelWaypoints[i]);
            }

            if (firstAddedIndex < path.Count)
                teleportWaypointIndices.Add(firstAddedIndex);
        }

        private static RoadDirection Opposite(RoadDirection dir)
        {
            return (RoadDirection)(((int)dir + 2) % 4);
        }
    }
}
