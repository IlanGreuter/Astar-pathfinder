using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Winkeldief.Utilities;

namespace Winkeldief.Pathfinding
{
    public class Pathfinder : Singleton<Pathfinder>
    {
        [SerializeField] Tilemap map;
        Astar astar;

        [Header("Pathfinding Config")]
        [Tooltip("Whether to allow diagonal movement in non-hex grids")] public bool AllowDiagonals;

        public static Vector3Int WorldToCell(Vector3 world) => instance.map.WorldToCell(world);
        public static Vector3 CellToWorld(Vector3Int cell) => instance.map.CellToWorld(cell) + instance.map.tileAnchor;

        private void OnValidate()
        {
           AstarTile.AllowDiagonals = AllowDiagonals;
        }

        protected override void Awake()
        {
            base.Awake();
            AstarTile.IsHexGrid = map.cellLayout == GridLayout.CellLayout.Hexagon;
            AstarTile.AllowDiagonals = AllowDiagonals;
            ConstructGrid();
        }

        /// <summary>Returns a path with each node from start to end </summary>
        public static Path FindPath(Vector3Int start, Vector3Int end)
        {
            return new Path(instance.astar.FindPath(start, end), CellToWorld(Vector3Int.zero));
        }

        //Turns the tilemap into a grid that can be used for the astar algorithm
        private void ConstructGrid()
        { //Extension: Combine multiple tilemaps
            map.CompressBounds();
            var bounds = map.cellBounds;
            
            AstarTile[,] tiles = new AstarTile[bounds.size.x, bounds.size.y];
            for (int x = bounds.xMin, i = 0; x < bounds.xMax; x++, i++)
            {
                for (int y = bounds.yMin, j = 0; y < bounds.yMax; y++, j++)
                {
                    AstarTile tile = new(x, y, 0);
                    tile.Cost = GetTileCost(tile.ToVec3Int);
                    tiles[i, j] = tile;
                }
            }

            astar = new Astar(tiles, new Vector3Int(bounds.xMin, bounds.yMin));
        }

        private int GetTileCost(Vector3Int cell)
        { //Extension: Allow different types of tiles to have different costs
            return map.HasTile(cell) ? -1 : 1;
        }
    }
}
