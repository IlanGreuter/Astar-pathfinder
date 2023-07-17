using System;
using System.Collections.Generic;
using UnityEngine;

namespace Winkeldief.Pathfinding
{
    [Serializable]
    public class AstarTile
    {
        public static bool IsHexGrid, AllowDiagonals;
        
        public int X, Y;
        public int Cost; //This node's cost. -1 means unwalkable

        public int G; // Cost from this tile to start node
        public int H; // Cost from this tile to end node
        public int F => G + H;

        public AstarTile previous;

        public AstarTile(int x, int y, int cost)
        {
            X = x;
            Y = y;
            Cost = cost;
        }

        /// <summary> Calculates the G and H costs </summary>
        public void CalculateCost(AstarTile end)
        {
            //Extension: Add tile's cost as well
            previous = null;
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
        public IEnumerable<Vector3Int> GetNeighbours(Vector3Int offset)
        {
            foreach (Vector3Int n in IsHexGrid ?
                PathfinderUtility.GetHexNeighbours(new(X, Y)) :
                PathfinderUtility.GetSquareNeighbours(new(X,Y), AllowDiagonals))
                yield return n + offset;
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

        /// <summary> Returns a Vector3Int with this tile's coordinates in the tilemap </summary>
        public Vector3Int ToVec3Int => new(X, Y);
    }
}