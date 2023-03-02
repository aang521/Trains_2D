using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacePath : MonoBehaviour
{
    public float Spacing = 0.1f;
    public float Resolution = 1f;

    public void Start()
    {
        Vector2[] points = FindObjectOfType<PathGenerator>().path.CalculateEvenlySpacedPoints(Spacing, Resolution);
        foreach (Vector2 point in points)
        {
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.transform.position = point;
            indicator.transform.localScale = Vector3.one * Spacing * 0.5f;
        }
    }
}
