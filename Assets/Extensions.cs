using UnityEngine;

public static class Extensions
{
    public static void Add(this Vector3 vec, Vector2 toAdd)
    {
        vec.x += toAdd.x;
        vec.y += toAdd.y;
    }

    public static void Add(this Vector2 vec, Vector3 toAdd)
    {
        vec.x += toAdd.x;
        vec.y += toAdd.y;
    }

    public static float Abs(this float a) => Mathf.Abs(a);
    public static int Abs(this int a) => Mathf.Abs(a);
}
