using UnityEngine;
using UnityEngine.Tilemaps;
using Winkeldief.Pathfinding;
public class Tester : MonoBehaviour
{
    [SerializeField] Tilemap map;
    bool a;
    Vector3Int c, last;
    [SerializeField] PathDrawer drawer;
    Camera cam;
    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            c = HoveredTile();
            a = true;
        }
        if (a)
        {
            var l = HoveredTile();
            if (l != last)
                drawer.SetPath(Pathfinder.FindPath(c, l));
            last = l;
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
