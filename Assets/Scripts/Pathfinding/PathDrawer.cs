using UnityEngine;

namespace Winkeldief.Pathfinding
{
    [RequireComponent(typeof(LineRenderer))]
    public class PathDrawer : MonoBehaviour
    {
        LineRenderer line;

        private void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.loop = false;
        }

        /// <summary> Set the path to be drawn </summary>
        public void SetPath(Path path)
        {
            path.CompressPath();
            line.positionCount = path.Length;
            line.SetPositions(path.ToWorld(Vector3.zero).ToArray());
        }
    }
}