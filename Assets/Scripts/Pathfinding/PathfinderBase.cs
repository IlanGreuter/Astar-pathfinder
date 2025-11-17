using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Winkeldief.Utilities;
using Winkeldief.Pathfinding.AStar;

namespace Winkeldief.Pathfinding
{
    public abstract class PathfinderBase : MonoSingleton<PathfinderBase>
    {
        protected NativeArray<AstarTile> grid;
        protected int2 gridSize, gridOffset;

        [Tooltip("The base tilemap that the pathfinding grid will be based off")]
        public Tilemap map;

        [Header("Pathfinding Config")]
        [SerializeField] TileType tileType = TileType.Square;
        [Tooltip("The maximum distance to search for a path. -1 to disable. For reference, the base distance between adjecent tiles is 10 (or 14 for diagonal)")]
        public int maxSearchDistance = -1;

        /// <summary> Returns the grid position of the tile the world position is in </summary>
        public static Vector3Int WorldToCell(Vector3 world) => instance.map.WorldToCell(world);
        /// <summary> Returns the world coordinates of the center of the grid position </summary>
        public static Vector3 CellToWorld(Vector3Int cell) => instance.map.GetCellCenterWorld(cell);

        #region FindPath
        private Astar GetAstarJob(int2 start, int2 end, NativeList<int2> output)
        {
            Astar astarJob = new(grid, gridSize, gridOffset, output);
            astarJob.SetPath(start, end, maxSearchDistance > 0 ? maxSearchDistance : int.MaxValue);
            return astarJob;
        }

        private BreadthFirstSearch GetFrontierJob(int2 start, int maxDistance, NativeList<int2> output)
        {
            BreadthFirstSearch bfs = new(grid, gridSize, gridOffset, output);
            bfs.SetPath(start, maxDistance > 0 ? maxDistance : int.MaxValue);
            return bfs;
        }

        /// <summary> Returns a path with each node from start to end </summary>
        public static Path FindPath(Vector3Int start, Vector3Int end)
        {
            NativeList<int2> result = new(Allocator.TempJob);
            Astar astarJob = instance.GetAstarJob(start.ToInt2(), end.ToInt2(), result);
            astarJob.Schedule().Complete();

            return new Path(result, CellToWorld(Vector3Int.zero));
        }

        /// <summary> Returns a list of paths. This method uses Jobs to calculate multiple paths at the same time </summary>
        /// <param name="pathStartEndPoints"> List with all start and end points of the paths to calculate. 
        /// List should be filled with tuples (Vector3Int start, Vector3Int end). </param>
        public static List<Path> FindMultiplePaths(List<(Vector3Int, Vector3Int)> pathStartEndPoints)
        {
            int numPaths = pathStartEndPoints.Count;
            NativeArray<NativeList<int2>> results = new(numPaths, Allocator.TempJob);
            NativeArray<JobHandle> jobHandleArray = new(numPaths, Allocator.TempJob);

            //Create all the jobs
            for (int i = 0; i < numPaths; i++)
            {
                results[i] = new(Allocator.TempJob);
                (Vector3Int start, Vector3Int end) = pathStartEndPoints[i];
                Astar astarJob = instance.GetAstarJob(start.ToInt2(), end.ToInt2(), results[i]);
                jobHandleArray[i] = astarJob.Schedule();
            }

            //Run the jobs
            JobHandle.CompleteAll(jobHandleArray);
            jobHandleArray.Dispose();

            //Turn all the results into paths
            Vector3 offset = CellToWorld(Vector3Int.zero);
            List<Path> paths = new();
            for (int i = 0; i < numPaths; i++)
                paths.Add(new Path(results[i], offset));

            results.Dispose();
            return paths;
        }

        /// <summary> Returns a path that goes through each point in the list </summary>
        public static Path FindPathAlongRoute(List<Vector3Int> route)
        {
            List<(Vector3Int, Vector3Int)> startEnds = new();
            for (int i = 0; i < route.Count - 1; i++)
                startEnds.Add((route[i], route[i+1]));

            List<Path> segments = FindMultiplePaths(startEnds);
            Path path = segments[0];
            for (int i = 1; i < segments.Count; i++)
            {
                if (segments.Count == 0)
                    return Path.EmptyPath;
                path.AppendPath(segments[i]);
            }

            return path;
        }

        /// <summary> Returns a frontier, aka a list containing all reachable positions </summary>
        /// <param name="maxDistance"> For reference, the base distance between adjecent tiles is 10 (or 14 for diagonal)" </param>
        public static List<Vector3Int> FindAllReachableTiles(Vector3Int start, int maxDistance)
        {
            NativeList<int2> result = new(Allocator.TempJob);
            BreadthFirstSearch frontierJob = instance.GetFrontierJob(start.ToInt2(), maxDistance, result);
            frontierJob.Schedule().Complete();

            List<Vector3Int> frontier = new(result.Length);
            foreach (int2 v in result)
                frontier.Add(v.ToVec3Int());

            result.Dispose();
            return frontier;
        }
        #endregion FindPath

        /// <summary> Turns the tilemap into a grid that can be used for the pathfinding algorithm </summary>
        protected virtual void ConstructGrid()
        {
            map.CompressBounds();
            var bounds = map.cellBounds;
            gridSize = new(bounds.size.x, bounds.size.y);
            gridOffset = new(bounds.xMin, bounds.yMin);

            //Fill in the grid
            grid = new(gridSize.x * gridSize.y, Allocator.Persistent);
            for (int x = bounds.xMin, i = 0; x < bounds.xMax; x++, i++)
            {
                for (int y = bounds.yMin, j = 0; y < bounds.yMax; y++, j++)
                {
                    int index = i + (j * gridSize.x);
                    AstarTile tile = new(new int2(x,y), index, (int)tileType);
                    tile.Cost = GetTileCost(tile.ToVec3Int);
                    grid[index] = tile;
                }
            }
        }

        /// <summary> 
        /// Takes a cell's position in the tilemap and returns a cost of that tile 
        /// The higher a cell's cost, the more a path will try to avoid that tile
        /// </summary>
        /// <returns> Assigns a cost to a tile. A tile is unwalkable if its score is -1. </returns>
        public abstract int GetTileCost(Vector3Int cell);

        /// <summary> Update a tile's cost without having to reconstruct the entire grid </summary>
        public static void UpdateTileCost(Vector3Int cell, int cost)
        {
            cell -= instance.gridOffset.ToVec3Int();
            int i = cell.x + (cell.y * instance.gridSize.x);
            AstarTile tile = instance.grid[i];
            tile.Cost = cost;
            instance.grid[i] = tile;
        }

        protected override void Awake()
        {
            base.Awake();
            ConstructGrid();
        }

        protected override void OnDestroy()
        {
            if (grid.IsCreated)
                grid.Dispose();
            base.OnDestroy();
        }

        public enum TileType
        {
            Square = 4,
            SquareDiagonal = 8,
            Hex = 6
        }
    }
}