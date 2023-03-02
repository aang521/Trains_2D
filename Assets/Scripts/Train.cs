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

	public void Awake()
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

		speed += acceleration / totalMass * trainSettings.maxAccelerationForce * Time.fixedDeltaTime;

		float airDecceleration = (oldSpeed * trainSettings.airResistance * Time.fixedDeltaTime) / totalMass;
		if((speed > 0 && airDecceleration > speed) || (speed < 0 && airDecceleration < speed))
			speed = 0;
		else
			speed -= airDecceleration;

		float decceleration = ((trainSettings.perWagonResistance * wagons.Count) / totalMass) * Time.fixedDeltaTime;
		if (speed > 0)
		{
			speed -= decceleration;
			if (speed < 0)
				speed = 0;
		}
		else
		{
			speed += decceleration;
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

		{
			Wagon mainWagon = wagons[mainWagonIndex];
			float distToTravel = speed * Time.deltaTime;

			var currentSegment = TrackManager.instance.segments[mainWagon.currentSegment];
			mainWagon.distanceAlongSegment += distToTravel;
			while (mainWagon.distanceAlongSegment < 0)
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
		}

		void UpdateWagonPosition(Wagon wagon, Wagon otherWagon, bool behind)
		{
			Vector2 prevAnchorPos = (Vector2)otherWagon.transform.position + (Vector2)otherWagon.transform.right * trainSettings.trainAnchorOffset * (behind ? -1 : 1);

			//TODO this needs to pick the previous track section
			wagon.currentSegment -= 1;
			if (wagon.currentSegment < 0)
				wagon.currentSegment = 0;

			var currentSegment = TrackManager.instance.segments[wagon.currentSegment];

			int currentPointIndex = 0;
			Vector2 currentAnchorPos = currentSegment.points[currentPointIndex].position + currentSegment.points[currentPointIndex].tangent * trainSettings.trainAnchorOffset * (behind ? 1 : -1);
			float sqrDist = (prevAnchorPos - currentAnchorPos).sqrMagnitude;
			float bestDist = sqrDist;
			int bestSegment = wagon.currentSegment;
			int bestPoint = currentPointIndex;
			bool foundBetter = true;
			while (sqrDist > trainSettings.trainAnchorMargin * trainSettings.trainAnchorMargin || foundBetter)
			{
				foundBetter = false;
				currentPointIndex++;

				if (currentPointIndex >= currentSegment.points.Length)
				{
					wagon.currentSegment += 1;//TODO this should be something else
					currentSegment = TrackManager.instance.segments[wagon.currentSegment];
					currentPointIndex = 0;
				}

				currentAnchorPos = currentSegment.points[currentPointIndex].position + currentSegment.points[currentPointIndex].tangent * trainSettings.trainAnchorOffset * (behind ? 1 : -1);
				sqrDist = (prevAnchorPos - currentAnchorPos).sqrMagnitude;
				if (sqrDist <= bestDist)
				{
					bestDist = sqrDist;
					bestSegment = wagon.currentSegment;
					bestPoint = currentPointIndex;
					foundBetter = true;
				}
			}

			wagon.currentSegment = bestSegment;
			currentSegment = TrackManager.instance.segments[wagon.currentSegment];
			wagon.transform.position = currentSegment.points[bestPoint].position;
			wagon.SetHeading(currentSegment.points[bestPoint].tangent);
		}

		//wagon in front
		for (int i = mainWagonIndex - 1; i >= 0; i--)
		{
			UpdateWagonPosition(wagons[i], wagons[i + 1], false);
		}

		//wagons behind
		for (int i = mainWagonIndex + 1; i < wagons.Count; i++)
		{
			UpdateWagonPosition(wagons[i], wagons[i - 1], true);
		}
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

	public void AddWagonBack(Wagon wagon)
	{
		wagons.Add(wagon);
		wagon.SetTrain(this);
		UpdateTotalMass();
	}

	public void AddWagonFront(Wagon wagon)
	{
		wagons.Insert(0, wagon);
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
