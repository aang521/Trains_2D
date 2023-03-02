using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wagon : MonoBehaviour
{
	public TrainSettings trainSettings;

	//mass of the empty wagon
	public float mass = 1;

	public SpriteRenderer spriteRenderer;

	//train that this wagon is part of
    private Train train;
	//the cargo in this wagon
    public Cargo cargo;

	public bool isLocomotive;

	private Vector2 prevVelocity;
	//stored per frame
	private Vector2 accumulatedAcceleration;

	public void Awake()
	{
		spriteRenderer.sprite = isLocomotive ? trainSettings.locomotiveSprite : trainSettings.wagonSprite;
	}

	private void FixedUpdate()
	{
		Vector2 heading = GetHeading();
		Vector2 velocity = heading * train.speed;

		Vector2 acceleration = velocity - prevVelocity;
		prevVelocity = velocity;
		accumulatedAcceleration = acceleration;

#pragma warning disable CS0618 // Type or member is obsolete
		transform.rotation = Quaternion.AxisAngle(new Vector3(0, 0, 1), Mathf.Atan2(heading.y, heading.x));
#pragma warning restore CS0618 // Type or member is obsolete
	}

	public Vector2 GetHeading()
	{
		//TODO get heading based on where on the track
		return new Vector2(1, 0);
	}

	public void AddImpulseAcceleration(Vector2 acceleration)
	{
		accumulatedAcceleration += acceleration;
	}

	public void SetTrain(Train train)
	{
		this.train = train;
		if (train.controller == -1)
			spriteRenderer.color = trainSettings.noPlayerColor;
		else
			spriteRenderer.color = trainSettings.playerColors[train.controller];
	}
}
