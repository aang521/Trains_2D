using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainSystem : MonoBehaviour
{
	public static TrainSystem instance;

	public List<Train> trains = new List<Train>();

	public PathGenerator pathGenerator;

	public Wagon wagonPrefab;
	public Train locomotivePrefab;

	public CargoDefinition testDefinition;

	public void Awake()
	{
		instance = this;
	}

	public void Start()
	{
		var locomotive = Instantiate(locomotivePrefab);
		trains.Add(locomotive);

		locomotive.wagons[0].transform.position = pathGenerator.path.GetPointsInSegment(0)[0];
		locomotive.wagons[0].currentSegment = 0;
		locomotive.wagons[0].distanceAlongSegment = 70;

		locomotive.AddWagonFront(Instantiate(wagonPrefab));
		locomotive.AddWagonBack(Instantiate(wagonPrefab));

		var last = Instantiate(wagonPrefab);
		locomotive.AddWagonBack(last);

		foreach (Train train in trains)
		{
			train.UpdatePositions();
		}
		last.AddCargo(testDefinition);
		last.UpdateCargo();
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

		foreach(Train train in trains)
		{
			foreach(Wagon wagon in train.wagons)
			{
				wagon.UpdateCargo();
			}
		}
	}
}
