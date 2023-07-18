using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
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

    [ContextMenu("TimerTest")]
    private void TimerTest()
    {
        //float startTime = Time.realtimeSinceStartup;
        //int findPathJobCount = 10;
        //NativeArray<JobHandle> jobHandleArray = new NativeArray<JobHandle>(findPathJobCount, Allocator.TempJob);

        //for (int i = 0; i < findPathJobCount; i++)
        //{
            //AstarJob astarJob = new AstarJob(new(-20, 56), new(45, -95));
            //jobHandleArray[i] = astarJob.Schedule();
            Pathfinder.FindPath(new(-4, 3), new(43, -6));
            //PathfinderUtility.CalculateSquareDistance(-19, 82, 16, -59, false);
        //}

        //JobHandle.CompleteAll(jobHandleArray);
        //jobHandleArray.Dispose();

        //Debug.Log("Time: " + ((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
    }

    private void Update()
    {
        if (true || Input.GetKeyDown(KeyCode.T))
            TimerTest();
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
