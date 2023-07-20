using Unity.Burst;
using Unity.Collections;

namespace Winkeldief.Pathfinding
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct AstarHeap
    {
        NativeArray<AstarTile> grid;
        NativeArray<int> heap;
        int currentCount;

        public int Count => currentCount;
        public bool IsEmpty => currentCount == 0;

        [BurstCompile] static int ParentIndex(int index) => (index - 1) / 2;
        [BurstCompile] static int LeftChildIndex(int index) => (index * 2) + 1;
        [BurstCompile] static int RightChildIndex(int index) => (index * 2) + 2;

        public AstarHeap(int maxHeapSize, NativeArray<AstarTile> grid)
        {
            heap = new(maxHeapSize, Allocator.Temp);
            currentCount = 0;
            this.grid = grid;
        }

        [BurstCompile]
        public void Add(AstarTile item)
        {
            item.HeapIndex = currentCount;
            heap[currentCount] = item.Index;
            grid[item.Index] = item;
            SortUp(item);
            currentCount++;
        }

        [BurstCompile]
        public AstarTile RemoveFirst()
        {
            AstarTile firstItem = grid[heap[0]];
            firstItem.HeapIndex = -1;
            grid[firstItem.Index] = firstItem;
            currentCount--;

            AstarTile temp = grid[heap[currentCount]];
            temp.HeapIndex = 0;
            heap[0] = temp.Index;
            grid[temp.Index] = temp;

            SortDown(temp);
            return firstItem;
        }

        [BurstCompile]
        public void UpdateItem(AstarTile item)
        {
            SortUp(item);
        }

        [BurstCompile]
        private void Swap(AstarTile itemA, AstarTile itemB)
        {
            int temp = itemA.HeapIndex;
            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = temp;

            heap[itemA.HeapIndex] = itemA.Index;
            heap[itemB.HeapIndex] = itemB.Index;
            grid[itemA.Index] = itemA;
            grid[itemB.Index] = itemB;
        }

        [BurstCompile]
        private void SortUp(AstarTile item)
        {
            while (true)
            {
                AstarTile parentItem = grid[heap[ParentIndex(item.HeapIndex)]];

                if (item.CompareTo(parentItem) > 0)
                {
                    Swap(item, parentItem);
                    item.HeapIndex = parentItem.HeapIndex;
                }
                else break;
            }
        }

        [BurstCompile]
        private void SortDown(AstarTile item)
        {
            while (true)
            {
                int childIndexL = LeftChildIndex(item.HeapIndex);
                int childIndexR = RightChildIndex(item.HeapIndex);

                if (childIndexL < currentCount)
                {
                    int swapIndex = childIndexL;

                    if (childIndexR < currentCount && grid[heap[childIndexR]].CompareTo(grid[heap[childIndexL]]) > 0)
                        swapIndex = childIndexR;

                    AstarTile swap = grid[heap[swapIndex]];
                    if (swap.CompareTo(item) > 0)
                    {
                        Swap(item, swap);
                        item.HeapIndex = swapIndex;
                    }
                    else return;
                }
                else return;
            }
        }

        [BurstCompile]
        public bool Contains(AstarTile item)
        {
            return item.Index == heap[item.HeapIndex];
        }

        [BurstCompile]
        public void Dispose()
        {
            heap.Dispose();
        }
    }
}
