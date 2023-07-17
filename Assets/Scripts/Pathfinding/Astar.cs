using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Winkeldief.Pathfinding
{
    public class Astar
    {
        readonly AstarTile[,] tiles;
        Vector3Int gridOffset;

        public Astar(AstarTile[,] grid, Vector3Int offset)
        {
            tiles = grid;
            gridOffset = offset;
        }

        //Returns a list of positions that form a path from start to end.
        //Will return null if no path was found
        public List<Vector3Int> FindPath(Vector3Int start, Vector3Int end)
        {
            start -= gridOffset;
            end -= gridOffset;

            if (!IsPathValid(start, end))
                return null;

            AstarTile startTile = tiles[start.x, start.y];
            AstarTile endTile = tiles[end.x, end.y];
            startTile.CalculateCost(endTile);

            List<AstarTile> openTiles = new() { startTile };
            List<AstarTile> closedTiles = new();

            //As long as we have tiles to search
            while (openTiles.Count > 0)
            { 
                //Find best tile
                AstarTile currentTile = openTiles.Aggregate((min, next) => min.Compare(next)); //Extension: This is very slow

                //If tile is end, construct path
                if (currentTile == endTile)
                    return ConstructPath(endTile, true);

                //Remove current from open and add to closed tiles
                openTiles.Remove(currentTile);
                closedTiles.Add(currentTile);

                //Foreach neighbour
                foreach (Vector3Int neighbour in currentTile.GetNeighbours(-gridOffset))
                {
                    //Skip if not in grid
                    if (!IsValid(neighbour))
                        continue;

                    AstarTile neigh = tiles[neighbour.x, neighbour.y];

                    //Skip if already searched or if not walkable
                    if (neigh.Cost < 0 || closedTiles.Contains(neigh))
                        continue;

                    bool isOpen = openTiles.Contains(neigh);
                    int tempG = currentTile.G + currentTile.GetDistanceTo(neigh.X, neigh.Y);

                    //If not yet searched or better path was found
                    if (!isOpen || tempG < neigh.G)
                    {
                        neigh.G = tempG;
                        neigh.CalculateCost(endTile);
                        neigh.previous = currentTile;
                        
                        if (!isOpen)
                            openTiles.Add(neigh);
                    }
                }
            }
            return null;
        }

        /// <summary> Construct path once end has been found </summary>
        private List<Vector3Int> ConstructPath(AstarTile end, bool includeStart)
        {
            List<Vector3Int> path = new();
            AstarTile current = end;

            while (current.previous != null)
            {
                path.Add(current.ToVec3Int);
                current = current.previous;
            }

            if (includeStart) path.Add(current.ToVec3Int);
            path.Reverse();
            return path;
        }

        /// <summary> Check if path could exist </summary>
        private bool IsPathValid(Vector3Int start, Vector3Int end)
        {
            if (!IsValid(start) || !IsValid(end))
                return false;

            return tiles[end.x, end.y].Cost >= 0;
        }

        /// <summary> Check if tile is in the grid </summary>
        private bool IsValid(Vector3Int cell) =>
            cell.x >= 0 && cell.x < tiles.GetUpperBound(0)
                && cell.y >= 0 && cell.y < tiles.GetUpperBound(1);
    }
}
