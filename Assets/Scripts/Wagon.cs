using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wagon : MonoBehaviour
{
	public TrainSettings trainSettings;

	//mass of the empty wagon
	public float mass = 1;

	public SpriteRenderer spriteRenderer;
	public new Collider2D collider;

	//train that this wagon is part of
    public Train train;

	public Cargo cargoPrefab;
	//the cargo in this wagon
    public Cargo cargo;
	public Vector2 cargoPosition;

	public Vector2 cargoDropThreshold;

	public int currentSegment;
	public float distanceAlongSegment;

	public bool isLocomotive;

	public void Awake()
	{
		prevWagonPos = transform.position;

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

	public void Update()
	{
		if (cargo)
		{
			cargo.transform.position = cargoPosition;
		}
	}

	public void AddCargo(CargoDefinition definition)
	{
		cargo = Instantiate(cargoPrefab);
		cargo.transform.parent = transform;
		cargo.definition = definition;
		cargoPosition = transform.position;
		prevWagonPos = transform.position;

		cargoVelocity = Vector2.zero;

		train.UpdateTotalMass();
	}

	private Vector2 prevWagonPos;
	private Vector2 cargoVelocity;
	public void UpdateCargo()
	{
		Vector2 wagonVelocity = ((Vector2)transform.position - prevWagonPos) / Time.deltaTime;
		prevWagonPos = transform.position;
		if (!cargo) return;

		Vector2 deltaVelocity = cargoVelocity - wagonVelocity;

		if (deltaVelocity.magnitude * cargo.GetMass() < cargo.definition.staticFriction)
		{
			cargoVelocity = wagonVelocity;
		}
		else
		{
			deltaVelocity *= cargo.definition.staticFriction / (deltaVelocity.magnitude * cargo.GetMass());
			cargoVelocity -= deltaVelocity;
		}

		cargoPosition += cargoVelocity * Time.deltaTime;

		Vector2 relativeCargoPos = cargoPosition - (Vector2)transform.position;
		if(Mathf.Abs(relativeCargoPos.x) > cargoDropThreshold.x || Mathf.Abs(relativeCargoPos.y) > cargoDropThreshold.y)
		{
			cargo.Detach(cargoVelocity);
			cargo = null;
		}
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(transform.position + transform.right * -trainSettings.trainAnchorOffset, 1);
		Gizmos.DrawSphere(transform.position + transform.right * trainSettings.trainAnchorOffset, 1);
	}
}
