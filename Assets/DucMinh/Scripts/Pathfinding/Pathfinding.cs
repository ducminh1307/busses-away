using System.Collections.Generic;
using UnityEngine;

namespace DucMinh
{
    public class Node
    {
        public bool isWalkable;
        public Vector2 position;
        public int gCost;
        public int hCost;
        public int fCost => gCost + hCost;
        public Node parent;

        public Node(bool isWalkable, Vector2Int position)
        {
            this.isWalkable = isWalkable;
            this.position = position;
        }
    }

    public enum MovementType
    {
        Orthogonal,
        Diagonal,
    }

    [DisallowMultipleComponent]
    public class Pathfinding : MonoBehaviour
    {
        [SerializeField] private MovementType movementType = MovementType.Diagonal;
        private Transform target;
        private bool isStopped = false;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void Stop()
        {
            isStopped = true;
        }

        public void Resume()
        {
            isStopped = false;
        }

        public List<Vector2> GetPath(Vector2 startPosition)
        {
            if (isStopped)
            {
                return null;
            }

            if (target == null)
            {
                Debug.LogWarning("Target chưa được gán cho Pathfinding!");
                return null;
            }

            Node startNode = GridMap.Instance.GetNode(startPosition);
            Node targetNode = GridMap.Instance.GetNode(target.position);

            List<Node> nodePath = FindPath(startNode, targetNode);
            if (nodePath == null) return null;

            List<Vector2> path = new List<Vector2>();
            foreach (Node node in nodePath)
            {
                path.Add(node.position);
            }

            return path;
        }

        private List<Node> FindPath(Node startNode, Node targetNode)
        {
            if (!GridMap.Instance.IsValidPosition(startNode.position) ||
                !GridMap.Instance.IsValidPosition(targetNode.position))
                return null;

            List<Node> openSet = new List<Node> { startNode };
            HashSet<Node> closedSet = new HashSet<Node>();

            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost ||
                        (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    return RetracePath(startNode, targetNode);
                }

                foreach (Node neighbor in GridMap.Instance.GetNeighbors(currentNode, movementType))
                {
                    if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                        continue;

                    int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                    if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, targetNode);
                        neighbor.parent = currentNode;

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return null;
        }

        private int GetDistance(Node a, Node b)
        {
            int dstX = Mathf.Abs((int)a.position.x - (int)b.position.x);
            int dstY = Mathf.Abs((int)a.position.y - (int)b.position.y);

            if (movementType == MovementType.Diagonal)
            {
                return 14 * Mathf.Min(dstX, dstY) + 10 * Mathf.Abs(dstX - dstY);
            }
            else
            {
                return 10 * (dstX + dstY);
            }
        }

        private List<Node> RetracePath(Node startNode, Node targetNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = targetNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }

            path.Reverse();
            return path;
        }
    }
}