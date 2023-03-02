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
		//if(speed > 0)
		{
			Vector2 vel = speed * wagons[0].GetHeading();
			wagons[0].transform.position += new Vector3(vel.x, vel.y, 0);
		}
		//else
		//{
		//	Vector2 vel = speed * wagons[wagons.Count-1].GetHeading();
		//	wagons[wagons.Count - 1].transform.position += new Vector3(vel.x, vel.y, 0);
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
