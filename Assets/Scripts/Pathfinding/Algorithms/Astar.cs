using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Winkeldief.Pathfinding.AStar
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct Astar : IJob
    {
        [ReadOnly] readonly NativeArray<AstarTile> grid;
        [ReadOnly] readonly int2 gridSize, gridOffset;
        [ReadOnly] int2 start, end;
        [ReadOnly] int maxSearchDistance;

        [WriteOnly] NativeList<int2> pathOutput;

        [BurstCompile] private int GetIndex(int x, int y) => x + (y * gridSize.x);
        [BurstCompile] private int GetIndex(int2 pos) => GetIndex(pos.x, pos.y);

        /// <summary> Check if tile is within the bounds of the grid </summary>
        [BurstCompile]
        private bool IsValid(int2 cell) =>
            cell.x >= 0 && cell.x < gridSize.x
                && cell.y >= 0 && cell.y < gridSize.y;

        public Astar(NativeArray<AstarTile> tiles, int2 size, int2 offset, NativeList<int2> output)
        {
            grid = tiles;
            gridSize = size;
            gridOffset = offset;
            pathOutput = output;

            start = new();
            end = new();
            maxSearchDistance = int.MaxValue;
        }

        public void SetPath(int2 start, int2 end, int maxRange = int.MaxValue)
        {
            this.start = start - gridOffset;
            this.end = end - gridOffset;
            maxSearchDistance = maxRange;
        }

        //Returns a list of positions that form a path from start to end.
        //Will return null if no path was found
        [BurstCompile]
        public void Execute()
        {
            if (!IsValid(start) || !IsValid(end))
                return;

            //Copy persistent array into a temp array
            NativeArray<AstarTile> tiles = new(grid.Length, Allocator.Temp);
            grid.CopyTo(tiles);

            AstarTile startTile = tiles[GetIndex(start)];
            AstarTile endTile = tiles[GetIndex(end)];
            startTile.G = 0;
            startTile.CalculateHCost(endTile);
            tiles[startTile.Index] = startTile;

            //Return if endTile is not walkable
            if (endTile.Cost < 0)
            {
                tiles.Dispose();
                return;
            }

            AstarHeap openTiles = new(tiles.Length, tiles);
            openTiles.Add(startTile);

            NativeArray<int2> neighbourOffsets = GetNeighboursArray();

            //As long as we have tiles to search
            while (!openTiles.IsEmpty)
            {
                //Take the best tile from the heap and add to closed
                AstarTile currentTile = openTiles.RemoveFirst();

                //If tile is end, stop searching
                if (currentTile.Index == endTile.Index)
                    break;

                int adjecentFlags = currentTile.GetNeighbourFlags();

                //Foreach neighbour
                for (int i = 0; i < neighbourOffsets.Length; i++)
                {
                    int2 neighbour = currentTile.ToInt2 - gridOffset + neighbourOffsets[i];

                    //Skip if not in grid or not adjecent
                    if ((adjecentFlags & (1 << i)) == 0 || !IsValid(neighbour))
                        continue;

                    AstarTile nTile = tiles[GetIndex(neighbour)];

                    //Skip if already searched or if not walkable
                    if (nTile.Cost < 0 || nTile.HeapIndex < 0)
                        continue;

                    int tempG = currentTile.G + currentTile.GetDistanceTo(nTile.Pos);

                    //If not yet searched or better path was found
                    if (tempG < nTile.G && tempG <= maxSearchDistance)
                    {
                        nTile.G = tempG;
                        nTile.CalculateHCost(endTile);
                        nTile.Previous = currentTile.Index;
                        tiles[nTile.Index] = nTile;

                        if (!openTiles.Contains(nTile))
                            openTiles.Add(nTile);
                        else
                            openTiles.UpdateItem(nTile);
                    }
                }
            }

            endTile = tiles[endTile.Index];
            if (endTile.Previous != -1)
                ConstructPath(tiles, endTile, true);

            //Dispose
            tiles.Dispose();
            openTiles.Dispose();
            neighbourOffsets.Dispose();
        }

        /// <summary> Construct path once end has been found </summary>
        [BurstCompile]
        private void ConstructPath(NativeArray<AstarTile> tiles, AstarTile end, bool includeStart)
        {
            AstarTile current = end;

            while (current.Previous > -1)
            {
                pathOutput.Add(current.ToInt2);
                current = tiles[current.Previous];
            }

            if (includeStart) pathOutput.Add(current.ToInt2);
        }

        /// <summary> Returns an array will 8 neighbours a tile can have </summary>
        [BurstCompile]
        public NativeArray<int2> GetNeighboursArray()
        {
            NativeArray<int2> neighbours = new(8, Allocator.Temp);
            neighbours[0] = new int2(-1, 0);
            neighbours[1] = new int2(1, 0);
            neighbours[2] = new int2(0, 1);
            neighbours[3] = new int2(0, -1);
            neighbours[4] = new int2(-1, 1);
            neighbours[5] = new int2(1, 1);
            neighbours[6] = new int2(-1, -1);
            neighbours[7] = new int2(1, -1);
            return neighbours;
        }
    }
}
