using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Winkeldief.Pathfinding
{
    [Serializable]
    public struct Path
    {
        /// <summary> The list of all points forming this path </summary>
        List<Vector3Int> path;
        Vector3 worldOffset;

        /// <summary> The length of the path </summary>
        public int Length => path.Count;

        /// <summary> The starting point of the path </summary>
        public Vector3Int Start => path[0];
        /// <summary> The ending point of the path </summary>
        public Vector3Int End => path[^1];

        public Path(List<Vector3Int> path, Vector3 offset)
        {
            this.path = path is not null ? path : new();
            worldOffset = offset;
        }

        /// <summary> Get Vector3Int at this index. Index is clamped between first and last entry on the path </summary>
        public Vector3Int GetAtIndex(int index)
        {
            return path[Mathf.Clamp(index, 0, path.Count)];
        }

        /// <summary> Remove Vector3Int at this index </summary>
        public void RemoveIndex(int index)
        {
            if (index < path.Count)
                path.RemoveAt(index);
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
            Vector3Int lastMove = Vector3Int.zero;

            //Form new path
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3Int v = path[i + 1] - path[i];
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
            return path.Select(v => (Vector3)v + offset).ToList();
        }
    }
}