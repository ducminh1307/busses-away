using System.Collections.Generic;
using DucMinh;
using UnityEngine;

namespace BussesAway
{
    /// <summary>
    /// Hầm/bến đầu cuối của bus.
    /// Một lane dùng để tạo bus và chạy ra ngoài, lane còn lại dùng để bus đầy khách chạy vào.
    /// </summary>
    public class BusDepot : MonoBehaviour
    {
        [Header("Anchors")]
        [Tooltip("Điểm đặt bus khi được tạo trong hầm.")]
        [SerializeField] private Transform spawnPoint;
        [Tooltip("Điểm cuối cùng khi bus chạy vào hầm.")]
        [SerializeField] private Transform returnPoint;
        [Tooltip("Parent chứa các bus được tạo từ depot. Có thể để trống.")]
        [SerializeField] private Transform busParent;

        [Header("Road Links")]
        [Tooltip("Road phía trước lane chạy ra.")]
        [SerializeField] private Road departureRoad;
        [Tooltip("Tắt Auto Detect thì dùng hướng này để xác định bus đi vào departureRoad từ phía trong hầm.")]
        [SerializeField] private RoadDirection departureRoadEntryDirection = RoadDirection.South;
        [Tooltip("Road phía trước lane chạy vào.")]
        [SerializeField] private Road returnRoad;
        [Tooltip("Tự suy ra hướng đi ra từ spawnPoint thay vì phải set tay departureRoadEntryDirection.")]
        [SerializeField] private bool autoDetectDepartureEntryDirection = true;

        [Header("Spawn")]
        [SerializeField] private int defaultPassengerCapacity = 8;
        [SerializeField] private bool autoStartDeparture = false;

        [Header("Debug")]
        [SerializeField] private Bus lastSpawnedBus;
        [SerializeField] private Road debugCurrentRoad;
        [SerializeField] private RoadDirection debugCurrentEntryDirection = RoadDirection.South;

        [Header("Validation")]
        [TextArea(2, 5)]
        [SerializeField] private string inspectorWarningMessage;

        public Bus LastSpawnedBus => lastSpawnedBus;

        private void OnValidate()
        {
            inspectorWarningMessage = BuildInspectorWarningMessage();
        }

        public Bus SpawnBus(int passengerCapacity = -1, bool startDeparture = false)
        {
            if (!ValidateSpawnSetup())
                return null;

            var bus = BussesAwayConfig.BusPrefab.Create<Bus>(busParent, spawnPoint.position);
            if (bus == null)
            {
                Debug.LogWarning("[BusDepot] BusPrefab chưa được gán trong BussesAwayConfig.");
                return null;
            }

            var resolvedDepartureEntryDirection = ResolveDepartureEntryDirection();
            bus.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            bus.Construct(passengerCapacity > 0 ? passengerCapacity : defaultPassengerCapacity);
            bus.SetCurrentRoad(departureRoad, resolvedDepartureEntryDirection);

            lastSpawnedBus = bus;

            if (startDeparture)
                StartDeparture(bus);

            return bus;
        }

        public bool IsBusWaitingAtSpawn(Bus candidate, float tolerance = 0.05f)
        {
            if (candidate == null || spawnPoint == null)
                return false;

            return Vector3.Distance(candidate.transform.position, spawnPoint.position) <= tolerance;
        }

        public bool TryBuildPathToDestination(
            Road destinationRoad,
            out List<Vector3> path,
            out RoadDirection destinationEntryDirection,
            out List<int> teleportWaypointIndices)
        {
            path = null;
            destinationEntryDirection = ResolveDepartureEntryDirection();
            teleportWaypointIndices = new List<int>();

            if (!ValidateSpawnSetup())
                return false;

            if (destinationRoad == null)
            {
                Debug.LogWarning("[BusDepot] destinationRoad is null.");
                return false;
            }

            var resolvedDepartureEntryDirection = ResolveDepartureEntryDirection();
            if (!RoadPathfinder.TryFindPath(
                    departureRoad,
                    resolvedDepartureEntryDirection,
                    destinationRoad,
                    out var roadPath,
                    out destinationEntryDirection,
                    out var roadTeleportWaypointIndices))
            {
                return false;
            }

            path = BuildDeparturePath(resolvedDepartureEntryDirection);
            var departureRoadWaypoints = departureRoad.GetWorldWaypointsFrom(resolvedDepartureEntryDirection);
            var skipCount = Mathf.Min(departureRoadWaypoints.Count, roadPath.Count);
            var appendStartIndex = path.Count;

            for (var i = skipCount; i < roadPath.Count; i++)
                path.Add(roadPath[i]);

            foreach (var roadTeleportIndex in roadTeleportWaypointIndices)
            {
                if (roadTeleportIndex < skipCount)
                    continue;

                teleportWaypointIndices.Add(appendStartIndex + (roadTeleportIndex - skipCount));
            }

            return true;
        }

        public bool StartDeparture(Bus bus)
        {
            if (bus == null || !ValidateSpawnSetup())
                return false;

            var resolvedDepartureEntryDirection = ResolveDepartureEntryDirection();
            var path = BuildDeparturePath(resolvedDepartureEntryDirection);
            if (path.Count == 0)
            {
                Debug.LogWarning("[BusDepot] Không build được path chạy ra khỏi hầm.");
                return false;
            }

            bus.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            bus.SetCurrentRoad(departureRoad, resolvedDepartureEntryDirection);
            bus.SetPath(path);
            bus.StartDriving();
            return true;
        }

        public bool SendBusHome(Bus bus, Road fromRoad, RoadDirection fromEntryDirection)
        {
            if (bus == null)
            {
                Debug.LogWarning("[BusDepot] Bus = null.");
                return false;
            }

            if (!ValidateReturnSetup())
                return false;

            if (!RoadPathfinder.TryFindPathToRoadEntry(
                    fromRoad, fromEntryDirection, returnRoad,
                    out var path, out var returnRoadEntryDirection, out var teleportWaypointIndices))
            {
                Debug.LogWarning($"[BusDepot] Không tìm được đường từ '{fromRoad?.name}' về '{returnRoad?.name}'.");
                return false;
            }

            AppendRoadWaypointsTowardPoint(
                path,
                returnRoad.GetWorldWaypointsFrom(returnRoadEntryDirection),
                returnPoint.position);
            AppendPoint(path, returnPoint.position);

            bus.SetCurrentRoad(fromRoad, fromEntryDirection);
            bus.SetPath(path, teleportWaypointIndices);
            bus.StartDriving();
            return true;
        }

        public List<Vector3> BuildDeparturePath()
        {
            return BuildDeparturePath(ResolveDepartureEntryDirection());
        }

        private List<Vector3> BuildDeparturePath(RoadDirection resolvedDepartureEntryDirection)
        {
            var path = new List<Vector3>();
            if (!ValidateSpawnSetup())
                return path;

            path.Add(spawnPoint.position);
            AppendRoadWaypoints(path, departureRoad.GetWorldWaypointsFrom(resolvedDepartureEntryDirection));
            return path;
        }

        private RoadDirection ResolveDepartureEntryDirection()
        {
            if (!autoDetectDepartureEntryDirection || spawnPoint == null || departureRoad == null)
                return departureRoadEntryDirection;

            var bestDirection = departureRoadEntryDirection;
            var bestDistance = float.MaxValue;

            foreach (RoadDirection direction in System.Enum.GetValues(typeof(RoadDirection)))
            {
                var waypoints = departureRoad.GetWorldWaypointsFrom(direction);
                if (waypoints.Count == 0)
                    continue;

                var distance = Vector3.Distance(spawnPoint.position, waypoints[0]);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestDirection = direction;
                }
            }

            return bestDirection;
        }

        private bool ValidateSpawnSetup()
        {
            if (spawnPoint == null)
            {
                Debug.LogWarning("[BusDepot] spawnPoint chưa được gán.");
                return false;
            }

            if (departureRoad == null)
            {
                Debug.LogWarning("[BusDepot] departureRoad chưa được gán.");
                return false;
            }

            return true;
        }

        private bool ValidateReturnSetup()
        {
            if (returnPoint == null)
            {
                Debug.LogWarning("[BusDepot] returnPoint chưa được gán.");
                return false;
            }

            if (returnRoad == null)
            {
                Debug.LogWarning("[BusDepot] returnRoad chưa được gán.");
                return false;
            }

            return true;
        }

        private static void AppendRoadWaypoints(List<Vector3> path, List<Vector3> roadWaypoints)
        {
            if (roadWaypoints == null || roadWaypoints.Count == 0)
                return;

            var startIndex = 0;
            if (path.Count > 0 && Vector3.Distance(path[^1], roadWaypoints[0]) < 0.001f)
                startIndex = 1;

            for (var i = startIndex; i < roadWaypoints.Count; i++)
                path.Add(roadWaypoints[i]);
        }

        private static void AppendRoadWaypointsTowardPoint(List<Vector3> path, List<Vector3> roadWaypoints, Vector3 targetPoint)
        {
            if (roadWaypoints == null || roadWaypoints.Count == 0)
                return;

            var targetIndex = 0;
            var bestDistance = Vector3.Distance(roadWaypoints[0], targetPoint);
            for (var i = 1; i < roadWaypoints.Count; i++)
            {
                var distance = Vector3.Distance(roadWaypoints[i], targetPoint);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    targetIndex = i;
                }
            }

            var startIndex = 0;
            if (path.Count > 0 && Vector3.Distance(path[^1], roadWaypoints[0]) < 0.001f)
                startIndex = 1;

            for (var i = startIndex; i <= targetIndex; i++)
                path.Add(roadWaypoints[i]);
        }

        private static void AppendPoint(List<Vector3> path, Vector3 point)
        {
            if (path.Count > 0 && Vector3.Distance(path[^1], point) < 0.001f)
                return;

            path.Add(point);
        }

        private string BuildInspectorWarningMessage()
        {
            var warnings = new List<string>();

            if (spawnPoint != null && departureRoad != null && returnRoad != null)
            {
                var departureDistance = GetDistanceToRoadPath(spawnPoint.position, departureRoad);
                var returnDistance = GetDistanceToRoadPath(spawnPoint.position, returnRoad);
                if (returnDistance + 0.05f < departureDistance)
                {
                    warnings.Add(
                        $"spawn_point đang gần return_road hơn departure_road " +
                        $"(departure: {departureDistance:F2}, return: {returnDistance:F2}).");
                }
            }

            if (returnPoint != null && departureRoad != null && returnRoad != null)
            {
                var departureDistance = GetDistanceToRoadPath(returnPoint.position, departureRoad);
                var returnDistance = GetDistanceToRoadPath(returnPoint.position, returnRoad);
                if (departureDistance + 0.05f < returnDistance)
                {
                    warnings.Add(
                        $"return_point đang gần departure_road hơn return_road " +
                        $"(departure: {departureDistance:F2}, return: {returnDistance:F2}).");
                }
            }

            var result = warnings.Count == 0
                ? "No lane warnings."
                : string.Join("\n", warnings);

            if (autoDetectDepartureEntryDirection && spawnPoint != null && departureRoad != null)
                result += $"\nAuto departure direction: {ResolveDepartureEntryDirection()}";

            return result;
        }

        private static float GetDistanceToRoadPath(Vector3 worldPoint, Road road)
        {
            if (road == null)
                return float.MaxValue;

            var waypoints = road.GetWorldWaypoints();
            if (waypoints.Count == 0)
                return Vector3.Distance(worldPoint, road.transform.position);

            if (waypoints.Count == 1)
                return Vector3.Distance(worldPoint, waypoints[0]);

            var bestDistance = float.MaxValue;
            for (var i = 1; i < waypoints.Count; i++)
            {
                var closestPoint = ClosestPointOnSegment(waypoints[i - 1], waypoints[i], worldPoint);
                var distance = Vector3.Distance(worldPoint, closestPoint);
                if (distance < bestDistance)
                    bestDistance = distance;
            }

            return bestDistance;
        }

        private static Vector3 ClosestPointOnSegment(Vector3 start, Vector3 end, Vector3 point)
        {
            var segment = end - start;
            var sqrLength = segment.sqrMagnitude;
            if (sqrLength < 0.0001f)
                return start;

            var t = Vector3.Dot(point - start, segment) / sqrLength;
            t = Mathf.Clamp01(t);
            return start + segment * t;
        }

#if UNITY_EDITOR && DEBUG_MODE
        [Button]
        private void DebugSpawnBus()
        {
            SpawnBus(defaultPassengerCapacity, autoStartDeparture);
        }

        [Button]
        private void DebugStartDeparture()
        {
            if (lastSpawnedBus == null)
                return;

            StartDeparture(lastSpawnedBus);
        }

        [Button]
        private void DebugSendLastBusHome()
        {
            if (lastSpawnedBus == null || debugCurrentRoad == null)
                return;

            SendBusHome(lastSpawnedBus, debugCurrentRoad, debugCurrentEntryDirection);
        }
#endif

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (spawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(spawnPoint.position, 0.12f);
                Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + spawnPoint.forward * 0.6f);
            }

            if (returnPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(returnPoint.position, 0.12f);
                Gizmos.DrawLine(returnPoint.position, returnPoint.position + returnPoint.forward * 0.6f);
            }

            if (spawnPoint != null && departureRoad != null)
            {
                Gizmos.color = new Color(0f, 1f, 1f, 0.8f);
                var departurePath = BuildDeparturePath();
                for (var i = 1; i < departurePath.Count; i++)
                    Gizmos.DrawLine(departurePath[i - 1], departurePath[i]);
            }
        }
#endif
    }
}
