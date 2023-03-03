using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    public static TrackManager instance;

    public PathGenerator[] pathGenerator;

    public List<TrackSegment> segments = new List<TrackSegment>();

    public TrackSegment Prev;
    public TrackSegment First;

    public float trackHalfWidth = 1;
    public Material trackMaterial;

    public void Awake()
    {
        instance = this;

        SetupLevel();
    }

    public void SetupLevel()
    {
        GameObject track = new GameObject("Track");
        for (int k = 0; k < pathGenerator.Length; k++)
        {
            pathGenerator[k].trackSegments = new TrackSegment[pathGenerator[k].path.NumberOfSegments];
            for (int i = 0; i < pathGenerator[k].path.NumberOfSegments; i++)
            {
                GameObject segment = new GameObject("segment");
                segment.transform.SetParent(track.transform);

                segment.transform.position = pathGenerator[k].path.GetPointsInSegment(i)[0];
                var pointData = pathGenerator[k].path.CalculateEvenlySpacedSegmentPoints(i, 0.1f);

                var points = pointData.points;

                TrackSegment trackSegment = segment.AddComponent<TrackSegment>();
                segments.Add(trackSegment);
                trackSegment.generator = k;
                pathGenerator[k].trackSegments[i] = trackSegment;
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

                    var a = pathGenerator[k].path.GetPointsInSegment(i);
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
        for (int k = 0; k < pathGenerator.Length; k++)
        {
            for (int i = 0; i < pathGenerator[k].trackSegments.Length; i++)
            {
                //Connecting tracksegments to each other
                TrackSegment trackSegment = pathGenerator[k].trackSegments[i];

                //Open path
                if (!pathGenerator[k].path.IsClosed)
                {
                    //connecting the starting segment to an existing segment of another generator
                    if (i == 0)
                    {
                        First = trackSegment;
                        if (pathGenerator[k].ConnectStartToPath.connector != null)
                        {
                            if (pathGenerator[k].ConnectStartToPath.connectToEnd)
                            {
                                TrackSegment connector = pathGenerator[k].ConnectStartToPath.connector.GetClosestSegmentEnd(trackSegment.transform.position);
                                trackSegment.ConnectPrev(connector);
                                connector.ConnectNext(trackSegment);
                            }
                            else if (pathGenerator[k].ConnectStartToPath.connectToStart)
                            {
                                TrackSegment connector = pathGenerator[k].ConnectStartToPath.connector.GetClosestSegmentStart(trackSegment.transform.position);
                                trackSegment.ConnectPrev(connector);
                                connector.ConnectPrev(trackSegment);
                            }
                        }
                    }
                    //Connection for the last segment of a track
                    else if (i >= pathGenerator[k].trackSegments.Length - 1 && pathGenerator[k].ConnectEndToPath.connector != null)
                    {
                        if (pathGenerator[k].ConnectEndToPath.connectToStart)
                        {
                            //connecting the ending segment to an existing segment of another generator
                            TrackSegment connector = pathGenerator[k].ConnectEndToPath.connector.GetClosestSegmentStart(trackSegment.points[trackSegment.points.Length - 1].position);
                            trackSegment.ConnectNext(connector);
                            connector.ConnectPrev(trackSegment);
                        }
                        else if (pathGenerator[k].ConnectEndToPath.connectToEnd)
                        {
                            TrackSegment connector = pathGenerator[k].ConnectEndToPath.connector.GetClosestSegmentEnd(trackSegment.points[trackSegment.points.Length - 1].position);
                            trackSegment.ConnectNext(connector);
                            connector.ConnectNext(trackSegment);
                        }
                    }

                    if (Prev != null)
                    {
                        trackSegment.ConnectPrev(Prev);
                        Prev.ConnectNext(trackSegment);
                    }
                }
                //Closed path
                else
                {
                    if (i == 0)
                        First = trackSegment;
                    //Connection for the last segment of a track
                    else if (i >= pathGenerator[k].trackSegments.Length - 1)
                    {
                        //connecting the ending segment to an existing segment of another generator
                        trackSegment.ConnectNext(First);
                        First.ConnectPrev(trackSegment);
                    }

                    if (Prev != null)
                    {
                        trackSegment.ConnectPrev(Prev);
                        Prev.ConnectNext(trackSegment);
                    }
                }

                Prev = trackSegment;
            }
            First = null;
            Prev = null;
        }
    }
}
