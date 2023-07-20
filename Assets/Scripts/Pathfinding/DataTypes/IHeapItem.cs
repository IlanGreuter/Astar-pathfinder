using System;

namespace Winkeldief.Pathfinding
{
    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex { get; set; }

        public abstract bool Equals(T other);
    }
}
