using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackSegment : MonoBehaviour
{
    public struct TrackPoint
	{
		public Vector2 position;
		public Vector2 tangent;
		public float nextDist;
		public float prevDist;
	}

	public TrackPoint[] points;
	public float length;
}
