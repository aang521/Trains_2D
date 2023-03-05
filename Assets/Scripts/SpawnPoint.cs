using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	[HideInInspector]
	public TrainSettings trainSettings;

	public int playerIndex;
	public bool reverseDirection;

	[HideInInspector]
	public int segmentIndex;
	[HideInInspector]
	public int pointIndex;

	private TrackManager trackManager;
	private void OnDrawGizmos()
	{
		if (!trackManager)
			trackManager = FindObjectOfType<TrackManager>();

		if (!trackManager) return;

		float minDist = float.MaxValue;
		int bestSegmentIndex = 0;
		int bestSegmentPoint = 0;

		if(trackManager.segments.Count == 0)
		{
			trackManager.SetupLevel();
		}

		for(int j = 0; j < trackManager.segments.Count; j++)
		{
			for( int k = 0; k < trackManager.segments[j].points.Length; k++)
			{
				float dist = (trackManager.segments[j].points[k].position - (Vector2)transform.position).sqrMagnitude;
				if(dist < minDist)
				{
					minDist = dist;
					bestSegmentIndex = j;
					bestSegmentPoint = k;
				}
			}
		}

		segmentIndex = bestSegmentIndex;
		pointIndex = bestSegmentPoint;

		if (playerIndex == -1)
			Gizmos.color = trainSettings.noPlayerColor;
		else
			Gizmos.color = trainSettings.playerColors[playerIndex];
		Gizmos.DrawSphere(trackManager.segments[segmentIndex].points[pointIndex].position, 5);
	}
}
