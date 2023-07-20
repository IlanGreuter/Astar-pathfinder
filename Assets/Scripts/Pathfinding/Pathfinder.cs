using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Winkeldief.Utilities;

namespace Winkeldief.Pathfinding
{
    public class Pathfinder : Singleton<Pathfinder>
    {
        public NativeArray<AstarTile> grid;
        int2 size, offset;

        [SerializeField] Tilemap map;

        [Header("Pathfinding Config")]
        [SerializeField, Tooltip("Whether to allow diagonal movement in non-hex grids")]
        bool allowDiagonals;
        bool isHexGrid;
        int tileType;

        public static Vector3Int WorldToCell(Vector3 world) => instance.map.WorldToCell(world);
        public static Vector3 CellToWorld(Vector3Int cell) => instance.map.GetCellCenterWorld(cell);

        /// <summary> Returns a path with each node from start to end </summary>
        public static Path FindPath(Vector3Int start, Vector3Int end)
        {
            NativeList<int2> result = new(Allocator.TempJob);
            Astar astarJob = new Astar(instance.grid, instance.size, instance.offset, result);
            astarJob.SetPath(new int2(start.x, start.y), new int2(end.x, end.y));

            astarJob.Schedule().Complete();
            Path path = new(result, CellToWorld(Vector3Int.zero));
            return path;
        }

        /// <summary> Returns a list of paths. This method uses Jobs to calculate multiple paths at the same time </summary>
        /// <param name="pathStartEndPoints"> List with all start and end points of the paths to calculate. 
        /// List should be filled with tuples (Vector3Int start, Vector3Int end). </param>
        public static List<Path> GetPaths(List<(Vector3Int, Vector3Int)> pathStartEndPoints)
        {
            int numPaths = pathStartEndPoints.Count;

            NativeArray<NativeList<int2>> results = new(numPaths, Allocator.TempJob);
            NativeArray<JobHandle> jobHandleArray = new(numPaths, Allocator.TempJob);
            for (int i = 0; i < numPaths; i++)
            {
                results[i] = new(Allocator.TempJob);
                Astar astarJob = new Astar(instance.grid, instance.size, instance.offset, results[i]);

                (Vector3Int start, Vector3Int end) = pathStartEndPoints[i];
                astarJob.SetPath(new int2(start.x, start.y), new int2(end.x, end.y));
                jobHandleArray[i] = astarJob.Schedule();
            }

            JobHandle.CompleteAll(jobHandleArray);
            jobHandleArray.Dispose();

            List<Path> paths = new();
            Vector3 offset = CellToWorld(Vector3Int.zero);
            for (int i = 0; i < numPaths; i++)
                paths.Add(new Path(results[i], offset));

            results.Dispose();
            return paths;
        }

        /// <summary> Turns the tilemap into a grid that can be used for the pathfinding algorithm </summary>
        protected virtual void ConstructGrid()
        { //Extension: Combine multiple tilemaps
            map.CompressBounds();
            var bounds = map.cellBounds;
            size = new(bounds.size.x, bounds.size.y);
            offset = new(bounds.xMin, bounds.yMin);

            //Fill in the grid
            grid = new(size.x * size.y, Allocator.Persistent);
            for (int x = bounds.xMin, i = 0; x < bounds.xMax; x++, i++)
            {
                for (int y = bounds.yMin, j = 0; y < bounds.yMax; y++, j++)
                {
                    int index = i + (j * size.x);
                    AstarTile tile = new(x, y, index, tileType);
                    tile.Cost = GetTileCost(tile.ToVec3Int);
                    grid[index] = tile;
                }
            }
        }

        /// <summary> Takes a cell's position in the tilemap and returns a cost of that tile </summary>
        /// <returns> Assigns a cost to a tile. A tile is unwalkable if its score is -1. </returns>
        public virtual int GetTileCost(Vector3Int cell)
        { //Extension: Allow different types of tiles to have different costs
            return map.HasTile(cell) ? -1 : 1;
        }

        /// <summary> Update a tile's cost without having to reconstruct the entire grid </summary>
        public void UpdateTileCost(int x, int y, int cost)
        {
            int i = x + (y * size.x);
            AstarTile tile = grid[i];
            tile.Cost = cost;
            grid[i] = tile;
        }

        private void OnValidate()
        {
            if (map != null)
                isHexGrid = map.cellLayout == GridLayout.CellLayout.Hexagon;
            tileType = isHexGrid ? 6 : allowDiagonals ? 8 : 4;
        }

        protected override void Awake()
        {
            base.Awake();
            isHexGrid = map.cellLayout == GridLayout.CellLayout.Hexagon;
            tileType = isHexGrid ? 6 : allowDiagonals ? 8 : 4;
            ConstructGrid();
        }

        protected override void OnDestroy()
        {
            if (grid.IsCreated)
                grid.Dispose();
            base.OnDestroy();
        }
    }
}
