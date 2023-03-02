using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Trains/Cargo Definition")]
public class CargoDefinition : ScriptableObject
{
	public float unitMass;
	public float escapeForce;
}
