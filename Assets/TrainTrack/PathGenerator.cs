using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGenerator : MonoBehaviour
{
    [HideInInspector]
    public Path path;

    public Color AnchorColor = Color.red;
    public Color ControlColor = Color.blue;
    public Color SegmentColor = Color.green;
    public Color SelectedSegmentColor = Color.yellow;
    public float AnchorDia = 0.1f;
    public float controlDia = 0.075f;
    public bool displayControlPoints = true;

    public void GeneratePath()
    {
        path = new Path(transform.position);
    }

    public void Reset()
    {
        GeneratePath();
    }
}
