using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Station : MonoBehaviour
{
	public new Collider2D collider;

	public SpriteRenderer loadingAreaSprite;

	public CargoDefinition cargoToPickUp;

	[Serializable]
	public struct AcceptedCargo
	{
		public CargoDefinition.CargoType type;
		public float price;
	}

	public List<AcceptedCargo> acceptedCargos = new List<AcceptedCargo>();

	public List<CargoDefinition> possibleCargosToPickUp = new List<CargoDefinition>();
	public CargoDefinition currentCargoAvailable;

	public float loadTime;
	[HideInInspector]
	public float loadStartTime;
	[HideInInspector]
	public Wagon currentLoadingWagon;

	public SpriteRenderer availableCargoSprite;

	public float minCargoSpawnTime;
	public float maxCargoSpawnTime;
	[HideInInspector]
	public float nextSpawnTime;

	public void Awake()
	{
		nextSpawnTime = Time.time + UnityEngine.Random.Range(minCargoSpawnTime, maxCargoSpawnTime);
	}

	public void FixedUpdate()
	{
		loadingAreaSprite.color = Color.red;

		List<Collider2D> results = new List<Collider2D>();
		ContactFilter2D a = new ContactFilter2D();
		a.NoFilter();
		collider.OverlapCollider(a, results);


		Wagon newLoadingWagon = null;
		foreach (Collider2D collider in results)
		{
			Wagon wagon = collider.GetComponent<Wagon>();
			if(wagon && wagon.train.controller >= 0 && !wagon.isLocomotive)
			{
				bool canDropcargo = wagon.cargo != null && acceptedCargos.Any(x => x.type == wagon.cargo.definition.cargoType);
				bool canPickUpcargo = wagon.cargo == null;
				if(canDropcargo || canPickUpcargo)
				{
					if (collider.OverlapPoint(wagon.transform.position))
					{
						loadingAreaSprite.color = Color.green;
						newLoadingWagon = wagon;
						break;
					}
				}
			}
		}

		if (newLoadingWagon == null)
		{
			currentLoadingWagon = null;
		}
		else
		{
			if(currentLoadingWagon != newLoadingWagon)
			{
				currentLoadingWagon = newLoadingWagon;
				loadStartTime = Time.time;
			}
		}

		if(currentLoadingWagon && Time.time  - loadStartTime > loadTime)
		{
			if(currentLoadingWagon.cargo == null)
			{
				if (currentCargoAvailable)
				{
					currentLoadingWagon.AddCargo(currentCargoAvailable);
					currentCargoAvailable = null;
					availableCargoSprite.enabled = false;
					nextSpawnTime = Time.time + UnityEngine.Random.Range(minCargoSpawnTime, maxCargoSpawnTime);
					Debug.Log("Picked up cargo");
				}
			}
			else
			{
				int index = acceptedCargos.FindIndex(x => x.type == currentLoadingWagon.cargo.definition.cargoType);
				if(index >= 0)
				{
					currentLoadingWagon.RemoveCargo();
					GameManager.instance.playerScores[currentLoadingWagon.train.controller] += acceptedCargos[index].price;
					Debug.Log("Dropped of cargo");
				}
			}
		}

		if(cargoToPickUp && currentCargoAvailable == null && Time.time >= nextSpawnTime)
		{
			currentCargoAvailable = cargoToPickUp;
			availableCargoSprite.enabled = true;
			availableCargoSprite.sprite = currentCargoAvailable.sprite;
		}
	}
}
