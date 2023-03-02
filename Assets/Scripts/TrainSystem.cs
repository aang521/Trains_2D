using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainSystem : MonoBehaviour
{
	public static TrainSystem instance;

	public List<Train> trains = new List<Train>();

	public PathGenerator pathGenerator;

	public void Awake()
	{
		instance = this;
	}

	public void Start()
	{
		trains[0].transform.position = pathGenerator.path.GetPointsInSegment(0)[0];
		trains[0].wagons[0].currentSegment = 0;
		trains[0].wagons[0].distanceAlongSegment = 0;
	}

	public void FixedUpdate()
	{
		foreach(Train train in trains)
		{
			train.UpdateSpeed();
		}

		foreach (Train train in trains)
		{
			train.UpdatePositions();
		}

		//TODO resolve collisions
	}
}
