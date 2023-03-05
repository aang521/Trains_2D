using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    public static TrackManager instance;

    public PathGenerator[] pathGenerators;

    public List<TrackSegment> segments = new List<TrackSegment>();

    public float trackHalfWidth = 1;
    public Material trackMaterial;

    public void Awake()
    {
        instance = this;

        SetupLevel();
    }

    public void SetupLevel()
    {
        for (int k = 0; k < pathGenerators.Length; k++)
        {
            if (!pathGenerators[k].path.IsClosed)
            {
                if (pathGenerators[k].ConnectEndToPath.connector != null)
                {
                    float smallestDist = float.MaxValue;
                    Vector2 newPos = Vector2.zero;
                    for (int i = 0; i < pathGenerators[k].ConnectEndToPath.connector.path.NumberOfSegments; i++)
                    {
                        var points = pathGenerators[k].ConnectEndToPath.connector.path.GetPointsInSegment(i);
                        float dist = (points[0] - pathGenerators[k].path.GetPointsInSegment(pathGenerators[k].path.NumberOfSegments - 1)[3]).sqrMagnitude;
                        float dist2 = (points[3] - pathGenerators[k].path.GetPointsInSegment(pathGenerators[k].path.NumberOfSegments - 1)[3]).sqrMagnitude;

                        if (dist < smallestDist)
                        {
                            smallestDist = dist;
                            newPos = points[0];
                        }
                        if (dist2 < smallestDist)
                        {
                            smallestDist = dist2;
                            newPos = points[3];
                        }
                    }
                    pathGenerators[k].path.MovePoint((pathGenerators[k].path.NumberOfSegments - 1)*3 + 3, newPos);
                }
                if (pathGenerators[k].ConnectStartToPath.connector != null)
                {
                    float smallestDist = float.MaxValue;
                    Vector2 newPos = Vector2.zero;
                    for (int i = 0; i < pathGenerators[k].ConnectStartToPath.connector.path.NumberOfSegments; i++)
                    {
                        var points = pathGenerators[k].ConnectStartToPath.connector.path.GetPointsInSegment(i);
                        float dist = (points[0] - pathGenerators[k].path.GetPointsInSegment(0)[0]).sqrMagnitude;
                        float dist2 = (points[3] - pathGenerators[k].path.GetPointsInSegment(0)[0]).sqrMagnitude;

                        if (dist < smallestDist)
                        {
                            smallestDist = dist;
                            newPos = points[0];
                        }
                        if (dist2 < smallestDist)
                        {
                            smallestDist = dist2;
                            newPos = points[3];
                        }
                    }
                    pathGenerators[k].path.MovePoint(0, newPos);
                }
            }
        }

        for(int i = 0; i < transform.childCount; i++)
		{
            DestroyImmediate(transform.GetChild(i).gameObject);
		}
        segments.Clear();
        GameObject track = new GameObject("Track");
        track.transform.parent = transform;
        for (int k = 0; k < pathGenerators.Length; k++)
        {
            pathGenerators[k].trackSegments = new TrackSegment[pathGenerators[k].path.NumberOfSegments];
            for (int i = 0; i < pathGenerators[k].path.NumberOfSegments; i++)
            {
                GameObject segment = new GameObject("segment");
                segment.transform.SetParent(track.transform);

                segment.transform.position = pathGenerators[k].path.GetPointsInSegment(i)[0];
                var pointData = pathGenerators[k].path.CalculateEvenlySpacedSegmentPoints(i, 0.1f);

                var points = pointData.points;

                TrackSegment trackSegment = segment.AddComponent<TrackSegment>();
                segments.Add(trackSegment);
                trackSegment.generator = k;
                pathGenerators[k].trackSegments[i] = trackSegment;
                trackSegment.points = new TrackSegment.TrackPoint[points.Length];
                MeshFilter meshFilter = segment.AddComponent<MeshFilter>();
                Mesh mesh = new Mesh();
                meshFilter.mesh = mesh;
                MeshRenderer renderer = segment.AddComponent<MeshRenderer>();
                renderer.material = trackMaterial;

                List<Vector3> vertices = new List<Vector3>();
                List<int> indices = new List<int>();

                float totalLength = 0;
                for (int j = 0; j < points.Length; j++)
                {
                    trackSegment.points[j].position = points[j];
                    if (j != 0)
                        trackSegment.points[j].prevDist = trackSegment.points[j - 1].nextDist;
                    if (j != points.Length - 1)
                    {
                        trackSegment.points[j].nextDist = (points[j] - points[j + 1]).magnitude;
                        totalLength += trackSegment.points[j].nextDist;
                    }

                    var a = pathGenerators[k].path.GetPointsInSegment(i);
                    trackSegment.points[j].tangent = Bezier.GetTanget(a[0], a[1], a[2], a[3], pointData.t[j]);

                    if (j != 0)
                    {
                        Vector2 normal1;
                        normal1.x = -trackSegment.points[j].tangent.y;
                        normal1.y = trackSegment.points[j].tangent.x;

                        Vector2 normal2;
                        normal2.x = -trackSegment.points[j - 1].tangent.y;
                        normal2.y = trackSegment.points[j - 1].tangent.x;

                        vertices.Add(trackSegment.points[j].position + normal1 * trackHalfWidth - (Vector2)trackSegment.transform.position);
                        vertices.Add(trackSegment.points[j].position - normal1 * trackHalfWidth - (Vector2)trackSegment.transform.position);
                        vertices.Add(trackSegment.points[j - 1].position - normal2 * trackHalfWidth - (Vector2)trackSegment.transform.position);

                        vertices.Add(trackSegment.points[j - 1].position - normal2 * trackHalfWidth - (Vector2)trackSegment.transform.position);
                        vertices.Add(trackSegment.points[j - 1].position + normal2 * trackHalfWidth - (Vector2)trackSegment.transform.position);
                        vertices.Add(trackSegment.points[j].position + normal1 * trackHalfWidth - (Vector2)trackSegment.transform.position);

                        indices.Add(indices.Count);
                        indices.Add(indices.Count);
                        indices.Add(indices.Count);

                        indices.Add(indices.Count);
                        indices.Add(indices.Count);
                        indices.Add(indices.Count);
                    }
                }
                trackSegment.length = totalLength;

                mesh.SetVertices(vertices);
                mesh.SetIndices(indices, MeshTopology.Triangles, 0);
                mesh.RecalculateBounds();
            }
        }
        UpdateTrackConnectivity();
    }

    private void UpdateTrackConnectivity()
    {
        for (int k = 0; k < pathGenerators.Length; k++)
        {
            //connect start to end if closed loop
			if (pathGenerators[k].path.IsClosed)
			{
                var start = pathGenerators[k].trackSegments[0];
                var end = pathGenerators[k].trackSegments.Last();

                start.ConnectPrev(end);
                end.ConnectNext(start);
            }
            //connect to other path
            if (pathGenerators[k].ConnectStartToPath.connector != null)
            {
                TrackSegment trackSegment = pathGenerators[k].trackSegments[0];
                if (pathGenerators[k].ConnectStartToPath.connectToEnd)
                {
                    TrackSegment connector = pathGenerators[k].ConnectStartToPath.connector.GetClosestSegmentEnd(trackSegment.transform.position);
                    trackSegment.ConnectPrev(connector);
                    connector.ConnectNext(trackSegment);
                }
                else if (pathGenerators[k].ConnectStartToPath.connectToStart)
                {
                    TrackSegment connector = pathGenerators[k].ConnectStartToPath.connector.GetClosestSegmentStart(trackSegment.transform.position);
                    trackSegment.ConnectPrev(connector);
                    connector.ConnectPrev(trackSegment);
                }
            }
            //connect to other path
            if (pathGenerators[k].ConnectEndToPath.connector != null)
            {
                TrackSegment trackSegment = pathGenerators[k].trackSegments.Last();
                if (pathGenerators[k].ConnectEndToPath.connectToStart)
                {
                    TrackSegment connector = pathGenerators[k].ConnectEndToPath.connector.GetClosestSegmentStart(trackSegment.points[trackSegment.points.Length - 1].position);
                    trackSegment.ConnectNext(connector);
                    connector.ConnectPrev(trackSegment);
                }
                else if (pathGenerators[k].ConnectEndToPath.connectToEnd)
                {
                    TrackSegment connector = pathGenerators[k].ConnectEndToPath.connector.GetClosestSegmentEnd(trackSegment.points[trackSegment.points.Length - 1].position);
                    trackSegment.ConnectNext(connector);
                    connector.ConnectNext(trackSegment);
                }
            }

            //connect segments in same path
            for (int i = 0; i < pathGenerators[k].trackSegments.Length-1; i++)
			{
                var current = pathGenerators[k].trackSegments[i];
                var next = pathGenerators[k].trackSegments[i + 1];
                current.ConnectNext(next);
                next.ConnectPrev(current);
            }
        }
    }
}
