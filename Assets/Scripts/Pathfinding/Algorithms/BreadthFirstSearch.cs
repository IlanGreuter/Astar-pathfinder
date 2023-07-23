using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Winkeldief.Pathfinding.AStar
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct BreadthFirstSearch : IJob
    {
        [ReadOnly] readonly NativeArray<AstarTile> grid;
        [ReadOnly] readonly int2 gridSize, gridOffset;
        [ReadOnly] int2 start;
        [ReadOnly] int maxSearchDistance;

        [WriteOnly] NativeList<int2> tilesOutput;

        [BurstCompile] private int GetIndex(int x, int y) => x + (y * gridSize.x);
        [BurstCompile] private int GetIndex(int2 pos) => GetIndex(pos.x, pos.y);

        /// <summary> Check if tile is within the bounds of the grid </summary>
        [BurstCompile]
        private bool IsValid(int2 cell) =>
            cell.x >= 0 && cell.x < gridSize.x
                && cell.y >= 0 && cell.y < gridSize.y;

        public BreadthFirstSearch(NativeArray<AstarTile> tiles, int2 size, int2 offset, NativeList<int2> output)
        {
            grid = tiles;
            gridSize = size;
            gridOffset = offset;
            tilesOutput = output;

            start = new();
            maxSearchDistance = int.MaxValue;
        }

        public void SetPath(int2 start, int maxRange = int.MaxValue)
        {
            this.start = start - gridOffset;
            maxSearchDistance = maxRange;
        }

        //Returns a list of positions that form a path from start to end.
        //Will return null if no path was found
        public void Execute()
        {
            if (!IsValid(start))
                return;

            //Copy persistent array into a temp array
            NativeArray<AstarTile> tiles = new(grid.Length, Allocator.Temp);
            grid.CopyTo(tiles);

            AstarTile startTile = tiles[GetIndex(start)];
            startTile.G = 0;
            tiles[startTile.Index] = startTile;

            AstarHeap openTiles = new(tiles.Length, tiles);
            openTiles.Add(startTile);

            NativeArray<int2> neighbourOffsets = GetNeighboursArray();

            //As long as we have tiles to search
            while (!openTiles.IsEmpty)
            {
                //Take the best tile from the heap and add to closed
                AstarTile currentTile = openTiles.RemoveFirst();
                tilesOutput.Add(currentTile.ToInt2);

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

                    //If not yet searched and within search distance
                    if (tempG <= maxSearchDistance && !openTiles.Contains(nTile))
                    {
                        nTile.G = tempG;
                        openTiles.Add(nTile);
                    }
                }
            }

            //Dispose
            tiles.Dispose();
            openTiles.Dispose();
            neighbourOffsets.Dispose();
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
