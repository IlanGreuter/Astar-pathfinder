using UnityEngine;

namespace Winkeldief.Pathfinding
{
    [RequireComponent(typeof(LineRenderer))]
    public class PathDrawer : MonoBehaviour
    {
        LineRenderer line;
        [SerializeField, Tooltip("Simplifies the path by removing all poins that are not corners")] 
        bool compressPath = true;

        private void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.loop = false;
        }

        /// <summary> Set the path to be drawn </summary>
        public void SetPath(Path path)
        {
            if (compressPath) path.CompressPath();
            line.positionCount = path.Count;
            line.SetPositions(path.ToWorld(Vector3.zero).ToArray());
        }
    }
}