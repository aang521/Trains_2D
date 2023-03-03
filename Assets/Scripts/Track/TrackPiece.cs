using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackPiece : MonoBehaviour
{
    public List<TrackPiece> Next = new List<TrackPiece>();
    public List<TrackPiece> Prev = new List<TrackPiece>();

    public void ConnectNext(TrackPiece next)
    {
        Next.Add(next);
    }

    public void ConnectPrev(TrackPiece prev)
    {
        Prev.Add(prev);
    }
}
