using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainSystem : MonoBehaviour
{
	public static TrainSystem instance;
	public TrainSettings trainSettings;

	public List<Train> trains = new List<Train>();

	public Wagon wagonPrefab;
	public Wagon locomotivePrefab;

	public int startWagonCount = 2;

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
		SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();
		foreach(SpawnPoint spawnPoint in spawnPoints)
		{
			var locomotiveWagon = Instantiate(locomotivePrefab);
			Train locomotive = new Train();
			locomotive.controller = spawnPoint.playerIndex;
			trains.Add(locomotive);
			locomotive.AddWagonFront(locomotiveWagon);

			locomotive.speed = 0.01f;
			locomotiveWagon.isInversedOnSegment = spawnPoint.reverseDirection;

			locomotive.wagons[0].currentSegment = spawnPoint.segmentIndex;
			float distance = 0;
			for(int i = 0; i < spawnPoint.pointIndex; i++)
			{
				distance += TrackManager.instance.segments[spawnPoint.segmentIndex].points[i].nextDist;
			}
			locomotive.wagons[0].distanceAlongSegment = distance;

			for(int i = 0; i < startWagonCount; i++)
			{
				Wagon wagon = Instantiate(wagonPrefab);
				locomotive.AddWagonBack(wagon);
				wagon.currentSegment = spawnPoint.segmentIndex;
				if(spawnPoint.reverseDirection)
					wagon.distanceAlongSegment = TrackManager.instance.segments[spawnPoint.segmentIndex].length;
				wagon.isInversedOnSegment = spawnPoint.reverseDirection;
			}
		}

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

		List<Train.SolvedCollision> solvedCollisions = new List<Train.SolvedCollision>(4);
		foreach (Train train in trains)
		{
			train.ResolveCollisions(solvedCollisions);
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
