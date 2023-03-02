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

	public int currentSegment;
	public float distanceAlongSegment;

	public bool isLocomotive;

	public void Awake()
	{
		spriteRenderer.sprite = isLocomotive ? trainSettings.locomotiveSprite : trainSettings.wagonSprite;
	}

	public void SetHeading(Vector2 heading)
	{
#pragma warning disable CS0618 // Type or member is obsolete
		transform.rotation = Quaternion.AxisAngle(new Vector3(0, 0, 1), Mathf.Atan2(heading.y, heading.x));
#pragma warning restore CS0618 // Type or member is obsolete
	}

	public void AddImpulseForce(Vector2 acceleration)
	{
		
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
