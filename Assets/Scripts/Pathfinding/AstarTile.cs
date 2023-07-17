using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Winkeldief.Pathfinding
{
    [Serializable]
    public struct AstarTile
    {
        public static bool IsHexGrid, AllowDiagonals;
        
        public int X, Y;
        public int Cost; //This node's cost. -1 means unwalkable

        public int G; // Cost from this tile to start node
        public int H; // Cost from this tile to end node
        public int F => G + H;

        public int Index, Previous;

        public AstarTile(int x, int y, int index)
        {
            X = x;
            Y = y;
            Index = index;

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
            return IsHexGrid ? 
                PathfinderUtility.CalculateHexDistance(X, Y, x, y) :
                PathfinderUtility.CalculateSquareDistance(X, Y, x, y, AllowDiagonals);
        }

        /// <summary> Returns a list with all of the neighbours </summary>
        public IEnumerable<int2> GetNeighbours(int2 offset)
        {
            foreach (int2 n in IsHexGrid ?
                PathfinderUtility.GetHexNeighbours(new(X, Y)) :
                PathfinderUtility.GetSquareNeighbours(new(X,Y), AllowDiagonals))
                yield return n + offset;
        }

        /// <summary> Returns a list with all of the neighbours </summary>
        public NativeArray<int2> GetNeighbourArray(int2 offset)
        {
            return (IsHexGrid) ? PathfinderUtility.GetHexNeighboursArray(new(X,Y)) :
                PathfinderUtility.GetSquareNeighboursArray(new(X + offset.x,Y + offset.y), AllowDiagonals) ;
        }

        /// <summary> Equals if the X and Y components are the same </summary>
        public bool Equals(AstarTile tile)
        {
            return tile.X == X && tile.Y == Y;
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
        public static int GetNeighbourFlags(int yPosition)
        {
            if (IsHexGrid)
                return (yPosition & 1) == 0 ? 95 : 175; // 0101 1111 : 1010 1111
            else
                return AllowDiagonals ? 255 : 15; // 1111 1111 : 0000 1111
        }

        /// <summary> Returns a Vector3Int with this tile's coordinates in the tilemap </summary>
        public Vector3Int ToVec3Int => new(X, Y);
        /// <summary> Returns a Int2 with this tile's coordinates in the tilemap </summary>
        public int2 ToInt2 => new(X, Y);
    }
}