using System;
using System.Collections.Generic;

namespace DucMinh
{
    public sealed class DelegatedPool<T> : IPool<T>
    {
        private readonly Stack<T> _stack;
        private readonly Func<T> _create;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;
        
        public bool AutoExpand { get; set; } = true;
        
        public int MaxSize { get; set; } = 0;

        public int CountInactive => _stack.Count;
        public int CountAll { get; private set; }
        
        #if UNITY_EDITOR
        private readonly HashSet<T> _inUse = new HashSet<T>();

        public int CountActive => _inUse.Count;
#else
        public int CountActive => CountAll - CountInactive;
#endif

        public DelegatedPool(
            Func<T> create,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            Action<T> onDestroy = null,
            int initialCapacity = 64)
        {
            _create = create ?? throw new ArgumentNullException(nameof(create));
            _onGet = onGet ?? (_ => { });
            _onRelease = onRelease ?? (_ => { });
            _onDestroy = onDestroy ?? (_ => { });
            _stack = new Stack<T>(Math.Max(1, initialCapacity));
        }

        public T Get()
        {
            T item;
            if (_stack.Count > 0)
            {
                item = _stack.Pop();
            }
            else
            {
                if (!AutoExpand && CountAll > 0)
                    throw new InvalidOperationException("Pool is empty and AutoExpand=false.");

                if (MaxSize > 0 && CountAll >= MaxSize)
                    throw new InvalidOperationException($"Pool reached MaxSize={MaxSize}.");

                item = _create();
                CountAll++;
            }

            _onGet(item);
#if UNITY_EDITOR
            _inUse.Add(item);
#endif
            return item;
        }

        public void Release(T item)
        {
            if (item == null) return;
#if UNITY_EDITOR
            if (!_inUse.Contains(item))
                Log.Warning($"[DelegatedPool<{typeof(T).Name}>] Releasing item not marked in-use.");
            _inUse.Remove(item);
#endif
            _onRelease(item);
            _stack.Push(item);
        }

        public void Preload(int count)
        {
            if (count <= 0) return;
            for (int i = 0; i < count; i++)
            {
                if (MaxSize > 0 && CountAll >= MaxSize) break;
                var it = _create();
                CountAll++;
                _onRelease(it); // đưa về trạng thái sẵn sàng
                _stack.Push(it);
            }
        }

        public void Clear()
        {
            // Huỷ phần inactive
            while (_stack.Count > 0)
            {
                var it = _stack.Pop();
                _onDestroy(it);
            }

#if UNITY_EDITOR
            // CẢNH BÁO: đang huỷ cả item đang sử dụng (tuỳ use-case).
            foreach (var it in _inUse)
                _onDestroy(it);
            _inUse.Clear();
#endif
            CountAll = 0;
        }

        public void Dispose()
        {
            Clear();
        }
    }
}