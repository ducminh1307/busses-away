using System.Collections.Generic;
using UnityEngine;

namespace DucMinh
{
    public class GridMap : SingletonDestroyableBehavior<GridMap>
    {
        [SerializeField] private Vector2Int gridSize = new Vector2Int(10, 10);
        [SerializeField] private LayerMask obstacleLayer;

        private Node[,] grid;
        private Vector3Int gridOffset;
        private List<Vector2Int> obstacleCells;
        private Dictionary<Vector2Int, List<NavAgent>> navAgentGrid; 
        
        public static LayerMask ObstacleLayer => Instance.obstacleLayer;

        protected override void OnAwake()
        {
            base.OnAwake();
            
            gridOffset = new Vector3Int(-gridSize.x / 2, -gridSize.y / 2);
            navAgentGrid = new();
            obstacleCells = new();
            InitializeGrid();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearInstance();
        }

        private void InitializeGrid()
        {
            grid = new Node[gridSize.x, gridSize.y];
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    var worldX = x + gridOffset.x;
                    var worldY = y + gridOffset.y;
                    Vector2Int worldPos = new(worldX, worldY);
                    var walkable = !Physics2D.OverlapCircle(worldPos, 0.2f, obstacleLayer);
                    grid[x, y] = new Node(walkable, worldPos);
                }
            }
        }

        public Node GetNode(Vector2 worldPos)
        {
            var pos = WorldToGrid(worldPos);
            return grid[pos.x, pos.y];
        }

        public bool IsValidPosition(Vector2 worldPos)
        {
            var pos = WorldToGrid(worldPos);
            return pos.x >= 0 && pos.x < gridSize.x && pos.y >= 0 && pos.y < gridSize.y;
        }
        
        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            int gridX = Mathf.RoundToInt(worldPos.x) - gridOffset.x;
            int gridY = Mathf.RoundToInt(worldPos.y) - gridOffset.y;
            return new Vector2Int(gridX, gridY);
        }

        public List<Node> GetNeighbors(Node node, MovementType movementType)
        {
            List<Node> neighbors = new();
            var gridPos = WorldToGrid(node.position);

            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { 1, 0, -1, 0 };

            if (movementType == MovementType.Diagonal)
            {
                dx = new int[] { 0, 1, 1, 1, 0, -1, -1, -1 };
                dy = new int[] { 1, 1, 0, -1, -1, -1, 0, 1 };
            }

            for (int i = 0; i < dx.Length; i++)
            {
                int checkX = gridPos.x + dx[i];
                int checkY = gridPos.y + dy[i];

                if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y)
                {
                    Node neighbor = grid[checkX, checkY];

                    // Nếu là di chuyển chéo, kiểm tra các ô vuông góc liền kề
                    if (movementType == MovementType.Diagonal && dx[i] != 0 && dy[i] != 0)
                    {
                        int orthoX = gridPos.x + dx[i];
                        int orthoY = gridPos.y + dy[i];
                        if (!grid[orthoX, gridPos.y].isWalkable || !grid[gridPos.x, orthoY].isWalkable)
                        {
                            continue; // Bỏ qua neighbor chéo nếu một trong hai ô vuông góc không đi được
                        }
                    }

                    neighbors.Add(neighbor);
                }
            }
            return neighbors;
        }

        public void RegisterAgent(NavAgent agent)
        {
            Vector2Int gridPos = WorldToGrid(agent.transform.position);
            if (!navAgentGrid.ContainsKey(gridPos))
                navAgentGrid[gridPos] = new List<NavAgent>();
            navAgentGrid[gridPos].Add(agent);
        }

        public void UnregisterAgent(NavAgent navAgents)
        {
            Vector2Int gridPos = WorldToGrid(navAgents.transform.position);
            if (navAgentGrid.TryGetValue(gridPos, out var agents))
            {
                if (agents.TryRemoveValue(navAgents))
                    if (agents.IsNullOrEmpty())
                        navAgentGrid.Remove(gridPos);
            }
        }

        public List<NavAgent> GetNearbyAgents(Vector2 position, float radius)
        {
            List<NavAgent> nearbyAgents = new();
            Vector2Int center = WorldToGrid(position);
            int gridRadius = Mathf.RoundToInt(radius);

            for (int x = -gridRadius; x <= gridRadius; x++)
            {
                for (int y = -gridRadius; y <= gridRadius; y++)
                {
                    Vector2Int checkPos = new(center.x + x, center.y + y);
                    if (navAgentGrid.TryGetValue(checkPos, out var navAgents))
                    {
                        foreach (var agent in navAgents)
                        {
                            if (Vector2.Distance(position, agent.transform.position) <= radius)
                            {
                                nearbyAgents.Add(agent);
                            }
                        }
                    }
                }
            }
            
            return nearbyAgents;
        }
        
        public List<Vector2> GetNearbyObstacles(Vector2 position, float radius)
        {
            List<Vector2> nearbyObstacles = new();
            Vector2Int center = WorldToGrid(position);
            int gridRadius = Mathf.RoundToInt(radius);

            foreach (var obstacleCell in obstacleCells)
            {
                if (Mathf.Abs(obstacleCell.x - center.x) <= gridRadius &&
                    Mathf.Abs(obstacleCell.y - center.y) <= gridRadius)
                {
                    Vector2 worldPos = grid[obstacleCell.x, obstacleCell.y].position;
                    if (Vector2.Distance(position, worldPos) <= radius)
                    {
                        nearbyObstacles.Add(worldPos);
                    }
                }
            }
            
            return nearbyObstacles;
        }
        
        void OnDrawGizmos()
        {
            if (grid == null) return;

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Gizmos.color = grid[x, y].isWalkable ? Color.green : Color.red;
                    Vector3 worldPos = new Vector3(grid[x, y].position.x, grid[x, y].position.y, 0);
                    Gizmos.DrawWireCube(worldPos, new Vector3(1, 1, 0.1f));
                }
            }
        }
    }
}