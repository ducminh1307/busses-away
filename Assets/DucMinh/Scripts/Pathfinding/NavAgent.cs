using System.Collections.Generic;
// using NaughtyAttributes;
using UnityEngine;

namespace DucMinh
{
    [RequireComponent(typeof(Pathfinding))]
    public class NavAgent : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float updatePathInterval = 0.5f;
        [SerializeField] private float minDistanceToAgent = 0.5f;
        [SerializeField] private float minDistanceToObstacle = 0.3f;
// #if UNITY_EDITOR
//         [Header("Gizmos")]
//         [SerializeField] bool gizmosEnabled = true;
//         [SerializeField, ShowIf("gizmosEnabled")] private Color roadColor = Color.yellow;
//         [SerializeField, ShowIf("gizmosEnabled")] private Color distanceObstacleColor = Color.red;
//         [SerializeField, ShowIf("gizmosEnabled")] private Color distanceOtherAgentColor = Color.blue;
// #endif


        private Pathfinding pathfinding;
        private List<Vector2> path;
        private int pathIndex;
        private float pathUpdateTimer;
        private Vector2Int lastGridPos;

        // Bình phương của khoảng cách để so sánh
        private float sqrMinDistanceToAgent;
        private float sqrMinDistanceToObstacle;

        void Awake()
        {
            pathfinding = gameObject.GetOrAddComponent<Pathfinding>();

            sqrMinDistanceToAgent = minDistanceToAgent * minDistanceToAgent;
            sqrMinDistanceToObstacle = minDistanceToObstacle * minDistanceToObstacle;
        }

        void Start()
        {
            pathUpdateTimer = updatePathInterval;
            GridMap.Instance.RegisterAgent(this);
            lastGridPos = GridMap.Instance.WorldToGrid(transform.position);
        }

        void OnDestroy()
        {
            GridMap.Instance.UnregisterAgent(this);
        }

        public void OnUpdate()
        {
            pathUpdateTimer -= Time.deltaTime;
            if (pathUpdateTimer <= 0)
            {
                UpdatePath();
                pathUpdateTimer = updatePathInterval;
            }

            FollowPath();
        }

        public void SetDestination(Transform target)
        {
            pathfinding.SetTarget(target);
        }

        public void Stop()
        {
            pathfinding.Stop();
            path = null;
        }

        public void Resume()
        {
            pathfinding.Resume();
            pathUpdateTimer = 0;
        }

        private void UpdatePath()
        {
            path = pathfinding.GetPath(transform.position);
            pathIndex = 0;
        }

        private void FollowPath()
        {
            if (path == null || pathIndex >= path.Count) return;

            Vector2 targetPos = path[pathIndex];
            Vector2 newPosition = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            // Cập nhật vị trí trong agentGrid
            Vector2Int newGridPos = GridMap.Instance.WorldToGrid(newPosition);
            if (newGridPos != lastGridPos)
            {
                GridMap.Instance.UnregisterAgent(this);
                GridMap.Instance.RegisterAgent(this);
                lastGridPos = newGridPos;
            }

            // Tránh chồng lấn với agent và obstacle
            newPosition = AvoidOverlap(newPosition);
            newPosition = AvoidObstacles(newPosition);

            transform.position = newPosition;

            // Dùng sqrMagnitude để kiểm tra khoảng cách đến điểm tiếp theo
            if ((newPosition - targetPos).sqrMagnitude < 0.01f) // 0.01f = (0.1f)^2
            {
                pathIndex++;
            }
        }

        private Vector2 AvoidOverlap(Vector2 desiredPosition)
        {
            List<NavAgent> nearbyAgents = GridMap.Instance.GetNearbyAgents(desiredPosition, minDistanceToAgent);
            foreach (var agent in nearbyAgents)
            {
                if (agent != this)
                {
                    Vector2 toAgent = desiredPosition - (Vector2)agent.transform.position;
                    float sqrDistance = toAgent.sqrMagnitude;
                    if (sqrDistance < sqrMinDistanceToAgent)
                    {
                        Vector2 direction = toAgent.normalized;
                        float distance = Mathf.Sqrt(sqrDistance); // Chỉ lấy căn khi cần điều chỉnh vị trí
                        desiredPosition += direction * (minDistanceToAgent - distance);
                    }
                }
            }

            return desiredPosition;
        }

        private Vector2 AvoidObstacles(Vector2 desiredPosition)
        {
            List<Vector2> nearbyObstacles = GridMap.Instance.GetNearbyObstacles(desiredPosition, minDistanceToObstacle);
            foreach (var obstaclePos in nearbyObstacles)
            {
                Vector2 toObstacle = desiredPosition - obstaclePos;
                float sqrDistance = toObstacle.sqrMagnitude;
                if (sqrDistance < sqrMinDistanceToObstacle)
                {
                    Vector2 direction = toObstacle.normalized;
                    float distance = Mathf.Sqrt(sqrDistance); // Chỉ lấy căn khi cần điều chỉnh vị trí
                    desiredPosition += direction * (minDistanceToObstacle - distance);
                }
            }

            return desiredPosition;
        }

#if UNITY_EDITOR
        // void OnDrawGizmos()
        // {
        //     if (!gizmosEnabled) return;
        //     
        //     if (path != null)
        //     {
        //         Gizmos.color = roadColor;
        //         for (int i = 0; i < path.Count - 1; i++)
        //         {
        //             Gizmos.DrawLine(path[i], path[i + 1]);
        //         }
        //     }
        //
        //     Gizmos.color = distanceOtherAgentColor;
        //     Gizmos.DrawWireSphere(transform.position, minDistanceToAgent);
        //     Gizmos.color = distanceObstacleColor;
        //     Gizmos.DrawWireSphere(transform.position, minDistanceToObstacle);
        // }
#endif
    }
}