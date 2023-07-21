using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Winkeldief.Pathfinding
{
    [Serializable]
    public class Path
    {
        /// <summary> The list of all points forming this path </summary>
        List<Vector3Int> path;
        Vector3 worldOffset;

        /// <summary> The starting point of the path </summary>
        public Vector3Int Start => path[0];
        /// <summary> The ending point of the path </summary>
        public Vector3Int End => path[^1];

        /// <summary> The amount of points on this path </summary>
        public int Count => path.Count;
        /// <summary> The total length (Sum of distance between all nodes) of the path </summary>
        public float Length => path.Skip(1).Select((point, i) => Vector3Int.Distance(point, path[i])).Sum();

        public Path(List<Vector3Int> path, Vector3 offset)
        {
            this.path = path is not null ? path : new();
            worldOffset = offset;
        }

        /// <summary> Imports a NativeList<int2> into a path. Automatically reverses and disposes the NativeList </summary>
        public Path(NativeList<int2> path, Vector3 offset, bool disposeList = true)
        {
            this.path = new(path.Length);
            if (path.IsCreated)
            {
                for (int i = path.Length - 1; i >= 0; i--)
                    this.path.Add(new(path[i].x, path[i].y));

                if (disposeList)
                    path.Dispose();
            }
            worldOffset = offset;
        }

        /// <summary> Get Vector3Int at this index. Index is clamped between first and last entry on the path </summary>
        public Vector3Int GetAtIndex(int index)
        {
            return path[Mathf.Clamp(index, 0, path.Count - 1)];
        }

        /// <summary> Remove Vector3Int at this index </summary>
        public void RemoveIndex(int index)
        {
            if (index < path.Count)
                path.RemoveAt(index);
        }

        /// <summary> Appends a point to the end of the path </summary>
        public void Append(Vector3Int toAppend)
        {
            path.Add(toAppend);
        }

        /// <summary> Combines two paths together by appending a path to this path </summary>
        public void AppendPath(Path toAppend)
        {
            path.Concat(toAppend.path);
        }

        /// <summary>
        /// Simplifies the path without changing its shape by removing all points except corners.
        /// E.G. [(0,0), (0,1), (0,2), (1,2)] into [(0,0), (0,2), (1,2)].
        /// </summary>
        public void CompressPath()
        {
            if (path.Count < 2)
                return;

            List<Vector3Int> compressedPath = new();
            Vector3 lastMove = Vector3.zero;

            //Form new path
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 v = Pathfinder.CellToWorld(path[i + 1]) - Pathfinder.CellToWorld(path[i]);
                if (lastMove != v)
                    compressedPath.Add(path[i]);
                lastMove = v;
            }
            compressedPath.Add(path[^1]);

            path = compressedPath;
        }

        /// <summary> Offsets the whole path by the specified amount </summary>
        public void OffsetPath(Vector3Int offset)
        {
            for (int i = 0; i <= path.Count - 1; i++)
                path[i] += offset;
        }

        /// <summary> Converts the entire path into world coordinates </summary>
        /// <param name="offset"> Optional additional offset </param>
        public List<Vector3> ToWorld(Vector3 offset)
        {
            offset += worldOffset;
            return path.Select(v => Pathfinder.CellToWorld(v)).ToList();
        }

        /// <summary> Returns an empty path with no nodes and no offset </summary>
        public static Path EmptyPath => new Path(new(), Vector3Int.zero);
    }
}