using System.Collections.Generic;

namespace DucMinh
{
    public class ListPool<T> : BasePool<List<T>>
    {
        private readonly int _defaultCapacity;
        private readonly int _maxRetainedCapacity;

        public ListPool(int defaultCapacity = 4, int maxRetainedCapacity = 1024)
        {
            _defaultCapacity = defaultCapacity;
            _maxRetainedCapacity = maxRetainedCapacity;
        }

        protected override List<T> CreateInstance()
        {
            return new List<T>(_defaultCapacity);
        }

        protected override bool OnRelease(List<T> component)
        {
            if (component == null) return false;
            component.Clear();
            if (component.Capacity > _maxRetainedCapacity)
            {
                return false;
            }
            return true;
        }
    }
}