using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainSystem : MonoBehaviour
{
	public static TrainSystem instance;
	public TrainSettings trainSettings;

	public List<Train> trains = new List<Train>();

	public PathGenerator pathGenerator;

	public Wagon wagonPrefab;
	public Wagon locomotivePrefab;

	public CargoDefinition testDefinition;

	public void Awake()
	{
		instance = this;
	}

	public Train MakeNewTrain()
	{
		Train newTrain = new Train();
		trains.Add(newTrain);

		return newTrain;
	}

	public void Start()
	{
		var locomotiveWagon = Instantiate(locomotivePrefab);
		Train locomotive = new Train();
		locomotive.controller = 0;
		trains.Add(locomotive);
		locomotive.AddWagonFront(locomotiveWagon);

		locomotive.wagons[0].transform.position = pathGenerator.path.GetPointsInSegment(0)[0];
		locomotive.wagons[0].currentSegment = 0;
		locomotive.wagons[0].distanceAlongSegment = 70;

		//locomotive.AddWagonFront(Instantiate(wagonPrefab));
		locomotive.AddWagonBack(Instantiate(wagonPrefab));
		locomotive.AddWagonBack(Instantiate(wagonPrefab));

		locomotiveWagon = Instantiate(locomotivePrefab);
		locomotive = new Train();
		trains.Add(locomotive);
		locomotive.AddWagonFront(locomotiveWagon);
		locomotive.controller = 1;
		locomotive.wagons[0].transform.position = pathGenerator.path.GetPointsInSegment(0)[0];
		locomotive.wagons[0].currentSegment = 0;
		locomotive.wagons[0].distanceAlongSegment = 120;
		locomotive.wagons[0].SetTrain(locomotive);

		locomotive.AddWagonBack(Instantiate(wagonPrefab));
		locomotive.AddWagonBack(Instantiate(wagonPrefab));

		foreach (Train train in trains)
		{
			train.UpdatePositions();
		}
	}

	public void Update()
	{
		for(int i = 0; i < trains.Count; i++)
			trains[i].UpdateInput();
	}

	public void FixedUpdate()
	{
		if (!GameManager.instance.playing) return;

		foreach(Train train in trains)
		{
			train.UpdateSpeed();
		}

		foreach (Train train in trains)
		{
			train.UpdatePositions();
		}

		foreach (Train train in trains)
		{
			train.ResolveCollisions();
		}

		foreach (Train train in trains)
		{
			foreach(Wagon wagon in train.wagons)
			{
				wagon.UpdateCargo();
			}
		}

		for(int i = 0; i < trains.Count; i++)
		{
			if (trains[i].wagons.Count == 0)
			{
				trains.RemoveAt(i);
				i--;
			}
		}
	}
}
