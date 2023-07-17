using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Winkeldief.Pathfinding
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct AstarTile
    {
        public int X, Y;
        public int Cost; //This node's cost. -1 means unwalkable

        public int F => G + H;
        public int G; // Cost from this tile to start node
        public int H; // Cost from this tile to end node

        public int Index, Previous;
        [ReadOnly] readonly int _tileType;

        public AstarTile(int x, int y, int index, int tiletype)
        {
            X = x;
            Y = y;
            Index = index;
            _tileType = tiletype;

            G = int.MaxValue;
            H = 0;
            Cost = 1;
            Previous = -1;
        }

        /// <summary> Calculates the G and H costs </summary>
        public void CalculateCost(AstarTile end)
        {
            //Extension: Add tile's cost as well
            Previous = -1;
            H = GetDistanceTo(end.X, end.Y);
        }

        /// <summary> Gets the distance from this tile to the coordinates </summary>
        public int GetDistanceTo(int x, int y)
        {
            return _tileType == 6 ?
                PathfinderUtility.CalculateHexDistance(X, Y, x, y) :
                PathfinderUtility.CalculateSquareDistance(X, Y, x, y, _tileType == 8);
        }

        /// <summary> Returns the tile that should be evaluated first </summary>
        public AstarTile Compare(AstarTile other)
        {
            if (F != other.F)
                return F < other.F ? this : other;
            else
                return H <= other.H ? this : other;
        }

        /// <summary>
        /// Returns an int representing where each bit represents if the neighbour is actually adjecent to a tile
        /// </summary>
        /// <param name="yPosition"> Only used for hex grids. 
        /// Neighbours coordinates may differ if the y position of the tile is even or odd.
        /// </param>
        public int GetNeighbourFlags(int yPosition)
        {
            if (_tileType == 6)
                return (yPosition & 1) == 0 ? 95 : 175; // 0101 1111 : 1010 1111
            else
                return (_tileType == 8) ? 255 : 15; // 1111 1111 : 0000 1111
        }

        /// <summary> Returns a Vector3Int with this tile's coordinates in the tilemap </summary>
        public Vector3Int ToVec3Int => new(X, Y);
        /// <summary> Returns a Int2 with this tile's coordinates in the tilemap </summary>
        public int2 ToInt2 => new(X, Y);
    }
}