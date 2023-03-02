using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Train : MonoBehaviour
{
	public TrainSettings trainSettings;

	//wagons in order from front to back
	public List<Wagon> wagons = new List<Wagon>();
	
	public float speed;
	public float totalMass;

	// Who owns the train (0 and higher is player, -1 is no controller)
	public int controller;

	public void Start()
	{
		foreach(Wagon wagon in wagons)
		{
			wagon.SetTrain(this);
		}
		UpdateTotalMass();
	}

	public void UpdateSpeed()
	{
		float oldSpeed = speed;

		float acceleration = 0;
		if (controller >= 0)
		{
			acceleration = Input.GetAxis("forward" + controller) - Input.GetAxis("backward" + controller);
			if (controller == 0)
				acceleration += Input.GetAxis("forwardDebug") - Input.GetAxis("backwardDebug");
		}

		speed += acceleration * trainSettings.maxAccelerationForce * Time.fixedDeltaTime;

		float airDecceleration = (oldSpeed * trainSettings.airResistance * Time.fixedDeltaTime) / totalMass;
		if((speed > 0 && airDecceleration > speed) || (speed < 0 && airDecceleration < speed))
			speed = 0;
		else
			speed -= airDecceleration;

		float deccelration = ((trainSettings.perWagonResistance * wagons.Count) / totalMass) * Time.fixedDeltaTime;
		if (speed > 0)
		{
			speed -= deccelration;
			if (speed < 0)
				speed = 0;
		}
		else
		{
			speed += deccelration;
			if (speed > 0)
				speed = 0;
		}
	}

	public void UpdatePositions()
	{
		int mainWagonIndex = wagons.FindIndex(x => x.isLocomotive);
		if(mainWagonIndex == -1)
		{
			if (speed >= 0)
				mainWagonIndex = 0;
			else
				mainWagonIndex = wagons.Count - 1;
		}
		Wagon mainWagon = wagons[mainWagonIndex];

		float distToTravel = speed * Time.deltaTime;

		var currentSegment = TrackManager.instance.segments[mainWagon.currentSegment];
		mainWagon.distanceAlongSegment += distToTravel;
		while(mainWagon.distanceAlongSegment < 0)
		{
			mainWagon.currentSegment -= 1;//TODO this should be something else
			currentSegment = TrackManager.instance.segments[mainWagon.currentSegment];
			mainWagon.distanceAlongSegment += currentSegment.length;
		}

		float remaining = mainWagon.distanceAlongSegment;
		int currentPointIndex = 0;
		while (remaining > 0)
		{
			remaining -= currentSegment.points[currentPointIndex].nextDist;
			currentPointIndex++;

			if (currentPointIndex >= currentSegment.points.Length)
			{
				mainWagon.currentSegment += 1;//TODO this should be something else
				currentSegment = TrackManager.instance.segments[mainWagon.currentSegment];
				currentPointIndex = 0;
				mainWagon.distanceAlongSegment = remaining; 
			}
		}
		mainWagon.transform.position = currentSegment.points[currentPointIndex].position;
		mainWagon.SetHeading(currentSegment.points[currentPointIndex].tangent);

		//for (int i = mainWagonIndex - 1; i >= 0; i--)
		//{
		//	Wagon wagon = wagons[i];
		//	var currentSegment = TrackManager.instance.segments[wagon.currentSegment];
		//	wagon.distanceAlongSegment += distToTravel;
		//	if(wagon.travelDirectionAlongSegment > 0)
		//	{
		//		float remaining = wagon.distanceAlongSegment;
		//		int currentPointIndex = 0;
		//		while(remaining > 0) {
		//			remaining -= currentSegment.points[currentPointIndex].nextDist;
		//			currentPointIndex++;
		//		}
		//		transform.position = currentSegment.points[currentPointIndex].position;
		//	}
		//	//wagon.transform.position = 
		//}

		//for (int i = mainWagonIndex + 1; i < wagons.Count; i++)
		//{

		//}
	}

	public void UpdateTotalMass()
	{
		totalMass = 0;
		foreach(Wagon wagon in wagons)
		{
			totalMass += wagon.mass;
			if(wagon.cargo != null)
			{
				totalMass += wagon.cargo.GetMass();
			}
		}
	}

	public void AddWagon(Wagon wagon)
	{
		wagons.Add(wagon);
		wagon.SetTrain(this);
		UpdateTotalMass();
	}

	public void RemoveWagon(Wagon wagon)
	{
		wagons.Remove(wagon);
		wagon.SetTrain(this);
		UpdateTotalMass();
	}
}
