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

    public Connection ConnectEndToPath;
    public Connection ConnectStartToPath;

    public TrackSegment[] trackSegments;

    [System.Serializable]
    public struct Connection
    {
        public PathGenerator connector;
        public bool connectToEnd;
        public bool connectToStart;
        public Direction direction;
    }

    public enum Direction
    {
        Left,
        Right,
        Middle
    }

    public void GeneratePath()
    {
        path = new Path(transform.position);
    }

    public void Reset()
    {
        GeneratePath();
    }

    public TrackSegment GetClosestSegmentEnd(Vector2 origin)
    {
        float minDistToSegment = 0;
        TrackSegment newSelectedSegment = null;

        for (int i = 0; i < trackSegments.Length; i++)
        {
            float dist = Vector2.Distance(origin, trackSegments[i].points[trackSegments[i].points.Length -1].position); //Searching for segment ends, so searching for point 2 in segment
            if (newSelectedSegment == null)
            {
                minDistToSegment = dist;
                newSelectedSegment = trackSegments[i];
            }
            else if (dist < minDistToSegment)
            {
                minDistToSegment = dist;
                newSelectedSegment = trackSegments[i];
            }
        }

        return newSelectedSegment;
    }

    public TrackSegment GetClosestSegmentStart(Vector2 origin)
    {
        float minDistToSegment = 0;
        TrackSegment newSelectedSegment = null;

        for (int i = 0; i < trackSegments.Length; i++)
        {
            float dist = Vector2.Distance(origin, path.GetPointsInSegment(i)[0]); //Searching for segment ends, so searching for point 0 in segment
            if (newSelectedSegment == null)
            {
                minDistToSegment = dist;
                newSelectedSegment = trackSegments[i];
            }
            else if (dist < minDistToSegment)
            {
                minDistToSegment = dist;
                newSelectedSegment = trackSegments[i];
            }
        }

        return newSelectedSegment;
    }

}
