using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cargo
{
	public CargoDefinition definition;
	public int amount;

	public float GetMass()
	{
		return definition.unitMass * amount;
	}
}