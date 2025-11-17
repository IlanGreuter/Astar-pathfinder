using UnityEngine;

public class CameraMotor : MonoBehaviour
{
    Transform t;
    Camera cam;

    [Header("Scrolling")]
    [SerializeField] float speed = 24f;

    [Header("Zooming")]
    [SerializeField, Min(0.01f)] float targetZoom;
    [SerializeField] float zoomStep = 1f;
    [SerializeField, Min(0)] float zoomDuration = 0.5f;

    [SerializeField, Min(0.1f)] float minZoom = 1f, maxZoom = 100f;
    [SerializeField, Range(0, 1f)] float zoomMouseAtraction = 0.5f;

    float currentZoom, zoomSpeed;
    Vector2 zoomOffset;

    private void Awake()
    { 
        cam = Camera.main;
        t = cam.transform;

        targetZoom = cam.orthographicSize;
        currentZoom = targetZoom;
    }

    private void Update()
    {
        //TODO Rotate?
        //TODO Middle mouse drag
        //TODO screen edge scrolling

        Vector2 move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0)
        {
            //Zoom when mouse is scrolled
            targetZoom = Mathf.Clamp(targetZoom + (scrollDelta * -zoomStep), minZoom, maxZoom);

            zoomOffset = cam.ScreenToWorldPoint(Input.mousePosition) - t.position;
            zoomOffset *= zoomMouseAtraction * scrollDelta > 0 ? 0.5f : -0.1f;
        }

        //Move camera towards the mouse 
        if (Mathf.Abs(zoomSpeed) > 0.1f)
            move += Vector2.MoveTowards(Vector2.zero, zoomOffset, Mathf.Min(0.04f * zoomMouseAtraction * Mathf.Abs(zoomSpeed) * zoomOffset.magnitude, 2f));

        t.Translate((speed * Time.deltaTime) * move);
        UpdateCameraZoom();
    }
    
    public void SetZoom(float zoom, bool immediate = false)
    {
        targetZoom = zoom;
        if (immediate)
            currentZoom = zoom;
    }

    private void CalculateZoom()
    {
        currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomSpeed, zoomDuration * 0.5f);
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        if (Mathf.Abs(currentZoom - targetZoom) < 0.01f)
            currentZoom = targetZoom;
    }

    private void UpdateCameraZoom()
    {
        CalculateZoom();
        cam.orthographicSize = currentZoom;
    }
}
