using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Winkeldief.Pathfinding
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct AstarTile : IHeapItem<AstarTile>
    {
        public readonly int X, Y;
        public int Cost; //This node's cost. -1 means unwalkable

        public int F => G + H;
        public int G; // Cost from this tile to start node
        public int H; // Cost from this tile to end node

        public readonly int Index, TileType;
        public int Previous;
        public int HeapIndex { get; set; }

        public AstarTile(int x, int y, int index, int tiletype)
        {
            X = x;
            Y = y;
            Index = index;
            TileType = tiletype;

            G = int.MaxValue;
            H = 0;
            Cost = 1;
            Previous = -1;
            HeapIndex = 0;
        }

        /// <summary> Calculates the G and H costs </summary>
        [BurstCompile]
        public void CalculateCost(AstarTile end)
        {
            //Extension: Add tile's cost as well
            Previous = -1;
            H = GetDistanceTo(end.X, end.Y);
        }

        /// <summary> Gets the distance from this tile to the coordinates </summary>
        [BurstCompile]
        public int GetDistanceTo(int x, int y)
        {
            return TileType == 6 ?
                PathfinderUtility.CalculateHexDistance(X, Y, x, y) :
                PathfinderUtility.CalculateSquareDistance(X, Y, x, y, TileType == 8);
        }

        /// <summary> Returns the tile that should be evaluated first </summary>
        [BurstCompile]
        public int CompareTo(AstarTile other)
        {
            int compare = F.CompareTo(other.F);
            return -(compare != 0 ? compare : H.CompareTo(other.H));
        }

        /// <summary> Checks if the indexes of the two match </summary>
        [BurstCompile]
        public bool Equals(AstarTile other)
        {
            return Index == other.Index;
        }

        /// <summary>
        /// Returns an int representing where each bit represents if the neighbour is actually adjecent to a tile
        /// </summary>
        /// <param name="yPosition"> Only used for hex grids. 
        /// Neighbours coordinates may differ if the y position of the tile is even or odd.
        /// </param>
        [BurstCompile]
        public int GetNeighbourFlags()
        {
            if (TileType == 6)
                return (Y & 1) == 0 ? 95 : 175; // 0101 1111 : 1010 1111
            else
                return (TileType == 8) ? 255 : 15; // 1111 1111 : 0000 1111
        }

        /// <summary> Returns a Vector3Int with this tile's coordinates in the tilemap </summary>
        public Vector3Int ToVec3Int => new(X, Y);
        /// <summary> Returns a Int2 with this tile's coordinates in the tilemap </summary>
        public int2 ToInt2 => new(X, Y);
    }
}