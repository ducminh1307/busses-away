using System.Collections.Generic;

namespace DucMinh
{
    public abstract class BasePool<T> : IPool<T>
    {
        protected Stack<T> _stack = new();
        protected readonly HashSet<T> _inUse = new();
        
        public void Dispose()
        {
            Clear();
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
                    throw new System.InvalidOperationException("Pool is empty and AutoExpand = false.");
                item = CreateInstance();
                CountAll++;
            }

            OnGet(item);
            _inUse.Add(item);
            return item;
        }

        public void Release(T item)
        {
            if (item == null) return;
            if (!_inUse.Contains(item))
            {
                Log.Warning($"[Pool] Releasing item that is not in-use: {item}");
            }
            _inUse.Remove(item);
            if (OnRelease(item))
            {
                _stack.Push(item);
            }
        }

        public int CountInactive => _stack.Count;

        public int CountActive => _inUse.Count;
        public int CountAll { get; protected set; }
        public bool AutoExpand { get; set; } = true;

        protected abstract T CreateInstance();
        protected virtual void DestroyInstance(T component) { }

        protected virtual bool OnRelease(T component) => false;
        protected virtual void OnGet(T item) { }
        
        public void Preload(int count)
        {
            if (count <= 0) return;
            for (int i = 0; i < count; i++)
            {
                var it = CreateInstance();
                CountAll++;
                if (OnRelease(it))
                {
                    _stack.Push(it);
                }
            }
        }

        public void Clear()
        {
            while (_stack.Count > 0)
            {
                var it = _stack.Pop();
                DestroyInstance(it);
            }

            foreach (var it in _inUse)
            {
                DestroyInstance(it);
            }
            _inUse.Clear();
            CountAll = 0;
        }
    }
}