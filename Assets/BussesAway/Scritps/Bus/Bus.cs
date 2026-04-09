using System;
using System.Collections.Generic;
using DucMinh;
using PrimeTween;
using TMPro;
using UnityEngine;

namespace BussesAway
{
    public partial class Bus: Entity<BusStateID>
    {
        [Header("Visual")]
        [SerializeField] private GameObject animObj;
        [SerializeField] private GameObject rendererObj;
        
        [Header("Configs")]
        [SerializeField] private Transform parentPassenger;
        [SerializeField] private Transform boardingPointR;
        [SerializeField] private Transform boardingPointL;
        [SerializeField] private List<TMP_Text> numberOfPassengerText = new();

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotateSpeed = 360f;

        [Header("Current Road")]
        [Tooltip("Mảnh đường bus đang đứng. Đặt trong Inspector hoặc gọi SetCurrentRoad().")]
        [SerializeField] private Road currentRoad;
        [Tooltip("Hướng bus đi vào currentRoad (dùng cho bước pathfinding đầu tiên).")]
        [SerializeField] private RoadDirection currentEntryDirection = RoadDirection.South;

        /// <summary>Tốc độ di chuyển của xe buýt (units/giây).</summary>
        public float MoveSpeed => moveSpeed;

        /// <summary>Tốc độ xoay của xe buýt (độ/giây).</summary>
        public float RotateSpeed => rotateSpeed;

        /// <summary>Mảnh đường bus hiện đang đứng/xuất phát.</summary>
        public Road CurrentRoad => currentRoad;
        public RoadDirection CurrentEntryDirection => currentEntryDirection;

        /// <summary>Path hiện tại xe buýt sẽ đi theo (world-space waypoints).</summary>
        public List<Vector3> CurrentPath { get; private set; }
        public HashSet<int> CurrentTeleportWaypointIndices { get; private set; } = new();
        
        [Header("Animation")] 
        [SerializeField] private float timeAnimationDoor = 0.5f;
        private SkinnedMeshRenderer skinnedMesh;
        private BoxCollider passengerBounds;
        
        private int _maxPassengerCount;
        public int MaxPassengerCount => _maxPassengerCount;
        private readonly List<Passenger> passengers = new();
        private readonly List<Vector3> passengerLocalPositions = new();
        
        protected override void Awake()
        {
            AnimationAdapter = AnimationAdapter.Create(animObj);
            StateFactory = new BusStateFactory(this);
            
            if (skinnedMesh == null)
            {
                skinnedMesh = rendererObj.GetComponent<SkinnedMeshRenderer>();
            }

            if (parentPassenger != null)
            {
                passengerBounds = parentPassenger.GetComponent<BoxCollider>();
            }
            
            base.Awake();
        }

#if DEBUG_MODE
        [Button]
#endif
        public void Construct(int maxPassengerCount)
        {
            _maxPassengerCount = maxPassengerCount;
            ChangeDoor(isOpen: false);
            SetFull(false);
            passengers.Clear();
            BuildPassengerGrid();
            SetShowPassengerText(true);
            UpdateNumberOfPassengersText();
#if DEBUG_MODE
            for (int i = 0; i < maxPassengerCount - 1; i++)
            {
                var passenger = BussesAwayConfig.PassengerPrefab.Create<Passenger>();
                AddPassenger(passenger);
            }
#endif
        }

        // ── Path / Movement ───────────────────────────────────────────────────

        /// <summary>
        /// Đặt Road mà bus đang đứng và hướng bus đi vào tile đó.
        /// Gọi trước khi dùng DriveTo().
        /// </summary>
        public void SetCurrentRoad(Road road, RoadDirection entryDirection)
        {
            currentRoad           = road;
            currentEntryDirection = entryDirection;
        }

        /// <summary>
        /// Tự động tìm đường ngắn nhất tới <paramref name="destination"/> rồi chạy.
        /// Cần đặt CurrentRoad trước (qua Inspector hoặc SetCurrentRoad()).
        /// </summary>
        /// <param name="destination">Mảnh đường đích.</param>
        /// <returns>true nếu tìm thấy đường và bắt đầu chạy.</returns>
        public bool DriveTo(Road destination)
        {
            if (currentRoad == null)
            {
                Debug.LogWarning("[Bus] CurrentRoad chưa được set. Hãy gọi SetCurrentRoad() hoặc khai báo trong Inspector.");
                return false;
            }

            if (!RoadPathfinder.TryFindPath(currentRoad, currentEntryDirection, destination, out var path, out _, out var teleportWaypointIndices))
                return false;

            SetPath(path, teleportWaypointIndices);
            StartDriving();
            return true;
        }

        /// <summary>
        /// Gán path world-space waypoints cho xe buýt (không tự chạy, gọi StartDriving() sau).
        /// </summary>
        public void SetPath(List<Vector3> waypoints)
        {
            SetPath(waypoints, null);
        }

        public void SetPath(List<Vector3> waypoints, IEnumerable<int> teleportWaypointIndices)
        {
            CurrentPath = waypoints;
            CurrentTeleportWaypointIndices = teleportWaypointIndices != null
                ? new HashSet<int>(teleportWaypointIndices)
                : new HashSet<int>();
        }

        /// <summary>
        /// Gán path từ một Road đầu tiên (tuyến tuyến tuyến tính theo connection chain).
        /// </summary>
        public void SetPath(Road startRoad, RoadDirection entryDirection)
        {
            if (startRoad == null) return;
            CurrentPath = startRoad.BuildPath(entryDirection);
            CurrentTeleportWaypointIndices = new HashSet<int>();
        }

        /// <summary>Bắt đầu di chuyển theo path đã được gán.</summary>
        public void StartDriving()
        {
            if (CurrentPath == null || CurrentPath.Count == 0)
            {
                Debug.LogWarning("[Bus] Chưa có path. Hãy gọi SetPath() hoặc DriveTo() trước.");
                return;
            }
            ChangeState(BusStateID.Run);
        }

        /// <summary>Dừng xe buýt và chuyển về Idle.</summary>
        public void StopDriving()
        {
            ChangeState(BusStateID.Idle);
        }

        public void ChangeDoor(Action onCompleted = null, bool isOpen = true)
        {
            if (skinnedMesh.IsNullObject()) return;
            var start = isOpen ? 0 : 100;
            var end = isOpen ? 100 : 0;
            Tween.Custom(start, end,  timeAnimationDoor, value =>
            {
                skinnedMesh?.SetBlendShapeWeight(1, value);
            });
        }

        private void SetFull(bool full)
        {
            if (skinnedMesh.IsNullObject()) return;
            skinnedMesh.SetBlendShapeWeight(0, full? 100: 0);
        }

        public void AddPassenger(Passenger passenger)
        {
            if (passenger == null || parentPassenger == null)
            {
                return;
            }

            if (passengerLocalPositions.Count == 0)
            {
                BuildPassengerGrid();
            }

            if (passengers.Count >= MaxPassengerCount || passengers.Count >= passengerLocalPositions.Count)
            {
                SetFull(true);
                return;
            }

            passengers.Add(passenger);
            passenger.transform.SetParent(parentPassenger, false);
            passenger.SetLocalPosition(passengerLocalPositions[passengers.Count - 1]);
            passenger.transform.localRotation = Quaternion.identity;
            passenger.SetScale(.7f);
            
            UpdateNumberOfPassengersText();
            SetFull(passengers.Count >= MaxPassengerCount);
        }

        private void UpdateNumberOfPassengersText()
        {
            if (numberOfPassengerText.IsNullOrEmpty()) return;

            var displayNumber = MaxPassengerCount - passengers.Count;
            if (displayNumber <= 0)
            {
                SetShowPassengerText(false);
                return;
            }
            foreach (var tmpText in numberOfPassengerText)
            {
                tmpText.SetText(displayNumber.ToString());
            }
        }

        private void SetShowPassengerText(bool show)
        {
            foreach (var tmpText in numberOfPassengerText)
            {
                tmpText.SetShow(show);
            }
        }

        private void BuildPassengerGrid()
        {
            passengerLocalPositions.Clear();

            if (parentPassenger == null || MaxPassengerCount <= 0)
            {
                return;
            }

            if (passengerBounds == null)
            {
                passengerBounds = parentPassenger.GetComponent<BoxCollider>();
            }

            if (passengerBounds == null)
            {
                for (var i = 0; i < MaxPassengerCount; i++)
                {
                    passengerLocalPositions.Add(Vector3.zero);
                }

                return;
            }

            var boundsCenter = passengerBounds.center;
            var boundsSize = passengerBounds.size;
            var boundsMin = boundsCenter - boundsSize * 0.5f;

            var columns = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(MaxPassengerCount)));
            var rows = Mathf.Max(1, Mathf.CeilToInt(MaxPassengerCount / (float) columns));
            var stepX = columns == 1 ? 0f : boundsSize.x / columns;
            var stepZ = rows == 1 ? 0f : boundsSize.z / rows;
            var yPosition = boundsMin.y;

            for (var index = 0; index < MaxPassengerCount; index++)
            {
                var row = index / columns;
                var column = index % columns;
                var xPosition = columns == 1
                    ? boundsCenter.x
                    : boundsMin.x + stepX * (column + 0.5f);
                var zPosition = rows == 1
                    ? boundsCenter.z
                    : boundsMin.z + stepZ * (row + 0.5f);
                passengerLocalPositions.Add(new Vector3(xPosition, yPosition, zPosition));
            }
        }
    }
}
