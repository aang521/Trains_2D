using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    public static TrackManager instance;

	public PathGenerator pathGenerator;

	public List<TrackSegment> segments = new List<TrackSegment>();

	public void Awake()
	{
		instance = this;

		SetupLevel();
	}

	public void SetupLevel() {
		GameObject track = new GameObject("Track");
		for(int i = 0; i < pathGenerator.path.NumberOfSegments; i++)
		{
			GameObject segment = new GameObject("segment");
			segment.transform.SetParent(track.transform);

			segment.transform.position = pathGenerator.path.GetPointsInSegment(i)[0];
			var pointData = pathGenerator.path.CalculateEvenlySpacedSegmentPoints(i, 0.1f);

			var points = pointData.points;

			TrackSegment trackSegment = segment.AddComponent<TrackSegment>();
			trackSegment.points = new TrackSegment.TrackPoint[points.Length];
			segments.Add(trackSegment);

			float totalLength = 0;
			for(int j = 0; j < points.Length; j++)
			{
				var t = new GameObject();
				t.transform.SetParent(segment.transform);
				t.transform.position = points[j];

				trackSegment.points[j].position = points[j];
				if(j != 0)
					trackSegment.points[j].prevDist = trackSegment.points[j-1].nextDist;
				if (j != points.Length - 1)
				{
					trackSegment.points[j].nextDist = (points[j] - points[j + 1]).magnitude;
					totalLength += trackSegment.points[j].nextDist;
				}

				var a = pathGenerator.path.GetPointsInSegment(i);
				trackSegment.points[j].tangent = Bezier.GetTanget(a[0], a[1], a[2], a[3], pointData.t[j]);
			}
			trackSegment.length = totalLength;

		}
	}
}
