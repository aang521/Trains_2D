using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Trains/Cargo Definition")]
public class CargoDefinition : ScriptableObject
{
	public float mass;
	public float staticFriction;

	public float droppingFine;

	public enum CargoType
	{
		FISH,
	}
	public CargoType cargoType;
}
