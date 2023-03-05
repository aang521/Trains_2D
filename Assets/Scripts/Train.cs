using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Train
{
	public TrainSettings trainSettings { get { return TrainSystem.instance.trainSettings; } }

	//wagons in order from front to back
	public List<Wagon> wagons = new List<Wagon>();

	public float speed;
	public float totalMass;

	// Who owns the train (0 and higher is player, -1 is no controller)
	public int controller = -1;


	public void Awake()
	{
		foreach (Wagon wagon in wagons)
		{
			wagon.SetTrain(this);
		}
		UpdateTotalMass();
	}

	public void DecoupleWagon(Wagon decoupledWagon)
	{
		if (!decoupledWagon.isLocomotive)
		{
			RemoveWagon(decoupledWagon);
			Train newTrain = TrainSystem.instance.MakeNewTrain();
			newTrain.controller = -1;
			newTrain.AddWagonBack(decoupledWagon);
			newTrain.speed = this.speed;
		}
	}

	public void UpdateInput()
	{
		if (controller < 0) return;
		if (Input.GetButtonDown("decoupleBack" + controller) || controller == 0 && Input.GetButtonDown("decoupleBackDebug"))
		{
			if (speed > -0.1)
				DecoupleWagon(wagons[wagons.Count - 1]);
		}

		if (Input.GetButtonDown("decoupleFront" + controller) || controller == 0 && Input.GetButtonDown("decoupleFrontDebug"))
		{
			if (speed < 0.1)
				DecoupleWagon(wagons[0]);
		}
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
		if ((speed > 0 && airDecceleration > speed) || (speed < 0 && airDecceleration < speed))
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

	private bool GetNextTrack(Wagon wagon, float excessDistance)
	{
		float targetValue = 0;
		if (Input.GetAxis("direction" + controller) < -0.5 || (controller == 0 && Input.GetKey(KeyCode.A)))
		{
			targetValue = -1;
		}
		else if (Input.GetAxis("direction" + controller) > 0.5 || (controller == 0 && Input.GetKey(KeyCode.D)))
		{
			targetValue = 1;
		}

		if (excessDistance > 0)
		{
			TrackSegment currentSegment = TrackManager.instance.segments[wagon.currentSegment];
			TrackSegment.TrackPoint current = currentSegment.points.Last();
			Vector2 currentEnd = current.position;
			Vector2 currentNormal = new Vector2(-current.tangent.y, current.tangent.x);
			if (wagon.isInversedOnSegment)
				currentNormal *= -1;
			float bestValue = float.MaxValue;
			TrackSegment bestNextSegment = null;
			bool bestShouldInvertDirection = false;
			for (int i = 0; i < TrackManager.instance.segments[wagon.currentSegment].Next.Count; i++) 
			{
				var next = TrackManager.instance.segments[wagon.currentSegment].Next[i];

				Vector2 endA = next.points[0].position;
				Vector2 endB = next.points.Last().position;

				float distA = Vector2.Distance(currentEnd, endA);
				float distB = Vector2.Distance(currentEnd, endB);

				float dot;
				if (distA < distB)
					dot = Vector2.Dot(currentNormal, (next.points[0].position - next.points[100].position).normalized);
				else
					dot = Vector2.Dot(currentNormal, (next.points.Last().position - next.points[next.points.Length - 101].position).normalized);

				float dist = Mathf.Abs(targetValue - dot);
				if (dist < bestValue)
				{
					bestValue = dist;
					bestNextSegment = next;
					bestShouldInvertDirection = distA > distB;
				}
			}

			wagon.currentSegment = TrackManager.instance.segments.IndexOf(bestNextSegment);
			if (bestShouldInvertDirection)
				wagon.isInversedOnSegment = !wagon.isInversedOnSegment;
		}
		else
		{
			TrackSegment.TrackPoint current = TrackManager.instance.segments[wagon.currentSegment].points[0];
			Vector2 currentEnd = current.position;
			Vector2 currentNormal = new Vector2(-current.tangent.y, current.tangent.x);
			if (wagon.isInversedOnSegment)
				currentNormal *= -1;
			float bestValue = float.MaxValue;
			TrackSegment bestNextSegment = null;
			bool bestShouldInvertDirection = false;
			for (int i = 0; i < TrackManager.instance.segments[wagon.currentSegment].Prev.Count; i++)
			{
				var next = TrackManager.instance.segments[wagon.currentSegment].Prev[i];

				Vector2 endA = next.points[0].position;
				Vector2 endB = next.points.Last().position;

				float distA = Vector2.Distance(currentEnd, endA);
				float distB = Vector2.Distance(currentEnd, endB);

				float dot;
				if (distA < distB)
					dot = Vector2.Dot(currentNormal, (next.points[0].position - next.points[100].position).normalized);
				else
					dot = Vector2.Dot(currentNormal, (next.points.Last().position - next.points[next.points.Length-101].position).normalized);

				float dist = Mathf.Abs(targetValue - dot);
				if (dist < bestValue)
				{
					bestValue = dist;
					bestNextSegment = next;
					bestShouldInvertDirection = distA < distB;
				}
			}

			wagon.currentSegment = TrackManager.instance.segments.IndexOf(bestNextSegment);
			if (bestShouldInvertDirection)
				wagon.isInversedOnSegment = !wagon.isInversedOnSegment;
		}
		return true;
	}

	public void UpdatePositions()
	{
		int mainWagonIndex;
		if (speed >= 0)
			mainWagonIndex = 0;
		else
			mainWagonIndex = wagons.Count - 1;

		{
			Wagon mainWagon = wagons[mainWagonIndex];

			float distToTravel = speed * Time.fixedDeltaTime;

			var currentSegment = TrackManager.instance.segments[mainWagon.currentSegment];
			if (mainWagon.isInversedOnSegment)
				mainWagon.distanceAlongSegment -= distToTravel;
			else
				mainWagon.distanceAlongSegment += distToTravel;
			
			{
				while (mainWagon.distanceAlongSegment < 0)
				{
					TrackSegment prevSegment = currentSegment;
					bool prevInversed = mainWagon.isInversedOnSegment;
					float distanceIntoNext = -mainWagon.distanceAlongSegment;

					bool foundTrack = GetNextTrack(mainWagon, mainWagon.distanceAlongSegment);
					if (!foundTrack)
					{
						speed = 0;
						mainWagon.distanceAlongSegment = 0;
						break;
					}
					currentSegment = TrackManager.instance.segments[mainWagon.currentSegment];

					if(mainWagon.isInversedOnSegment != prevInversed)
						mainWagon.distanceAlongSegment = distanceIntoNext;
					else
						mainWagon.distanceAlongSegment = currentSegment.length - distanceIntoNext;
				}

				while (mainWagon.distanceAlongSegment > currentSegment.length)
				{
					TrackSegment prevSegment = currentSegment;
					bool prevInversed = mainWagon.isInversedOnSegment;
					float distanceIntoNext = mainWagon.distanceAlongSegment - currentSegment.length;

					bool foundTrack = GetNextTrack(mainWagon, mainWagon.distanceAlongSegment);
					if (!foundTrack)
					{
						mainWagon.distanceAlongSegment = currentSegment.length;
						speed = 0;
						break;
					}
					currentSegment = TrackManager.instance.segments[mainWagon.currentSegment];

					if (mainWagon.isInversedOnSegment != prevInversed)
						mainWagon.distanceAlongSegment = currentSegment.length - distanceIntoNext;
					else
						mainWagon.distanceAlongSegment = distanceIntoNext;
				}
			}

			float remaining = mainWagon.distanceAlongSegment;
			int currentPointIndex = 0;
			while (remaining > 0 && currentPointIndex < currentSegment.points.Length - 1)
			{
				remaining -= currentSegment.points[currentPointIndex].nextDist;
				currentPointIndex++;
			}
			mainWagon.transform.position = currentSegment.points[currentPointIndex].position;
			mainWagon.SetHeading(currentSegment.points[currentPointIndex].tangent * (mainWagon.isInversedOnSegment ? -1 : 1));
		}

		void UpdateWagonPosition(Wagon wagon, int wagonIndex, Wagon otherWagon, bool behind)
		{
			

			if (speed == 0)
				return;
			Vector2 otherAnchorPos = (Vector2)otherWagon.transform.position + (Vector2)otherWagon.transform.right * trainSettings.trainAnchorOffset * (behind ? -1 : 1);

			var currentSegment = TrackManager.instance.segments[wagon.currentSegment];
			int currentPointIndex = 0;

			void GetPrevTrack()
			{
				Vector2 currentEnd = TrackManager.instance.segments[wagon.currentSegment].points[0].position;
				TrackSegment otherSegment = TrackManager.instance.segments[otherWagon.currentSegment];
				TrackSegment next = currentSegment.Prev.Find(x => x == otherSegment);
				if (next == null)
					next = currentSegment.Prev.Find(x => x.Next.Any(y => y == otherSegment) || x.Prev.Any(y => y == otherSegment));
				if (next == null)
					next = currentSegment.Next.Find(x => x == otherSegment);
				if (next == null)
					next = currentSegment.Next.Find(x => x.Next.Any(y => y == otherSegment) || x.Prev.Any(y => y == otherSegment));
				currentSegment = next;
				if (currentSegment == null)
				{
					Debug.DebugBreak();
					Debug.LogError("Could not find next track");
				}
				currentSegment = next;

				wagon.currentSegment = TrackManager.instance.segments.IndexOf(currentSegment);

				Vector2 endA = TrackManager.instance.segments[wagon.currentSegment].points[0].position;
				Vector2 endB = TrackManager.instance.segments[wagon.currentSegment].points.Last().position;

				float distA = Vector2.Distance(currentEnd, endA);
				float distB = Vector2.Distance(currentEnd, endB);

				bool start = distA < distB;

				if (distA < distB)
					wagon.isInversedOnSegment = !wagon.isInversedOnSegment;

				currentPointIndex = start ? 0 : currentSegment.points.Length - 1;
			}

			void GetNextTrack()
			{
				Vector2 currentEnd = TrackManager.instance.segments[wagon.currentSegment].points.Last().position;
				TrackSegment otherSegment = TrackManager.instance.segments[otherWagon.currentSegment];
				TrackSegment next = currentSegment.Next.Find(x => x == otherSegment);
				if (next == null)
					next = currentSegment.Next.Find(x => x.Next.Any(y => y == otherSegment) || x.Prev.Any(y => y == otherSegment));
				if (next == null)
					next = currentSegment.Prev.Find(x => x == otherSegment);
				if (next == null)
					next = currentSegment.Prev.Find(x => x.Next.Any(y => y == otherSegment) || x.Prev.Any(y => y == otherSegment));
				currentSegment = next;
				if (currentSegment == null)
				{
					Debug.DebugBreak();
					Debug.LogError("Could not find next track");
				}
				wagon.currentSegment = TrackManager.instance.segments.IndexOf(currentSegment);

				wagon.currentSegment = TrackManager.instance.segments.IndexOf(currentSegment);

				Vector2 endA = TrackManager.instance.segments[wagon.currentSegment].points[0].position;
				Vector2 endB = TrackManager.instance.segments[wagon.currentSegment].points.Last().position;

				float distA = Vector2.Distance(currentEnd, endA);
				float distB = Vector2.Distance(currentEnd, endB);

				bool start = distA < distB;

				if (distA > distB)
					wagon.isInversedOnSegment = !wagon.isInversedOnSegment;

				currentPointIndex = start ? 0 : currentSegment.points.Length - 1;
			}

			float remaining = wagon.distanceAlongSegment;
			while(remaining > 0 && currentPointIndex < currentSegment.points.Length-1)
			{
				remaining -= currentSegment.points[currentPointIndex].nextDist;
				currentPointIndex++;
			}

			if (speed * (wagon.isInversedOnSegment ? -1 : 1) > 0)
				currentPointIndex--;
			else
				currentPointIndex++;
			if (currentPointIndex < 0)
			{
				GetPrevTrack();
			}
			if (currentPointIndex >= currentSegment.points.Length)
			{
				GetNextTrack();
			}

			bool dragged = (behind && speed >= 0) || (!behind && speed < 0);
			Debug.Assert(dragged);

			Vector2 currentAnchorPos = currentSegment.points[currentPointIndex].position + currentSegment.points[currentPointIndex].tangent * trainSettings.trainAnchorOffset * (behind ? 1 : -1);
			float sqrDist = (otherAnchorPos - currentAnchorPos).sqrMagnitude;
			float bestDist = sqrDist;
			int bestSegment = wagon.currentSegment;
			int bestPoint = currentPointIndex;
			bool bestInversed = wagon.isInversedOnSegment;
			bool foundBetter = true;
			while (sqrDist > trainSettings.trainAnchorMargin * trainSettings.trainAnchorMargin || foundBetter)
			{
				foundBetter = false;
				if (speed * (wagon.isInversedOnSegment ? -1 : 1) > 0)
					currentPointIndex++;
				else
					currentPointIndex--;

				if(currentPointIndex < 0)
				{
					GetPrevTrack();

					bestDist = float.MaxValue;
				}
				if(currentPointIndex >= currentSegment.points.Length)
				{
					GetNextTrack();

					bestDist = float.MaxValue;
				}

				currentAnchorPos = currentSegment.points[currentPointIndex].position + currentSegment.points[currentPointIndex].tangent * (wagon.isInversedOnSegment ? -1 : 1) * trainSettings.trainAnchorOffset * (behind ? 1 : -1);
				sqrDist = (otherAnchorPos - currentAnchorPos).sqrMagnitude;
				if (sqrDist < bestDist)
				{
					bestDist = sqrDist;
					bestSegment = wagon.currentSegment;
					bestPoint = currentPointIndex;
					bestInversed = wagon.isInversedOnSegment;
					foundBetter = true;
				}
				else
				{
					break;
				}
			}

			wagon.isInversedOnSegment = bestInversed;
			wagon.currentSegment = bestSegment;
			currentSegment = TrackManager.instance.segments[wagon.currentSegment];
			wagon.transform.position = currentSegment.points[bestPoint].position;
			wagon.SetHeading(currentSegment.points[bestPoint].tangent * (wagon.isInversedOnSegment ? -1 : 1));

			wagon.distanceAlongSegment = 0;
			for (int i = 0; i < bestPoint; i++)
			{
				wagon.distanceAlongSegment += currentSegment.points[i].nextDist;
			}
		}

		//wagon in front
		for (int i = mainWagonIndex - 1; i >= 0; i--)
		{
			UpdateWagonPosition(wagons[i], i, wagons[i + 1], false);
		}

		//wagons behind
		for (int i = mainWagonIndex + 1; i < wagons.Count; i++)
		{
			UpdateWagonPosition(wagons[i], i, wagons[i - 1], true);
		}
	}

	public void SelectSwitchTrack(Wagon wagon, TrackSegment nextSegment)
	{
		wagon.currentSegment = TrackManager.instance.segments.IndexOf(nextSegment);
	}

	public struct SolvedCollision
	{
		public Train a, b;
	}

	public void ResolveCollisions(List<SolvedCollision> solvedCollisions)
	{
		//foreach(Wagon wagon in wagons)
		for (int i = 0; i < wagons.Count; i++)
		{
			Wagon wagon = wagons[i];
			List<Collider2D> results = new List<Collider2D>();
			ContactFilter2D a = new ContactFilter2D();
			a.NoFilter();
			wagon.collider.OverlapCollider(a, results);
			foreach (Collider2D collider in results)
			{
				Wagon otherWagon = collider.GetComponent<Wagon>();
				if (!otherWagon || otherWagon.train == wagon.train) continue;
				//TODO check if collision is at one of the ends of the wagon and is on the same track
				if(otherWagon && otherWagon.train.controller == -1)
				{
					//TODO this wont work for when the train move into us, needs some relative velocity
					if(speed < 0)
					{
						AddWagonBack(otherWagon);
					}
					else
					{
						AddWagonFront(otherWagon);
					}
				}
				else
				{
					if (solvedCollisions.Any(x => (x.a == wagon.train && x.b == otherWagon.train) || (x.a == otherWagon.train && x.b == wagon.train)))
						continue;

					Vector2 velocity1 = wagon.train.speed * wagon.transform.right;
					Vector2 velocity2 = otherWagon.train.speed * otherWagon.transform.right;
					Vector2 relativeVelocity = velocity1 - velocity2;

					float tmpSpeed1 = wagon.train.speed;
					float tmpSpeed2 = otherWagon.train.speed;

					wagon.train.speed *= -1;
					otherWagon.train.speed *= -1;

					otherWagon.train.UpdatePositions();
					wagon.train.UpdatePositions();

					wagon.train.speed = tmpSpeed1;
					otherWagon.train.speed = tmpSpeed2;

					otherWagon.train.speed += Vector2.Dot(otherWagon.transform.right, (relativeVelocity * wagon.train.totalMass / otherWagon.train.totalMass));
					wagon.train.speed -= Vector2.Dot(wagon.transform.right, (relativeVelocity * otherWagon.train.totalMass / wagon.train.totalMass));

					otherWagon.train.UpdatePositions();
					otherWagon.train.UpdatePositions();
					wagon.train.UpdatePositions();
					wagon.train.UpdatePositions();

					solvedCollisions.Add(new SolvedCollision { a=wagon.train, b=otherWagon.train });
				}
			}
		}
	}

	public void UpdateTotalMass()
	{
		totalMass = 0;
		foreach (Wagon wagon in wagons)
		{
			totalMass += wagon.mass;
			if (wagon.cargo != null)
			{
				totalMass += wagon.cargo.GetMass();
			}
		}
	}

	public void AddWagonBack(Wagon wagon)
	{
		wagon.train?.RemoveWagon(wagon);
		wagons.Add(wagon);
		wagon.SetTrain(this);
		UpdateTotalMass();
	}

	public void AddWagonFront(Wagon wagon)
	{
		wagon.train?.RemoveWagon(wagon);
		wagons.Insert(0, wagon);
		wagon.SetTrain(this);
		UpdateTotalMass();
	}

	public void RemoveWagon(Wagon wagon)
	{
		wagons.Remove(wagon);
		wagon.SetTrain(null);
		UpdateTotalMass();
	}
}
