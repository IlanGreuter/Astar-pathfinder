using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Winkeldief.Pathfinding.AStar
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct AstarTile
    {
        public readonly int2 Pos;
        public int Cost; //This node's cost. -1 means unwalkable

        public int F => G + H;
        public int G; // Cost from this tile to start node
        public int H; // Cost from this tile to end node

        public readonly int Index, TileType; //TileType should be 6 for hex, 4 for square and 8 for square with diagonal
        public int Previous;
        public int HeapIndex { get; set; }

        public AstarTile(int2 pos, int index, int tiletype)
        {
            Pos = pos;
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
        public void CalculateHCost(AstarTile end)
        {
            H = GetDistanceTo(end.Pos) + Cost;
        }

        /// <summary> Gets the distance from this tile to the coordinates </summary>
        [BurstCompile]
        public int GetDistanceTo(int2 target)
        {
            return TileType == 6 ?
                PathfinderUtility.CalculateHexCost(Pos.x, Pos.y, target.x, target.y) :
                PathfinderUtility.CalculateSquareCost(Pos.x, Pos.y, target.x, target.y, TileType == 8);
        }

        /// <summary> Returns the tile that should be evaluated first </summary>
        [BurstCompile]
        public int CompareTo(AstarTile other)
        {
            int compare = F.CompareTo(other.F);
            return -(compare != 0 ? compare : H.CompareTo(other.H));
        }

        /// <summary>
        /// Returns an int representing where each bit represents if the neighbour is actually adjecent to a tile
        /// Neighbours coordinates in hex grids may differ if the y position of the tile is even or odd.
        /// </summary>
        [BurstCompile]
        public int GetNeighbourFlags()
        {
            if (TileType == 6)
                return (Pos.y & 1) == 0 ? 95 : 175; // 0101 1111 : 1010 1111
            else
                return (TileType == 8) ? 255 : 15; // 1111 1111 : 0000 1111
        }

        /// <summary> Returns a Vector3Int with this tile's coordinates in the tilemap </summary>
        public Vector3Int ToVec3Int => new(Pos.x, Pos.y);
        /// <summary> Returns a Int2 with this tile's coordinates in the tilemap </summary>
        public int2 ToInt2 => Pos;
    }
}