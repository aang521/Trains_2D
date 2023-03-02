using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainSystem : MonoBehaviour
{
	public List<Train> trains = new List<Train>();

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
