using System.Collections.Generic;
using UnityEngine;

namespace Winkeldief.Pathfinding
{
    internal static class PathfinderUtility
    {
        public const int DirectMoveCost = 10, DiagonalMoveCost = 14;

        #region CalculateDistance
        public static int CalculateSquareDistance(int x1, int y1, int x2, int y2, bool allowDiagonals)
        {
            int dx = Mathf.Abs(x1 - x2);
            int dy = Mathf.Abs(y1 - y2);

            if (allowDiagonals)
                return Mathf.Min(dx, dy) * DiagonalMoveCost + Mathf.Abs(dx - dy) * DirectMoveCost;
            else
                return (dx + dy) * DirectMoveCost;
        }

        public static int CalculateHexDistance(int x1, int y1, int x2, int y2)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            int adx = Mathf.Abs(dx);
            int ady = Mathf.Abs(dy);
            
            if ((dx < 0) ^ ((y1 & 1) == 1))
                adx = Mathf.Max(0, adx - (ady + 1) / 2);
            else
                adx = Mathf.Max(0, adx - (ady) / 2);
            return (adx + ady) * DirectMoveCost;
        }
        #endregion CalculateDistance

        #region GetNeighbours
        public static IEnumerable<Vector3Int> GetSquareNeighbours(Vector3Int offset, bool includeDiagonals)
        {
            yield return offset + new Vector3Int(-1, 0);
            yield return offset + new Vector3Int(1, 0);
            yield return offset + new Vector3Int(0, 1);
            yield return offset + new Vector3Int(0, -1);

            if (includeDiagonals)
            {
                yield return offset + new Vector3Int(-1, 1);
                yield return offset + new Vector3Int(1, 1);
                yield return offset + new Vector3Int(-1, -1);
                yield return offset + new Vector3Int(1, -1); 
            }
        }

        public static IEnumerable<Vector3Int> GetHexNeighbours(Vector3Int offset)
        {
            yield return offset + new Vector3Int(-1, 0);
            yield return offset + new Vector3Int(1, 0); 
            yield return offset + new Vector3Int(0, 1); 
            yield return offset + new Vector3Int(0, -1);

            bool isEven = (offset.y & 1) == 0;
            yield return offset + (new Vector3Int(1, 1) * (isEven ? -1 : 1));
            yield return offset + (new Vector3Int(1, -1) * (isEven ? -1 : 1));
        }
        #endregion GetNeighbours
    }
}