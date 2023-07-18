using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Winkeldief.Pathfinding
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct Astar : IJob
    {
        [ReadOnly] readonly NativeArray<AstarTile> grid;
        [ReadOnly] readonly int2 gridSize, gridOffset;
        [ReadOnly] readonly int2 start, end;

        private int GetIndex(int x, int y) => x + (y * gridSize.x);
        private int GetIndex(int2 pos) => GetIndex(pos.x, pos.y);

        /// <summary> Check if tile is within the bounds of the grid </summary>
        private bool IsValid(int2 cell) =>
            cell.x >= 0 && cell.x < gridSize.x
                && cell.y >= 0 && cell.y < gridSize.y;

        public Astar(NativeArray<AstarTile> tiles, int2 size, int2 offset, int2 start, int2 end)
        {
            grid = tiles;
            gridSize = size;
            gridOffset = offset;

            this.start = start - offset;
            this.end = end - offset;
        }

        //Returns a list of positions that form a path from start to end.
        //Will return null if no path was found
        public void Execute()
        {
            if (!IsValid(start) || !IsValid(end))
                return;// new(Allocator.Temp);

            //Copy persistent array into a temp array
            NativeArray<AstarTile> tiles = new(grid.Length, Allocator.Temp);
            grid.CopyTo(tiles);

            AstarTile startTile = tiles[GetIndex(start)];
            AstarTile endTile = tiles[GetIndex(end)];
            startTile.CalculateCost(endTile);
            startTile.G = 0;
            tiles[startTile.Index] = startTile;

            //Return if endTile is not walkable
            if (endTile.Cost < 0)
            {
                tiles.Dispose();
                return;// new(Allocator.Temp);
            }

            NativeList<int> openTiles = new(Allocator.Temp);
            NativeList<int> closedTiles = new(Allocator.Temp);
            openTiles.Add(startTile.Index);

            NativeArray<int2> neighbourOffsets = GetNeighboursArray();

            //As long as we have tiles to search
            while (!openTiles.IsEmpty)
            {
                //Find best tile
                int currentIndex = GetLowestFIndex(tiles, openTiles); //Extension: This is very slow

                //If tile is end, stop searching
                if (currentIndex == endTile.Index)
                    break;

                //Remove current from open and add to closed tiles
                openTiles.RemoveAtSwapBack(openTiles.IndexOf(currentIndex));
                closedTiles.Add(currentIndex);

                AstarTile currentTile = tiles[currentIndex];
                int adjecentFlags = currentTile.GetNeighbourFlags(currentTile.Y);

                //Foreach neighbour
                for (int i = 0; i < neighbourOffsets.Length; i++)
                {
                    int2 neighbour = currentTile.ToInt2 - gridOffset + neighbourOffsets[i];

                    //Skip if not in grid or not adjecent
                    if ((adjecentFlags & (1 << i)) == 0 || !IsValid(neighbour))
                        continue;

                    int nIndex = GetIndex(neighbour);
                    AstarTile nTile = tiles[nIndex];

                    //Skip if already searched or if not walkable
                    if (nTile.Cost < 0 || closedTiles.Contains(nIndex))
                        continue;

                    int tempG = currentTile.G + currentTile.GetDistanceTo(nTile.X, nTile.Y);

                    //If not yet searched or better path was found
                    if (tempG < nTile.G)
                    {
                        nTile.G = tempG;
                        nTile.CalculateCost(endTile);
                        nTile.Previous = currentIndex;
                        tiles[nIndex] = nTile;

                        if (!openTiles.Contains(nIndex))
                            openTiles.Add(nIndex);
                    }
                }
            }

            endTile = tiles[endTile.Index];
            NativeList<int2> path = (endTile.Previous != -1) ?
                ConstructPath(tiles, endTile, true) :
                new(Allocator.Temp);

            //Dispose
            tiles.Dispose();
            openTiles.Dispose();
            closedTiles.Dispose();
            neighbourOffsets.Dispose();

            return;// path;
        }

        [BurstCompile]
        private int GetLowestFIndex(NativeArray<AstarTile> tiles, NativeList<int> indices)
        { //Extension: this can be made more efficient (heap?)
            AstarTile lowest = tiles[indices[0]];
            for (int i = 1; i < indices.Length; i++)
                lowest = lowest.Compare(tiles[indices[i]]);
            return lowest.Index;
        }

        /// <summary> Construct path once end has been found </summary>
        [BurstCompile]
        private NativeList<int2> ConstructPath(NativeArray<AstarTile> tiles, AstarTile end, bool includeStart)
        {
            NativeList<int2> path = new(Allocator.Temp);
            AstarTile current = end;

            while (current.Previous > -1)
            {
                path.Add(current.ToInt2);
                current = tiles[current.Previous];
            }

            if (includeStart) path.Add(current.ToInt2);
            return path;
        }

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

        /// <summary> Disposes of the internally stored grid, freeing up memory </summary>
        public void Dispose()
        {
            grid.Dispose();
        }
    }
}
