using System;
using System.Collections.Generic;
using UnityEngine;

namespace DucMinh
{
    public delegate Vector2 GetPositionFunc<in T>(T item);

    /// <summary>
    /// Generic, pool-optimized Quadtree for spatial queries.
    /// Zero GC after warmup. Positions cached at insert time to avoid native bridge overhead.
    /// </summary>
    public class Quadtree<T>
    {
        private readonly int _maxObjects;
        private readonly int _maxLevels;
        private readonly GetPositionFunc<T> _getPosition;
        private readonly DelegatedPool<QuadtreeNode> _nodePool;
        private readonly ListPool<Entry> _listPool;
        private QuadtreeNode _root;

        public AABB Bounds => _root.Bounds;
        public int Count => _root.Count;

        /// <summary>Cached item + position pair to avoid repeated native bridge calls.</summary>
        public struct Entry
        {
            public T Item;
            public Vector2 Position;
        }

        public Quadtree(AABB bounds, GetPositionFunc<T> getPosition,
                        int maxObjects = 8, int maxLevels = 6, int preloadNodes = 20)
        {
            _getPosition = getPosition ?? throw new ArgumentNullException(nameof(getPosition));
            _maxObjects = maxObjects;
            _maxLevels = maxLevels;

            _nodePool = new DelegatedPool<QuadtreeNode>(
                create: () => new QuadtreeNode(),
                onRelease: node => node.Reset(),
                initialCapacity: preloadNodes
            );
            _listPool = new ListPool<Entry>(_maxObjects);
            _nodePool.Preload(preloadNodes);
            _root = AcquireNode(bounds, 0);
        }

        public void Clear()
        {
            RecycleChildren(_root);
            _root.ClearObjects();
        }

        public void SetBounds(AABB newBounds)
        {
            Clear();
            _root.SetBounds(newBounds);
        }

        public bool Insert(T item)
        {
            var pos = _getPosition(item);
            return InsertInternal(_root, new Entry { Item = item, Position = pos });
        }

        public bool Remove(T item)
        {
            var pos = _getPosition(item);
            return RemoveInternal(_root, item, pos);
        }

        public void Query(Vector2 point, List<T> results)
        {
            QueryPointInternal(_root, point, results);
        }

        public void Query(AABB range, List<T> results)
        {
            QueryRangeInternal(_root, range, results);
        }

        public void QueryRadius(Vector2 center, float radius, List<T> results)
        {
            var aabb = AABB.FromCenterExtents(center, new Vector2(radius, radius));
            QueryRadiusInternal(_root, center, radius * radius, aabb, results);
        }

        private bool InsertInternal(QuadtreeNode node, Entry entry)
        {
            if (!node.Bounds.Contains(entry.Position)) return false;

            if (node.HasChildren)
            {
                int idx = GetChildIndex(node.Bounds, entry.Position);
                if (InsertInternal(node.ChildAt(idx), entry))
                {
                    node.IncrementCount();
                    return true;
                }
                return false;
            }

            node.Objects.Add(entry);
            node.IncrementCount();

            if (node.Objects.Count > _maxObjects && node.Level < _maxLevels)
            {
                if (!node.HasChildren) SubdivideNode(node);

                var objects = node.Objects;
                for (int i = objects.Count - 1; i >= 0; i--)
                {
                    var e = objects[i];
                    InsertInternal(node.ChildAt(GetChildIndex(node.Bounds, e.Position)), e);
                    objects.RemoveAt(i);
                }
            }

            return true;
        }

        private bool RemoveInternal(QuadtreeNode node, T item, Vector2 pos)
        {
            if (!node.Bounds.Contains(pos)) return false;

            var objects = node.Objects;
            for (int i = 0; i < objects.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(objects[i].Item, item))
                {
                    objects.RemoveAt(i);
                    node.DecrementCount();
                    return true;
                }
            }

            if (node.HasChildren)
            {
                if (RemoveInternal(node.ChildAt(GetChildIndex(node.Bounds, pos)), item, pos))
                {
                    node.DecrementCount();
                    TryMerge(node);
                    return true;
                }
            }

            return false;
        }

        private void QueryPointInternal(QuadtreeNode node, Vector2 point, List<T> results)
        {
            if (!node.Bounds.Contains(point)) return;

            var objects = node.Objects;
            for (int i = 0; i < objects.Count; i++)
                results.Add(objects[i].Item);

            if (node.HasChildren)
                QueryPointInternal(node.ChildAt(GetChildIndex(node.Bounds, point)), point, results);
        }

        private void QueryRangeInternal(QuadtreeNode node, AABB range, List<T> results)
        {
            if (!node.Bounds.Intersects(range)) return;

            var objects = node.Objects;
            for (int i = 0; i < objects.Count; i++)
            {
                if (range.Contains(objects[i].Position))
                    results.Add(objects[i].Item);
            }

            if (node.HasChildren)
                for (int i = 0; i < 4; i++)
                    QueryRangeInternal(node.ChildAt(i), range, results);
        }

        private void QueryRadiusInternal(QuadtreeNode node, Vector2 center,
                                          float sqrRadius, AABB aabb, List<T> results)
        {
            if (!node.Bounds.Intersects(aabb)) return;

            var objects = node.Objects;
            for (int i = 0; i < objects.Count; i++)
            {
                if ((objects[i].Position - center).sqrMagnitude <= sqrRadius)
                    results.Add(objects[i].Item);
            }

            if (node.HasChildren)
                for (int i = 0; i < 4; i++)
                    QueryRadiusInternal(node.ChildAt(i), center, sqrRadius, aabb, results);
        }

        private void SubdivideNode(QuadtreeNode node)
        {
            var center = node.Bounds.Center;
            var min = node.Bounds.Min;
            var max = node.Bounds.Max;
            int childLevel = node.Level + 1;

            node.SetChild(0, AcquireNode(AABB.FromMinMax(min, center), childLevel));
            node.SetChild(1, AcquireNode(AABB.FromMinMax(new Vector2(center.x, min.y), new Vector2(max.x, center.y)), childLevel));
            node.SetChild(2, AcquireNode(AABB.FromMinMax(new Vector2(min.x, center.y), new Vector2(center.x, max.y)), childLevel));
            node.SetChild(3, AcquireNode(AABB.FromMinMax(center, max), childLevel));
            node.HasChildren = true;
        }

        private void TryMerge(QuadtreeNode node)
        {
            if (!node.HasChildren) return;

            int total = 0;
            for (int i = 0; i < 4; i++)
            {
                if (node.ChildAt(i).HasChildren) return;
                total += node.ChildAt(i).Objects.Count;
            }

            if (total + node.Objects.Count > _maxObjects) return;

            for (int i = 0; i < 4; i++)
            {
                var childObjects = node.ChildAt(i).Objects;
                for (int j = 0; j < childObjects.Count; j++)
                    node.Objects.Add(childObjects[j]);
            }

            RecycleChildren(node);
        }

        private QuadtreeNode AcquireNode(AABB bounds, int level)
        {
            var node = _nodePool.Get();
            node.Initialize(bounds, level, _listPool.Get());
            return node;
        }

        private void RecycleNode(QuadtreeNode node)
        {
            if (node == null) return;
            if (node.Objects != null) _listPool.Release(node.Objects);
            _nodePool.Release(node);
        }

        private void RecycleChildren(QuadtreeNode node)
        {
            if (!node.HasChildren) return;
            for (int i = 0; i < 4; i++)
            {
                RecycleChildren(node.ChildAt(i));
                RecycleNode(node.ChildAt(i));
                node.SetChild(i, null);
            }
            node.HasChildren = false;
        }

        private static int GetChildIndex(AABB bounds, Vector2 pos)
        {
            var center = bounds.Center;
            if (pos.x < center.x)
                return pos.y < center.y ? 0 : 2;
            else
                return pos.y < center.y ? 1 : 3;
        }

        /// <summary>
        /// Internal node. Pre-allocated fixed-size children array — no allocation on subdivide.
        /// </summary>
        internal class QuadtreeNode
        {
            public AABB Bounds;
            public int Level;
            public int Count;
            public List<Entry> Objects;
            public bool HasChildren;

            private readonly QuadtreeNode[] _children = new QuadtreeNode[4];

            public QuadtreeNode ChildAt(int index) => _children[index];
            public void SetChild(int index, QuadtreeNode child) => _children[index] = child;

            public void Initialize(AABB bounds, int level, List<Entry> objects)
            {
                Bounds = bounds;
                Level = level;
                Count = 0;
                Objects = objects;
                HasChildren = false;
            }

            public void Reset()
            {
                Bounds = default;
                Level = 0;
                Count = 0;
                Objects = null;
                HasChildren = false;
                _children[0] = null;
                _children[1] = null;
                _children[2] = null;
                _children[3] = null;
            }

            public void SetBounds(AABB bounds) => Bounds = bounds;
            public void ClearObjects() { Objects?.Clear(); Count = 0; }
            public void IncrementCount() => Count++;
            public void DecrementCount() => Count--;
        }

#if UNITY_EDITOR
        public void DrawGizmos(Color color = default)
        {
            if (color == default) color = Color.green;
            DrawGizmosRecursive(_root, color, 1f);
        }

        private void DrawGizmosRecursive(QuadtreeNode node, Color color, float alpha)
        {
            if (node == null) return;
            Gizmos.color = new Color(color.r, color.g, color.b, alpha);

            var min = node.Bounds.Min;
            var max = node.Bounds.Max;
            Gizmos.DrawLine(new Vector3(min.x, min.y), new Vector3(max.x, min.y));
            Gizmos.DrawLine(new Vector3(max.x, min.y), new Vector3(max.x, max.y));
            Gizmos.DrawLine(new Vector3(max.x, max.y), new Vector3(min.x, max.y));
            Gizmos.DrawLine(new Vector3(min.x, max.y), new Vector3(min.x, min.y));

            if (node.Objects != null && node.Objects.Count > 0)
            {
                Gizmos.color = new Color(1f, 0.3f, 0.3f, alpha);
                var center = node.Bounds.Center;
                float dotSize = Mathf.Min(node.Bounds.Width, node.Bounds.Height) * 0.05f;
                Gizmos.DrawWireSphere(new Vector3(center.x, center.y), dotSize);
            }

            if (node.HasChildren)
            {
                float childAlpha = Mathf.Max(alpha * 0.7f, 0.15f);
                for (int i = 0; i < 4; i++)
                    DrawGizmosRecursive(node.ChildAt(i), color, childAlpha);
            }
        }
#endif
    }

    /// <summary>GameObject-specific Quadtree. Drop-in convenience wrapper.</summary>
    public class GameObjectQuadtree : Quadtree<GameObject>
    {
        public GameObjectQuadtree(AABB bounds, int maxObjects = 8, int maxLevels = 6, int preloadNodes = 20)
            : base(bounds, go => (Vector2)go.transform.position, maxObjects, maxLevels, preloadNodes) { }
    }
}