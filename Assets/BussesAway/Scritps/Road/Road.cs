using System.Collections.Generic;
using UnityEngine;

namespace BussesAway
{
    /// <summary>
    /// Loại đường xác định hướng đi vào/ra của xe buýt trên mảnh đường.
    /// </summary>
    public enum RoadType
    {
        Straight,   // đường thẳng: 2 điểm kết nối đối diện nhau
        Curve,      // đường cong 90°: 2 điểm kết nối vuông góc
        Tunnel,     // đường hầm: đi vào đầu này, thoát ra ở tunnel pair
        DeadEnd,    // đầu cuối / chỗ đỗ: 1 điểm kết nối
        Blank,      // mảnh trống không có đường
    }

    /// <summary>
    /// Hướng kết nối của một mảnh đường (tính theo local space của tile grid).
    /// </summary>
    public enum RoadDirection
    {
        North = 0,  // +Z
        East  = 1,  // +X
        South = 2,  // -Z
        West  = 3,  // -X
    }

    public enum TunnelRole
    {
        Bidirectional,
        EntranceOnly,
        ExitOnly,
    }

    /// <summary>
    /// Một slot kết nối: hướng ra ngoài và tham chiếu tới Road kề bên (có thể null).
    /// </summary>
    [System.Serializable]
    public class RoadConnection
    {
        [Tooltip("Hướng kết nối tính từ trung tâm mảnh đường này.")]
        public RoadDirection direction;

        [Tooltip("Mảnh đường liền kề theo hướng này (kéo thả trong Inspector).")]
        public Road neighbor;
    }

    /// <summary>
    /// Component gắn vào mỗi prefab mảnh đường.
    /// Định nghĩa waypoints nội bộ và các slot kết nối tới Road lân cận,
    /// cho phép bus duyệt lần lượt qua chuỗi Road để tạo thành path di chuyển.
    /// </summary>
    public class Road : MonoBehaviour
    {
        private readonly struct ConnectionAnchor
        {
            public readonly RoadDirection Direction;
            public readonly Vector3 Point;

            public ConnectionAnchor(RoadDirection direction, Vector3 point)
            {
                Direction = direction;
                Point = point;
            }
        }
        // ── Inspector ──────────────────────────────────────────────────────────

        [Header("Road Info")]
        [SerializeField] private RoadType roadType = RoadType.Straight;

        [Header("Waypoints (local space, theo thứ tự di chuyển)")]
        [Tooltip("Các điểm kiểm soát nội bộ của mảnh đường tính theo local space. " +
                 "Nếu để trống, Road tự tạo 2 waypoint đơn giản dựa trên RoadType.")]
        [SerializeField] private List<Vector3> localWaypoints = new();

        [Header("Grid")]
        [Tooltip("Kích thước 1 tile theo world-space. Dùng để tự động phát hiện neighbor.")]
        [SerializeField] private float tileSize = 1f;

        [Header("Connections")]
        [Tooltip("Tự động điền bằng nút Auto-Detect hoặc RoadEditor. Vẫn có thể kéo thả thủ công.")]
        [SerializeField] private List<RoadConnection> connections = new();

        [Header("Tunnel")]
        [Tooltip("Chỉ dùng cho RoadType.Tunnel. Tunnel ghép cặp để bus teleport sang đầu còn lại.")]
        [SerializeField] private Road tunnelPair;
        [Tooltip("EntranceOnly: chỉ đầu vào mới teleport. ExitOnly: chỉ là đầu thoát. Bidirectional: đi 2 chiều.")]
        [SerializeField] private TunnelRole tunnelRole = TunnelRole.Bidirectional;

        // ── Properties ────────────────────────────────────────────────────────

        public RoadType RoadType => roadType;

        /// <summary>Kích thước 1 tile (world-space). Dùng cho proximity detection.</summary>
        public float TileSize => tileSize;

        /// <summary>Danh sách kết nối sang Road lân cận.</summary>
        public IReadOnlyList<RoadConnection> Connections => connections;
        public Road TunnelPair => tunnelPair;
        public bool IsTunnel => roadType == RoadType.Tunnel || tunnelPair != null;
        public TunnelRole TunnelMode => tunnelRole;

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Trả về danh sách waypoints của mảnh đường này ở world space.
        /// </summary>
        public List<Vector3> GetWorldWaypoints()
        {
            EnsureDefaultWaypoints();
            var result = new List<Vector3>(localWaypoints.Count);
            foreach (var local in localWaypoints)
                result.Add(transform.TransformPoint(local));
            return result;
        }

        public List<Vector3> GetWorldWaypointsFromTunnelInterior()
        {
            var world = GetWorldWaypoints();
            world.Reverse();
            return world;
        }

        /// <summary>
        /// Trả về Road láng giềng theo hướng chỉ định (null nếu không có).
        /// </summary>
        public Road GetNeighbor(RoadDirection direction)
        {
            foreach (var conn in connections)
                if (conn.direction == direction)
                    return conn.neighbor;
            return null;
        }

        /// <summary>
        /// Tự động phát hiện và gán các Road lân cận dựa trên vị trí world-space.
        /// Quét 4 hướng (N/E/S/W) cách <see cref="tileSize"/> đơn vị,
        /// tìm Road nào có tâm nằm gần vị trí đó (trong ngưỡng 10% tileSize).
        /// Xoá connections cũ trước khi gán lại.
        /// Có thể gọi từ [ContextMenu] (chuột phải vào component trong Inspector)
        /// hoặc từ RoadEditor bằng nút "Auto-Link ALL Roads".
        /// </summary>
        [ContextMenu("Auto-Detect Connections")]
        public void AutoDetectConnections()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Auto-Detect Road Connections");
#endif
            connections.Clear();

            // Lấy tất cả Road trong scene (kể cả inactive)
            var allRoads = FindObjectsByType<Road>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var threshold = tileSize * 0.1f; // 10% tileSize làm ngưỡng sai số

            var dirOffsets = new (RoadDirection dir, Vector3 offset)[]
            {
                (RoadDirection.North, Vector3.forward  * tileSize),
                (RoadDirection.East,  Vector3.right    * tileSize),
                (RoadDirection.South, Vector3.back     * tileSize),
                (RoadDirection.West,  Vector3.left     * tileSize),
            };

            foreach (var (dir, offset) in dirOffsets)
            {
                var expectedPos = transform.position + offset;
                // Bỏ qua Y để hoạt động tốt dù các tile có độ cao khác nhau nhẹ
                expectedPos.y   = transform.position.y;

                Road foundNeighbor = null;

                foreach (var road in allRoads)
                {
                    if (road == this) continue;

                    var neighborPos = road.transform.position;
                    neighborPos.y   = transform.position.y;

                    if (Vector3.Distance(neighborPos, expectedPos) <= threshold)
                    {
                        foundNeighbor = road;
                        break; // Chỉ 1 neighbor mỗi hướng
                    }
                }

                // Fallback cho road custom/dài như lane trong depot:
                // nếu center-to-center không match theo grid thì thử nối theo endpoint.
                if (foundNeighbor == null)
                    foundNeighbor = FindNeighborByEndpoint(allRoads, dir, threshold);

                if (foundNeighbor != null)
                    connections.Add(new RoadConnection { direction = dir, neighbor = foundNeighbor });
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Build một chuỗi world-space waypoints liên tục bắt đầu từ Road này,
        /// duyệt tối đa <paramref name="maxRoads"/> mảnh đường.
        /// </summary>
        /// <param name="entryDirection">
        /// Hướng mà bus đi VÀO mảnh đường đầu tiên (từ phía nào đến).
        /// Dùng để xác định chiều duyệt waypoints (xuôi hay ngược).
        /// </param>
        /// <param name="maxRoads">Số mảnh đường tối đa để tránh vòng lặp vô hạn.</param>
        public List<Vector3> BuildPath(RoadDirection entryDirection, int maxRoads = 100)
        {
            var path = new List<Vector3>();
            var visited = new HashSet<Road>();
            var current = this;
            var entry = entryDirection;

            while (current != null && !visited.Contains(current) && maxRoads-- > 0)
            {
                visited.Add(current);

                // Lấy waypoints của mảnh hiện tại theo đúng chiều bus đi vào
                var waypoints = current.GetWorldWaypointsFrom(entry);
                // Bỏ điểm đầu nếu đã có (trừ mảnh đầu tiên) để tránh trùng lặp
                var startIndex = (path.Count > 0 && waypoints.Count > 0) ? 1 : 0;
                for (var i = startIndex; i < waypoints.Count; i++)
                    path.Add(waypoints[i]);

                // Tìm kết nối đi tiếp: lấy connection khác với chiều entry
                current = current.GetNextRoad(entry, out var nextEntry);
                entry = nextEntry;
            }

            return path;
        }

        // ── Internal Helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Trả về waypoints world-space đã sắp xếp đúng chiều bus di chuyển.
        /// Dùng so sánh khoảng cách không gian: waypoint nào gần biên entry hơn
        /// thì đặt làm điểm đầu — không phụ thuộc vào thứ tự connections trong Inspector.
        /// </summary>
        public List<Vector3> GetWorldWaypointsFrom(RoadDirection entryDirection)
        {
            EnsureDefaultWaypoints();
            var world = GetWorldWaypoints();
            if (world.Count <= 1) return world;

            var entryBorderWorld = transform.position + DirectionToWorldOffset(entryDirection);

            var distFirst = Vector3.Distance(world[0], entryBorderWorld);
            var distLast  = Vector3.Distance(world[world.Count - 1], entryBorderWorld);

            // Nếu waypoints[last] gần entry hơn → list đang ngược → đảo lại
            if (distLast < distFirst)
                world.Reverse();

            return world;
        }

        public bool TryGetTunnelTeleportTarget(out Road pairRoad, out RoadDirection pairEntryDirection)
        {
            pairRoad = null;
            pairEntryDirection = RoadDirection.South;

            if (tunnelPair == null || tunnelPair == this)
                return false;

            if (tunnelRole == TunnelRole.ExitOnly)
                return false;

            if (!tunnelPair.TryGetTunnelMouthDirection(out var pairMouthDirection))
                return false;

            pairRoad = tunnelPair;
            pairEntryDirection = Opposite(pairMouthDirection);
            return true;
        }

        public bool TryGetTunnelMouthDirection(out RoadDirection mouthDirection)
        {
            mouthDirection = RoadDirection.South;
            if (!IsTunnel)
                return false;

            EnsureDefaultWaypoints();
            if (localWaypoints == null || localWaypoints.Count == 0)
                return false;

            var mouthPoint = transform.TransformPoint(localWaypoints[0]);

            var foundConnection = false;
            var bestDistance = float.MaxValue;

            foreach (var conn in connections)
            {
                if (conn.neighbor == null) continue;

                var distance = Vector3.Distance(mouthPoint, transform.position + DirectionToWorldOffset(conn.direction));
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    mouthDirection = conn.direction;
                    foundConnection = true;
                }
            }

            if (foundConnection)
                return true;

            foreach (RoadDirection direction in System.Enum.GetValues(typeof(RoadDirection)))
            {
                var distance = Vector3.Distance(mouthPoint, transform.position + DirectionToWorldOffset(direction));
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    mouthDirection = direction;
                }
            }

            return true;
        }

        /// <summary>
        /// Offset world-space từ tâm tile tới biên theo hướng grid (North = +Z, East = +X, …).
        /// </summary>
        private Vector3 DirectionToWorldOffset(RoadDirection dir)
        {
            var half = tileSize * 0.5f;
            return dir switch
            {
                RoadDirection.North => new Vector3( 0f,   0f,  half),
                RoadDirection.East  => new Vector3( half, 0f,  0f),
                RoadDirection.South => new Vector3( 0f,   0f, -half),
                RoadDirection.West  => new Vector3(-half, 0f,  0f),
                _                   => Vector3.zero,
            };
        }

        private Road FindNeighborByEndpoint(Road[] allRoads, RoadDirection dir, float threshold)
        {
            if (!TryGetEndpointForDirection(dir, out var myAnchor))
                return null;

            var dirVector = DirectionToWorldOffset(dir).normalized;
            Road bestRoad = null;
            var bestDistance = float.MaxValue;
            var endpointThreshold = Mathf.Max(threshold, tileSize * 0.6f);

            foreach (var road in allRoads)
            {
                if (road == this) continue;
                if (!road.TryGetEndpointForDirection(Opposite(dir), out var neighborAnchor))
                    continue;

                var offset = neighborAnchor - myAnchor;
                offset.y = 0f;

                var distance = offset.magnitude;
                if (distance > endpointThreshold)
                    continue;

                if (offset.sqrMagnitude > 0.0001f && Vector3.Dot(offset.normalized, dirVector) < 0.75f)
                    continue;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestRoad = road;
                }
            }

            return bestRoad;
        }

        private bool TryGetEndpointForDirection(RoadDirection dir, out Vector3 endpoint)
        {
            endpoint = Vector3.zero;
            foreach (var anchor in GetConnectionAnchorsWorld())
            {
                if (anchor.Direction != dir)
                    continue;

                endpoint = anchor.Point;
                return true;
            }

            return false;
        }

        private List<ConnectionAnchor> GetConnectionAnchorsWorld()
        {
            var world = GetWorldWaypoints();
            var anchors = new List<ConnectionAnchor>();
            if (world.Count < 2)
                return anchors;

            anchors.Add(new ConnectionAnchor(
                GetDirectionFromVector(world[0] - world[1]),
                world[0]));

            if (roadType != RoadType.DeadEnd && roadType != RoadType.Tunnel)
            {
                anchors.Add(new ConnectionAnchor(
                    GetDirectionFromVector(world[^1] - world[^2]),
                    world[^1]));
            }

            return anchors;
        }

        private List<Vector3> GetEndpointsWorld()
        {
            var world = GetWorldWaypoints();
            if (world.Count == 0)
                return new List<Vector3>();

            if (world.Count == 1)
                return new List<Vector3> { world[0] };

            return new List<Vector3> { world[0], world[world.Count - 1] };
        }

        private static RoadDirection GetDirectionFromVector(Vector3 vector)
        {
            vector.y = 0f;

            if (Mathf.Abs(vector.x) >= Mathf.Abs(vector.z))
                return vector.x >= 0f ? RoadDirection.East : RoadDirection.West;

            return vector.z >= 0f ? RoadDirection.North : RoadDirection.South;
        }

        /// <summary>
        /// Tìm Road tiếp theo và hướng entry tương ứng.
        /// </summary>
        private Road GetNextRoad(RoadDirection currentEntry, out RoadDirection nextEntry)
        {
            if (TryGetTunnelMouthDirection(out var tunnelMouthDirection) &&
                currentEntry == tunnelMouthDirection &&
                TryGetTunnelTeleportTarget(out var pairRoad, out var pairEntryDirection))
            {
                nextEntry = pairEntryDirection;
                return pairRoad;
            }

            nextEntry = currentEntry;
            foreach (var conn in connections)
            {
                // EntryDir = phía bus đã đi VÀO → bỏ connection cùng chiều đó (= đi ngược lại)
                if (conn.direction == currentEntry) continue;
                if (conn.neighbor == null) continue;

                nextEntry = Opposite(conn.direction); // bus vào neighbor từ phía đối diện
                return conn.neighbor;
            }
            return null;
        }

        /// <summary>
        /// Tạo waypoints mặc định nếu chưa được cấu hình trong Inspector.
        /// </summary>
        private void EnsureDefaultWaypoints()
        {
            if (localWaypoints != null && localWaypoints.Count > 0) return;

            localWaypoints = new List<Vector3>();
            switch (roadType)
            {
                case RoadType.Straight:
                    // Dọc theo trục Z (North–South)
                    localWaypoints.Add(new Vector3(0f, 0f, -0.5f));
                    localWaypoints.Add(new Vector3(0f, 0f,  0.5f));
                    break;

                case RoadType.Curve:
                    // Cong từ South lên East (¼ cung tròn, 3 điểm)
                    localWaypoints.Add(new Vector3( 0f, 0f, -0.5f));
                    localWaypoints.Add(new Vector3( 0f, 0f,  0f));
                    localWaypoints.Add(new Vector3( 0.5f, 0f, 0f));
                    break;

                case RoadType.Tunnel:
                    localWaypoints.Add(new Vector3(0f, 0f, -0.5f));
                    localWaypoints.Add(new Vector3(0f, 0f,  0.35f));
                    break;

                case RoadType.DeadEnd:
                    localWaypoints.Add(new Vector3(0f, 0f, -0.5f));
                    localWaypoints.Add(new Vector3(0f, 0f,  0f));
                    break;

                default:
                    localWaypoints.Add(Vector3.zero);
                    break;
            }
        }

        private static RoadDirection Opposite(RoadDirection dir) =>
            (RoadDirection)(((int)dir + 2) % 4);

        // ── Gizmos ────────────────────────────────────────────────────────────

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            DrawWaypointGizmos();
            DrawConnectionGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            DrawWaypointGizmos(selected: true);
            DrawConnectionGizmos();
        }

        private void DrawWaypointGizmos(bool selected = false)
        {
            EnsureDefaultWaypoints();
            if (localWaypoints == null || localWaypoints.Count == 0) return;

            var color = selected ? new Color(1f, 0.8f, 0f) : new Color(1f, 0.8f, 0f, 0.4f);
            Gizmos.color = color;

            Vector3? prev = null;
            for (int i = 0; i < localWaypoints.Count; i++)
            {
                var world = transform.TransformPoint(localWaypoints[i]);
                Gizmos.DrawSphere(world, 0.07f);
                if (prev.HasValue)
                {
                    Gizmos.color = selected ? Color.cyan : new Color(0f, 1f, 1f, 0.4f);
                    Gizmos.DrawLine(prev.Value, world);
                    // Vẽ mũi tên hướng
                    var dir = (world - prev.Value).normalized;
                    var right = Vector3.Cross(Vector3.up, dir) * 0.08f;
                    Gizmos.DrawLine(world, world - dir * 0.15f + right);
                    Gizmos.DrawLine(world, world - dir * 0.15f - right);
                    Gizmos.color = color;
                }
                prev = world;
            }
        }

        private void DrawConnectionGizmos()
        {
            if (connections == null) return;
            foreach (var conn in connections)
            {
                if (conn.neighbor == null) continue;
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, conn.neighbor.transform.position);
                Gizmos.DrawWireSphere(conn.neighbor.transform.position, 0.12f);
            }
        }
#endif
    }
}
