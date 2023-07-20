using Unity.Burst;
using Unity.Collections;

namespace Winkeldief.Pathfinding
{
    [BurstCompile]
    public struct Heap<T> where T : struct, IHeapItem<T>
    {
        NativeArray<T> heap;
        int currentCount;

        public int Count => currentCount;
        public bool IsEmpty => currentCount == 0;

        [BurstCompile] static int ParentIndex(int index) => (index - 1) / 2;
        [BurstCompile] static int LeftChildIndex(int index) => (index * 2) + 1;
        [BurstCompile] static int RightChildIndex(int index) => (index * 2) + 2;

        public Heap(int maxHeapSize)
        {
            heap = new(maxHeapSize, Allocator.Temp);
            currentCount = 0;
        }

        [BurstCompile]
        public void Add(T item)
        {
            item.HeapIndex = currentCount;
            heap[currentCount] = item;
            SortUp(item);
            currentCount++;
        }

        [BurstCompile]
        public T RemoveFirst()
        {
            T firstItem = heap[0];
            firstItem.HeapIndex = -1;
            currentCount--;

            T temp = heap[currentCount];
            temp.HeapIndex = 0;
            heap[0] = temp;

            SortDown(heap[0]);
            return firstItem;
        }

        [BurstCompile]
        public void UpdateItem(T item)
        {
            //Error Here
            //Change Heap to use grid & work with indexes?
            //Heap Updates Indexes?
            heap[item.HeapIndex] = item;
            SortUp(item);
        }

        [BurstCompile]
        private void Swap(T itemA, T itemB)
        {
            int temp = itemA.HeapIndex;
            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = temp;

            heap[itemA.HeapIndex] = itemA;
            heap[itemB.HeapIndex] = itemB;
        }

        [BurstCompile]
        private void SortUp(T item)
        {
            int parentIndex = ParentIndex(item.HeapIndex);
            while (true)
            {
                T parentItem = heap[parentIndex];

                if (item.CompareTo(parentItem) > 0)
                    Swap(item, parentItem);
                else break;

                parentIndex = ParentIndex(item.HeapIndex);
            }
        }

        [BurstCompile]
        private void SortDown(T item)
        {
            while (true)
            {
                int childIndexL = LeftChildIndex(item.HeapIndex);
                int childIndexR = RightChildIndex(item.HeapIndex);

                if (childIndexL < currentCount)
                {
                    int swapIndex = childIndexL;

                    if (childIndexR < currentCount && heap[childIndexR].CompareTo(heap[childIndexL]) > 0)
                        swapIndex = childIndexR;

                    if (item.CompareTo(heap[swapIndex]) < 0)
                        Swap(item, heap[swapIndex]);
                    else return;
                }
                else return;
            }
        }

        [BurstCompile]
        public bool Contains(T item)
        {
            return item.Equals(heap[item.HeapIndex]);
        }

        [BurstCompile]
        public void Dispose()
        {
            heap.Dispose();
        }
    }
}
