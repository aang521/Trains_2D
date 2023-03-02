using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path
{
    [SerializeField, HideInInspector]
    List<Vector2> points;

    private bool isClosedPath;

    public Path(Vector2 centre)
    {
        points = new List<Vector2>
        {
            centre + Vector2.left,
            centre + (Vector2.left + Vector2.up) * .5f,
            centre + (Vector2.right + Vector2.down) * .5f,
            centre + Vector2.right
        };
    }

    public Vector2 this[int i]
    {
        get
        {
            return points[i];
        }
    }

    public bool IsClosed
    {
        get
        {
            return isClosedPath;
        }
        set
        {
            if (isClosedPath != value)
            {
                isClosedPath = value;
                if (isClosedPath)
                {
                    points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
                    points.Add(points[0] * 2 - points[1]);
                }
                else
                {
                    points.RemoveRange(points.Count - 2, 2);
                }
            }
        }
    }

    public int NumberOfPoints
    {
        get
        {
            return points.Count;
        }
    }

    public int NumberOfSegments
    {
        get
        {
            return points.Count / 3;
        }
    }

    public void AddSegment(Vector2 anchorPos)
    {
        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        points.Add((points[points.Count - 1] + anchorPos) * .5f);
        points.Add(anchorPos);
    }

    public void SplitSegment(Vector2 anchorPoint, int segmentIndex)
    {
        points.InsertRange(segmentIndex * 3 + 2, new Vector2[] { Vector2.zero, anchorPoint, Vector2.zero });

        AutoSetAnchorControlPoints(segmentIndex * 3 + 3);
    }

    public void DeleteSegment(int anchorIndex)
    {
        if (NumberOfSegments > 2 || !isClosedPath && NumberOfSegments > 1)
        {
            if (anchorIndex == 0)
            {
                if (isClosedPath)
                {
                    points[points.Count - 1] = points[2];
                }
                points.RemoveRange(0, 3);
            }
            else if (anchorIndex == points.Count - 1 && !isClosedPath)
            {
                points.RemoveRange(anchorIndex - 2, 3);
            }
            else
            {
                points.RemoveRange(anchorIndex - 1, 3);
            }
        }
    }

    public Vector2[] GetPointsInSegment(int i)
    {
        return new Vector2[] { points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[LoopIndex(i * 3 + 3)] };
    }

    public void MovePoint(int i, Vector2 pos)
    {
        Vector2 deltaMove = pos - points[i];
        points[i] = pos;

        if (i % 3 == 0)
        {

            if (i + 1 < points.Count || isClosedPath)
                points[LoopIndex(i + 1)] += deltaMove;
            if (i - 1 >= 0 || isClosedPath)
                points[LoopIndex(i - 1)] += deltaMove;
        }
        else
        {
            bool nextPointIsAnchor = (i + 1) % 3 == 0;
            int correspondingControlIndex = (nextPointIsAnchor) ? i + 2 : i - 2;
            int anchorIndex = (nextPointIsAnchor) ? i + 1 : i - 1;

            if (correspondingControlIndex >= 0 && correspondingControlIndex < points.Count || isClosedPath)
            {
                float dist = (points[LoopIndex(anchorIndex)] - points[LoopIndex(correspondingControlIndex)]).magnitude;
                Vector2 dir = (points[LoopIndex(anchorIndex)] - pos).normalized;
                points[LoopIndex(correspondingControlIndex)] = points[LoopIndex(anchorIndex)] + dir * dist;
            }
        }
    }
    /*
    public void ToggleClosed()
    {
        isClosedPath = !isClosedPath;

        if (isClosedPath)
        {
            points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
            points.Add(points[0] * 2 - points[1]);
        }
        else
        {
            points.RemoveRange(points.Count - 2, 2);
        }
    }
    */
    private int LoopIndex(int i)
    {
        return (i + points.Count) % points.Count;
    }

    public Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        List<Vector2> evenlySpacedPoints = new List<Vector2>();
        evenlySpacedPoints.Add(points[0]);
        Vector2 previousPoint = points[0];
        float distTraveledSinceEvenPoint = 0;

        for (int i = 0; i < NumberOfSegments; i++)
        {
            Vector2[] p = GetPointsInSegment(i);
            float controlNetLength = (Vector2.Distance(p[0], p[1]) + Vector2.Distance(p[1], p[2]) + Vector2.Distance(p[2], p[3]));
            float estimatedBezierLenght = Vector2.Distance(p[0], p[3]) + controlNetLength / 2;
            int divisions = Mathf.CeilToInt(estimatedBezierLenght * resolution * 10);
            float t = 0;
            while (t <= 1)
            {
                t += 1f / divisions;
                Vector2 pointOnCurve = Bezier.EveluateQubic(p[0], p[1], p[2], p[3], t);
                distTraveledSinceEvenPoint += Vector2.Distance(previousPoint, pointOnCurve);

                while (distTraveledSinceEvenPoint >= spacing)
                {
                    float overshotDist = distTraveledSinceEvenPoint - spacing;
                    Vector2 newEvenlySpacedPont = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshotDist;
                    evenlySpacedPoints.Add(newEvenlySpacedPont);
                    distTraveledSinceEvenPoint = overshotDist;
                    previousPoint = newEvenlySpacedPont;
                }

                previousPoint = pointOnCurve;
            }
        }

        return evenlySpacedPoints.ToArray();
    }

    public struct PointData
	{
        public Vector2[] points;
        public float[] t;
    }
    public PointData CalculateEvenlySpacedSegmentPoints(int index, float spacing, float resolution = 1) {
        List<Vector2> evenlySpacedPoints = new List<Vector2>();
        List<float> ts = new List<float>();
        
        Vector2[] p = GetPointsInSegment(index);
        evenlySpacedPoints.Add(p[0]);
        ts.Add(0);
        Vector2 previousPoint = p[0];
        float distTraveledSinceEvenPoint = 0;

        float controlNetLength = (Vector2.Distance(p[0], p[1]) + Vector2.Distance(p[1], p[2]) + Vector2.Distance(p[2], p[3]));
        float estimatedBezierLenght = Vector2.Distance(p[0], p[3]) + controlNetLength / 2;
        int divisions = Mathf.CeilToInt(estimatedBezierLenght * resolution * 10);
        float t = 0;
        while (t <= 1)
        {
            t += 1f / divisions;
            Vector2 pointOnCurve = Bezier.EveluateQubic(p[0], p[1], p[2], p[3], t);
            distTraveledSinceEvenPoint += Vector2.Distance(previousPoint, pointOnCurve);

            while (distTraveledSinceEvenPoint >= spacing)
            {
                float overshotDist = distTraveledSinceEvenPoint - spacing;
                Vector2 newEvenlySpacedPont = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshotDist;
                evenlySpacedPoints.Add(newEvenlySpacedPont);
                ts.Add(t);
                distTraveledSinceEvenPoint = overshotDist;
                previousPoint = newEvenlySpacedPont;
            }

            previousPoint = pointOnCurve;
        }

        return new PointData
        {
            points = evenlySpacedPoints.ToArray(),
            t = ts.ToArray(),
        };
    }

    private void AutoSetAnchorControlPoints(int anchorIndex)
    {
        Vector2 anchorPos = points[anchorIndex];
        Vector2 dir = Vector2.zero;
        float[] neighbourDistances = new float[2];

        if (anchorIndex - 3 >= 0 || isClosedPath)
        {
            Vector2 offset = points[LoopIndex(anchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neighbourDistances[0] = offset.magnitude;
        }
        if (anchorIndex + 3 >= 0 || isClosedPath)
        {
            Vector2 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neighbourDistances[1] = -offset.magnitude;
        }

        dir.Normalize();

        for (int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if (controlIndex >= 0 && controlIndex < points.Count || isClosedPath)
            {
                points[LoopIndex(controlIndex)] = anchorPos + dir * neighbourDistances[i] * .5f;
            }
        }
    }
}