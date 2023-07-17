using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Collections;
using Winkeldief.Utilities;
using Unity.Mathematics;
using Unity.Jobs;

namespace Winkeldief.Pathfinding
{
    public class Pathfinder : Singleton<Pathfinder>
    {
        [SerializeField] Tilemap map;
        Astar astar;

        [Header("Pathfinding Config")]
        [Tooltip("Whether to allow diagonal movement in non-hex grids")] 
        public bool AllowDiagonals;
        private bool IsHexGrid;

        public static Vector3Int WorldToCell(Vector3 world) => instance.map.WorldToCell(world);
        public static Vector3 CellToWorld(Vector3Int cell) => instance.map.GetCellCenterWorld(cell);

        /// <summary>Returns a path with each node from start to end </summary>
        public static Path FindPath(Vector3Int start, Vector3Int end)
        {
            float startTime = Time.realtimeSinceStartup;
            int findPathJobCount = 10;
            NativeArray<JobHandle> jobHandleArray = new NativeArray<JobHandle>(findPathJobCount, Allocator.TempJob);

            for (int i = 0; i < findPathJobCount; i++)
            {
                instance.ConstructGrid();
                AstarJob findPathJob = new AstarJob(instance.astar, new int2(43, -6), new int2(-4, 3));
                jobHandleArray[i] = findPathJob.Schedule();
            }
            JobHandle.CompleteAll(jobHandleArray);
            jobHandleArray.Dispose();

            Debug.Log("Time: " + ((Time.realtimeSinceStartup - startTime) * 1000f));
            return new Path(null, Vector3.zero);//new Path(instance.astar.FindPath(new(start.x, start.y), new(end.x, end.y)), CellToWorld(Vector3Int.zero));
        }

        //Turns the tilemap into a grid that can be used for the astar algorithm
        protected virtual void ConstructGrid()
        { //Extension: Combine multiple tilemaps
            map.CompressBounds();
            var bounds = map.cellBounds;
            int tileType = IsHexGrid ? 6 : AllowDiagonals ? 8 : 4 ;

            NativeArray<AstarTile> tiles = new(bounds.size.x * bounds.size.y, Allocator.Persistent);
            for (int x = bounds.xMin, i = 0; x < bounds.xMax; x++, i++)
            {
                for (int y = bounds.yMin, j = 0; y < bounds.yMax; y++, j++)
                {
                    int index = i + (j * bounds.size.x);
                    AstarTile tile = new(x, y, index, tileType);
                    tile.Cost = GetTileCost(tile.ToVec3Int);
                    tiles[index] = tile;
                }
            }

            astar = new Astar(tiles, new(bounds.size.x, bounds.size.y), new(bounds.xMin, bounds.yMin));
        }

        private int GetTileCost(Vector3Int cell)
        { //Extension: Allow different types of tiles to have different costs
            return map.HasTile(cell) ? -1 : 1;
        }

        private void OnValidate()
        {
            IsHexGrid = map.cellLayout == GridLayout.CellLayout.Hexagon;
        }

        protected override void Awake()
        {
            base.Awake();
            IsHexGrid = map.cellLayout == GridLayout.CellLayout.Hexagon;
            ConstructGrid();
        }

        protected override void OnDestroy()
        {
            astar.Dispose();
            base.OnDestroy();
        }
    }
}
