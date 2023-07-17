using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Winkeldief.Pathfinding
{
    internal static class PathfinderUtility
    {
        public const int DirectMoveCost = 10, DiagonalMoveCost = 14;

        #region CalculateDistance
        [BurstCompile]
        public static int CalculateSquareDistance(int x1, int y1, int x2, int y2, bool allowDiagonals)
        {
            int dx = Mathf.Abs(x1 - x2);
            int dy = Mathf.Abs(y1 - y2);

            if (allowDiagonals)
                return Mathf.Min(dx, dy) * DiagonalMoveCost + Mathf.Abs(dx - dy) * DirectMoveCost;
            else
                return (dx + dy) * DirectMoveCost;
        }

        [BurstCompile]
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
        public static IEnumerable<int2> GetSquareNeighbours(int2 offset, bool includeDiagonals)
        {
            yield return offset + new int2(-1, 0);
            yield return offset + new int2(1, 0);
            yield return offset + new int2(0, 1);
            yield return offset + new int2(0, -1);

            if (includeDiagonals)
            {
                yield return offset + new int2(-1, 1);
                yield return offset + new int2(1, 1);
                yield return offset + new int2(-1, -1);
                yield return offset + new int2(1, -1);
            }
        }

        [BurstCompile]
        public static NativeArray<int2> GetSquareNeighboursArray(int2 offset, bool allowDiagonals)
        {
            NativeArray<int2> neighbours = new(allowDiagonals ? 8 : 4, Allocator.Temp);
            neighbours[0] = offset + new int2(-1, 0);
            neighbours[1] = offset + new int2(1, 0);
            neighbours[2] = offset + new int2(0, 1);
            neighbours[3] = offset + new int2(0, -1);
            if (!allowDiagonals)
                return neighbours;

            neighbours[4] = offset + new int2(-1, 1);
            neighbours[5] = offset + new int2(1, 1);
            neighbours[6] = offset + new int2(-1, -1);
            neighbours[7] = offset + new int2(1, -1);
            return neighbours;
        }

        public static IEnumerable<int2> GetHexNeighbours(int2 offset)
        {
            yield return offset + new int2(-1, 0);
            yield return offset + new int2(1, 0);
            yield return offset + new int2(0, 1);
            yield return offset + new int2(0, -1);

            bool isEven = (offset.y & 1) == 0;
            yield return offset + (new int2(1, 1) * (isEven ? -1 : 1));
            yield return offset + (new int2(1, -1) * (isEven ? -1 : 1));
        }

        [BurstCompile]
        public static NativeArray<int2> GetHexNeighboursArray(int2 offset)
        {
            NativeArray<int2> neighbours = new(6, Allocator.Temp);
            neighbours[0] = offset + new int2(-1, 0);
            neighbours[1] = offset + new int2(1, 0);
            neighbours[2] = offset + new int2(0, 1);
            neighbours[3] = offset + new int2(0, -1);

            bool isEven = (offset.y & 1) == 0;
            neighbours[2] = offset + (new int2(1, 1) * (isEven ? -1 : 1));
            neighbours[3] = offset + (new int2(1, -1) * (isEven ? -1 : 1));
            return neighbours;
        }
        #endregion GetNeighbours
    }
}