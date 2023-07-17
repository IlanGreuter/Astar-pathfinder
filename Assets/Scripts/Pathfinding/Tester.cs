using UnityEngine;
using UnityEngine.Tilemaps;

namespace Winkeldief.Pathfinding
{
    public class Tester : MonoBehaviour
    {
        [SerializeField] Tilemap map;

        Camera cam;
        private void Awake()
        {
            cam = Camera.main;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Vector3Int h = HoveredTile();
                h.z = 0;
                float i = 1;
                var b = map.cellBounds;
                //foreach (PathfinderUtility.GetSquareNeighbours(h, false))
                //foreach (Vector3Int n in PathfinderUtility.GetHexNeighbours(h))
                for (int j = b.xMin; j <= b.xMax; j++)
                    for (int k = b.yMin; k <= b.yMax; k++)
                    {
                        int a = PathfinderUtility.CalculateHexDistance(h.x, h.y, j, k);
                        i = 1 - ((0.1f) * a);
                        
                        map.SetColor(new(j,k), Color.green * i);
                    }
            }
        }

        private Vector3Int HoveredTile()
        {
            return Pathfinder.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));
        }

        private void OnGUI()
        {
            var style = new GUIStyle() { fontSize = 32, normal = new GUIStyleState() { textColor = Color.white } };
            GUI.Label(new Rect(20, Screen.height - 60, 100, 100), HoveredTile().ToString(), style);
        }
    }
}
