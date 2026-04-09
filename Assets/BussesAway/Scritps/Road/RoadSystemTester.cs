using System.Collections.Generic;
using DucMinh;
using UnityEngine;

namespace BussesAway
{
    /// <summary>
    /// Manual test harness for road pathfinding and bus movement.
    /// </summary>
    public class RoadSystemTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Bus bus;
        [SerializeField] private Road startRoad;
        [SerializeField] private RoadDirection startEntryDirection = RoadDirection.South;
        [SerializeField] private Road destinationRoad;

        [Header("Depot Test")]
        [Tooltip("Depot used to spawn a test bus and send the bus back to the return lane.")]
        [SerializeField] private BusDepot busDepot;
        [SerializeField] private int testBusPassengerCapacity = 8;

        [Header("Settings")]
        [Tooltip("Test move speed. 0 means use the Bus inspector value.")]
        [SerializeField] private float testMoveSpeed = 0f;
        [Tooltip("If true, continue looping around the destination road after reaching it.")]
        [SerializeField] private bool loopContinuouslyAtDestination = true;

        [Header("Debug (read-only)")]
        [SerializeField] private int foundPathCount;
        [SerializeField] private string lastResult = "-";
        [SerializeField] private bool isLoopingAtDestination;

        private List<Vector3> _debugPath;
        private bool _watchBusCompletion;
        private bool _busWasRunning;
        private RoadDirection _lastDestinationEntryDirection;

        public bool IsLoopingAtDestination => isLoopingAtDestination;

        private void Update()
        {
            if (bus == null) return;

            var isRunning = bus.CurrentStateID == BusStateID.Run;
            if (_watchBusCompletion && _busWasRunning && !isRunning)
                HandleBusFinishedPath();

            _busWasRunning = isRunning;
        }

#if DEBUG_MODE
        [Button]
#endif
        [ContextMenu("1. Auto-Link All Roads in Scene")]
        public void AutoLinkAllRoads()
        {
            var allRoads = FindObjectsByType<Road>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (allRoads.Length == 0)
            {
                lastResult = "ERROR: No roads found in scene";
                Debug.LogWarning("[RoadSystemTester] " + lastResult);
                return;
            }

            foreach (var road in allRoads)
                road.AutoDetectConnections();

            lastResult = $"OK: Auto-linked {allRoads.Length} roads";
            Debug.Log($"[RoadSystemTester] {lastResult}");
        }

#if DEBUG_MODE
        [Button]
#endif
        [ContextMenu("2. Set Bus Start Road")]
        public void SetBusStartRoad()
        {
            if (!ValidateBus()) return;
            if (startRoad == null)
            {
                lastResult = "ERROR: startRoad = null";
                Debug.LogWarning("[RoadSystemTester] " + lastResult);
                return;
            }

            ResetLoopState();
            bus.SetCurrentRoad(startRoad, startEntryDirection);

            var waypoints = startRoad.GetWorldWaypointsFrom(startEntryDirection);
            if (waypoints.Count > 0)
                bus.transform.position = waypoints[0];

            lastResult = $"OK: Bus placed on '{startRoad.name}' with entry {startEntryDirection}";
            Debug.Log($"[RoadSystemTester] {lastResult}");
        }

#if DEBUG_MODE
        [Button]
#endif
        [ContextMenu("3. Test: DriveTo Destination")]
        public void TestDriveTo()
        {
            if (!ValidateBus()) return;
            if (destinationRoad == null)
            {
                lastResult = "ERROR: destinationRoad = null";
                Debug.LogWarning("[RoadSystemTester] " + lastResult);
                return;
            }

            if (TryStartDriveFromWaitingDepotBus(destinationRoad))
                return;

            StartDrive(
                startRoad,
                startEntryDirection,
                destinationRoad,
                armLoopAfterArrival: loopContinuouslyAtDestination && startRoad != destinationRoad);
        }

#if DEBUG_MODE
        [Button]
#endif
        [ContextMenu("4. Preview Path Only")]
        public void PreviewPath()
        {
            _debugPath = null;
            foundPathCount = 0;

            if (startRoad == null || destinationRoad == null)
            {
                lastResult = "ERROR: startRoad and destinationRoad are required";
                Debug.LogWarning("[RoadSystemTester] " + lastResult);
                return;
            }

            var found = RoadPathfinder.TryFindPath(
                startRoad,
                startEntryDirection,
                destinationRoad,
                out var path,
                out var destinationEntryDirection);

            if (!found)
            {
                lastResult = $"ERROR: No path from '{startRoad.name}' to '{destinationRoad.name}'";
                Debug.LogWarning("[RoadSystemTester] " + lastResult);
                return;
            }

            _debugPath = path;
            foundPathCount = path.Count;
            _lastDestinationEntryDirection = destinationEntryDirection;
            lastResult = $"OK: Preview path found with {path.Count} waypoints";
            Debug.Log($"[RoadSystemTester] {lastResult}");
        }

#if DEBUG_MODE
        [Button]
#endif
        [ContextMenu("5. Stop Bus")]
        public void StopBus()
        {
            if (!ValidateBus()) return;

            ResetLoopState();
            bus.StopDriving();
            lastResult = "OK: Bus stopped";
            Debug.Log($"[RoadSystemTester] {lastResult}");
        }

#if DEBUG_MODE
        [Button]
#endif
        [ContextMenu("6. Reset Bus to Start")]
        public void ResetBusToStart()
        {
            if (!ValidateBus()) return;

            ResetLoopState();
            bus.StopDriving();
            SetBusStartRoad();
            _debugPath = null;
            foundPathCount = 0;
            lastResult = "OK: Bus reset to start road";
            Debug.Log($"[RoadSystemTester] {lastResult}");
        }

#if DEBUG_MODE
        [Button]
#endif
        [ContextMenu("7. Full Test")]
        public void FullTest()
        {
            AutoLinkAllRoads();
            SetBusStartRoad();
            TestDriveTo();
        }

#if DEBUG_MODE
        [Button]
#endif
        [ContextMenu("8. Spawn Test Bus From Depot")]
        public void SpawnTestBusFromDepot()
        {
            if (busDepot == null)
            {
                lastResult = "ERROR: busDepot = null";
                Debug.LogWarning("[RoadSystemTester] busDepot is not assigned.");
                return;
            }

            ResetLoopState();
            var spawnedBus = busDepot.SpawnBus(testBusPassengerCapacity, startDeparture: false);
            if (spawnedBus == null)
            {
                lastResult = "ERROR: Failed to spawn test bus from depot";
                Debug.LogWarning("[RoadSystemTester] " + lastResult);
                return;
            }

            bus = spawnedBus;
            if (bus.CurrentRoad != null)
            {
                startRoad = bus.CurrentRoad;
                startEntryDirection = bus.CurrentEntryDirection;
            }

            _debugPath = null;
            foundPathCount = 0;
            _watchBusCompletion = false;
            _busWasRunning = bus.CurrentStateID == BusStateID.Run;

            lastResult = "OK: Spawned test bus from depot and kept it idle at spawn";
            Debug.Log($"[RoadSystemTester] {lastResult}");
        }

#if DEBUG_MODE
        [Button]
#endif
        [ContextMenu("9. Send Bus To Depot Return")]
        public void SendBusToDepotReturn()
        {
            if (!ValidateBus()) return;
            if (busDepot == null)
            {
                lastResult = "ERROR: busDepot = null";
                Debug.LogWarning("[RoadSystemTester] busDepot is not assigned.");
                return;
            }

            var fromRoad = ResolveReturnStartRoad();
            var fromEntryDirection = ResolveReturnStartEntryDirection(fromRoad);
            if (fromRoad == null)
            {
                lastResult = "ERROR: Could not resolve a start road for the depot return test";
                Debug.LogWarning("[RoadSystemTester] " + lastResult);
                return;
            }

            ResetLoopState();
            if (!busDepot.SendBusHome(bus, fromRoad, fromEntryDirection))
            {
                lastResult = $"ERROR: Failed to send bus to depot from '{fromRoad.name}'";
                Debug.LogWarning($"[RoadSystemTester] {lastResult}");
                return;
            }

            _debugPath = bus.CurrentPath != null ? new List<Vector3>(bus.CurrentPath) : null;
            foundPathCount = _debugPath?.Count ?? 0;
            _busWasRunning = bus.CurrentStateID == BusStateID.Run;

            lastResult = $"OK: Bus is driving to depot return from '{fromRoad.name}' ({foundPathCount} waypoints)";
            Debug.Log($"[RoadSystemTester] {lastResult}");
        }

        private bool TryStartDriveFromWaitingDepotBus(Road toRoad)
        {
            if (busDepot == null || bus == null)
                return false;

            if (busDepot.LastSpawnedBus != bus)
                return false;

            if (!busDepot.IsBusWaitingAtSpawn(bus))
                return false;

            if (!busDepot.TryBuildPathToDestination(
                    toRoad,
                    out var path,
                    out var destinationEntryDirection,
                    out var teleportWaypointIndices))
            {
                lastResult = $"ERROR: Cannot build depot departure path to '{toRoad.name}'";
                Debug.LogWarning($"[RoadSystemTester] {lastResult}");
                return true;
            }

            _debugPath = path;
            foundPathCount = path.Count;
            _lastDestinationEntryDirection = destinationEntryDirection;

            bus.SetPath(path, teleportWaypointIndices);
            bus.StartDriving();

            _watchBusCompletion = loopContinuouslyAtDestination && bus.CurrentRoad != toRoad;
            _busWasRunning = bus.CurrentStateID == BusStateID.Run;
            isLoopingAtDestination = false;

            lastResult = $"OK: Depot bus started driving to '{toRoad.name}' ({path.Count} waypoints)";
            Debug.Log($"[RoadSystemTester] {lastResult}");
            return true;
        }

        private bool ValidateBus()
        {
            if (bus != null) return true;

            lastResult = "ERROR: bus = null";
            Debug.LogWarning("[RoadSystemTester] bus is not assigned.");
            return false;
        }

        private Road ResolveReturnStartRoad()
        {
            if (bus != null && bus.CurrentRoad != null)
                return bus.CurrentRoad;

            if (destinationRoad != null)
                return destinationRoad;

            return startRoad;
        }

        private RoadDirection ResolveReturnStartEntryDirection(Road fromRoad)
        {
            if (fromRoad == null)
                return startEntryDirection;

            if (bus != null && fromRoad == bus.CurrentRoad)
                return bus.CurrentEntryDirection;

            if (fromRoad == destinationRoad)
                return _lastDestinationEntryDirection;

            return startEntryDirection;
        }

        private void ResetLoopState()
        {
            _watchBusCompletion = false;
            _busWasRunning = false;
            isLoopingAtDestination = false;
        }

        private void HandleBusFinishedPath()
        {
            if (!loopContinuouslyAtDestination || destinationRoad == null)
            {
                ResetLoopState();
                return;
            }

            if (!TryStartDestinationLoop())
                ResetLoopState();
        }

        private bool StartDrive(
            Road fromRoad,
            RoadDirection fromEntryDirection,
            Road toRoad,
            bool armLoopAfterArrival)
        {
            _debugPath = null;
            foundPathCount = 0;

            if (fromRoad == null || toRoad == null)
            {
                lastResult = "ERROR: start road and destination road are required";
                Debug.LogWarning("[RoadSystemTester] " + lastResult);
                return false;
            }

            if (!RoadPathfinder.TryFindPath(
                    fromRoad,
                    fromEntryDirection,
                    toRoad,
                    out var path,
                    out var destinationEntryDirection,
                    out var teleportWaypointIndices))
            {
                ResetLoopState();
                lastResult = $"ERROR: Cannot find path from '{fromRoad.name}' to '{toRoad.name}'";
                Debug.LogWarning("[RoadSystemTester] " + lastResult);
                return false;
            }

            _debugPath = path;
            foundPathCount = path.Count;
            _lastDestinationEntryDirection = destinationEntryDirection;

            bus.SetCurrentRoad(fromRoad, fromEntryDirection);
            bus.SetPath(path, teleportWaypointIndices);
            bus.StartDriving();

            _watchBusCompletion = armLoopAfterArrival;
            _busWasRunning = bus.CurrentStateID == BusStateID.Run;
            isLoopingAtDestination = false;

            lastResult = armLoopAfterArrival
                ? $"OK: DriveTo '{toRoad.name}' with {path.Count} waypoints, loop armed"
                : $"OK: DriveTo '{toRoad.name}' with {path.Count} waypoints";
            Debug.Log($"[RoadSystemTester] {lastResult}");
            return true;
        }

        private bool TryStartDestinationLoop()
        {
            if (!TryGetDestinationLoopStart(out var loopStartRoad, out var loopStartEntryDirection))
            {
                lastResult = $"ERROR: '{destinationRoad?.name}' has no valid loop exit";
                Debug.LogWarning($"[RoadSystemTester] {lastResult}");
                return false;
            }

            if (!RoadPathfinder.TryFindPath(
                    loopStartRoad,
                    loopStartEntryDirection,
                    destinationRoad,
                    out var path,
                    out var destinationEntryDirection,
                    out var teleportWaypointIndices))
            {
                lastResult = $"ERROR: Cannot find loop path around '{destinationRoad.name}'";
                Debug.LogWarning($"[RoadSystemTester] {lastResult}");
                return false;
            }

            _debugPath = path;
            foundPathCount = path.Count;
            _lastDestinationEntryDirection = destinationEntryDirection;

            bus.SetCurrentRoad(loopStartRoad, loopStartEntryDirection);
            bus.SetPath(path, teleportWaypointIndices);
            bus.StartDriving();

            _watchBusCompletion = true;
            _busWasRunning = bus.CurrentStateID == BusStateID.Run;
            isLoopingAtDestination = true;

            lastResult = $"OK: Missed stop on '{destinationRoad.name}', looping with {path.Count} waypoints";
            Debug.Log($"[RoadSystemTester] {lastResult}");
            return true;
        }

        private bool TryGetDestinationLoopStart(out Road loopStartRoad, out RoadDirection loopStartEntryDirection)
        {
            loopStartRoad = null;
            loopStartEntryDirection = _lastDestinationEntryDirection;

            if (destinationRoad == null)
                return false;

            var destinationWaypoints = destinationRoad.GetWorldWaypoints();
            if (destinationWaypoints.Count == 0)
                return false;

            var endPoint = destinationWaypoints[^1];
            var bestDistance = float.MaxValue;

            if (TryFindLoopStartCandidate(endPoint, excludeEntryDirection: true, ref bestDistance, out loopStartRoad, out loopStartEntryDirection))
                return true;

            bestDistance = float.MaxValue;
            return TryFindLoopStartCandidate(endPoint, excludeEntryDirection: false, ref bestDistance, out loopStartRoad, out loopStartEntryDirection);
        }

        private bool TryFindLoopStartCandidate(
            Vector3 destinationEndPoint,
            bool excludeEntryDirection,
            ref float bestDistance,
            out Road loopStartRoad,
            out RoadDirection loopStartEntryDirection)
        {
            loopStartRoad = null;
            loopStartEntryDirection = _lastDestinationEntryDirection;

            foreach (var conn in destinationRoad.Connections)
            {
                if (conn.neighbor == null) continue;
                if (excludeEntryDirection && conn.direction == _lastDestinationEntryDirection) continue;

                var distance = Vector3.Distance(destinationEndPoint, GetRoadBorderWorld(destinationRoad, conn.direction));
                if (distance >= bestDistance) continue;

                var candidateEntryDirection = Opposite(conn.direction);
                if (!RoadPathfinder.CanReach(conn.neighbor, candidateEntryDirection, destinationRoad))
                    continue;

                bestDistance = distance;
                loopStartRoad = conn.neighbor;
                loopStartEntryDirection = candidateEntryDirection;
            }

            return loopStartRoad != null;
        }

        private static Vector3 GetRoadBorderWorld(Road road, RoadDirection direction)
        {
            var half = road.TileSize * 0.5f;
            var offset = direction switch
            {
                RoadDirection.North => new Vector3(0f, 0f, half),
                RoadDirection.East => new Vector3(half, 0f, 0f),
                RoadDirection.South => new Vector3(0f, 0f, -half),
                RoadDirection.West => new Vector3(-half, 0f, 0f),
                _ => Vector3.zero,
            };

            return road.transform.position + offset;
        }

        private static RoadDirection Opposite(RoadDirection direction) =>
            (RoadDirection)(((int)direction + 2) % 4);

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_debugPath == null || _debugPath.Count == 0) return;

            for (var i = 0; i < _debugPath.Count; i++)
            {
                var t = _debugPath.Count <= 1 ? 0f : (float)i / (_debugPath.Count - 1);
                Gizmos.color = Color.Lerp(Color.green, Color.red, t);
                Gizmos.DrawSphere(_debugPath[i], 0.1f);

                if (i > 0)
                {
                    Gizmos.color = Color.Lerp(new Color(0f, 1f, 0f, 0.6f), new Color(1f, 0f, 0f, 0.6f), t);
                    Gizmos.DrawLine(_debugPath[i - 1], _debugPath[i]);
                }
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_debugPath[0], 0.18f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_debugPath[^1], 0.18f);

            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(_debugPath[0] + Vector3.up * 0.3f, "START");
            UnityEditor.Handles.Label(_debugPath[^1] + Vector3.up * 0.3f, "END");
        }
#endif
    }
}
