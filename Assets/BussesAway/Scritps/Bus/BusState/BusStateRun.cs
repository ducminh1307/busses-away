using System.Collections.Generic;
using DucMinh;
using UnityEngine;

namespace BussesAway
{
    /// <summary>
    /// State xe buýt đang chạy theo path được cấu hình sẵn.
    /// Path là danh sách Vector3 world-space waypoints được truyền vào qua Bus.SetPath().
    /// </summary>
    public class BusStateRun : State<Bus, BusStateID>
    {
        private List<Vector3> path;
        private HashSet<int> teleportWaypointIndices;
        private int waypointIndex;

        private float moveSpeed;
        private float rotateSpeed;

        public BusStateRun(string animationName, Entity<BusStateID> entity)
            : base(animationName, entity) { }

        public override void OnEnter()
        {
            base.OnEnter();
            path         = Entity.CurrentPath;
            teleportWaypointIndices = Entity.CurrentTeleportWaypointIndices;
            waypointIndex = 0;
            moveSpeed    = Entity.MoveSpeed;
            rotateSpeed  = Entity.RotateSpeed;

            if (path == null || path.Count == 0)
            {
                Entity.ChangeState(BusStateID.Idle);
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (path == null || waypointIndex >= path.Count)
            {
                Entity.ChangeState(BusStateID.Idle);
                return;
            }

            MoveTowardsWaypoint();
        }

        private void MoveTowardsWaypoint()
        {
            var target    = path[waypointIndex];
            var current   = Entity.transform.position;

            if (teleportWaypointIndices != null && teleportWaypointIndices.Contains(waypointIndex))
            {
                Entity.transform.position = target;
                SnapRotationToUpcomingPath();
                waypointIndex++;
                return;
            }

            var direction = target - current;
            var distance  = direction.magnitude;

            if (distance < 0.05f)
            {
                waypointIndex++;
                return;
            }

            // Di chuyển
            var delta = moveSpeed * Time.deltaTime;
            Entity.transform.position = Vector3.MoveTowards(current, target, delta);

            // Xoay mượt về hướng di chuyển
            var lookRot = Quaternion.LookRotation(direction.normalized, Vector3.up);
            Entity.transform.rotation = Quaternion.RotateTowards(
                Entity.transform.rotation, lookRot, rotateSpeed * Time.deltaTime);
        }

        private void SnapRotationToUpcomingPath()
        {
            for (var i = waypointIndex + 1; i < path.Count; i++)
            {
                var direction = path[i] - Entity.transform.position;
                if (direction.sqrMagnitude < 0.0001f)
                    continue;

                Entity.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                return;
            }
        }
    }
}
