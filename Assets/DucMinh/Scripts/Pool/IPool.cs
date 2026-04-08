using System;

namespace DucMinh
{
    public interface IPool<T> : IDisposable
    {
        T Get();
        void Release(T item);
        
        int CountActive { get; }
        int CountInactive { get; }
        int CountAll { get; }

        void Preload(int count);
        void Clear();
    }
}