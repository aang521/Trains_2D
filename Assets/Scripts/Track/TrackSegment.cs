using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackSegment : MonoBehaviour
{
    [Serializable]
    public struct TrackPoint
    {
        public Vector2 position;
        public Vector2 tangent;
        public float nextDist;
        public float prevDist;
    }

    public TrackPoint[] points;
    public float length;
    public int generator;

    public List<TrackSegment> Next = new List<TrackSegment>();
    public List<TrackSegment> Prev = new List<TrackSegment>();

    public void ConnectNext(TrackSegment next)
    {
        if (!Next.Contains(next))
        Next.Add(next);
    }

    public void ConnectPrev(TrackSegment prev)
    {
        if (!Prev.Contains(prev))
            Prev.Add(prev);
    }
}
