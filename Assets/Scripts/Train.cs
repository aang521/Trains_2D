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

    private void GetNextTrack(Wagon wagon, float excessDistance)
	{
        int NextSegment = 0;
        if (excessDistance > 0)
		{
            Vector2 currentEnd = TrackManager.instance.segments[wagon.currentSegment].points.Last().position;

            //If there is a switch at the end of the current segment
            if (TrackManager.instance.segments[wagon.currentSegment].Next.Count == 2)
            {
                if(controller == -1)
				{
                    NextSegment = TrackManager.instance.segments.IndexOf(TrackManager.instance.segments[wagon.currentSegment].Next[0]);
                }
                else if (Input.GetAxis("direction" + controller) < -0.5 || (controller == 0 && Input.GetKey(KeyCode.A)))
                {
                    NextSegment = TrackManager.instance.segments.IndexOf(TrackManager.instance.segments[wagon.currentSegment].Next[0]);
                }
                else if (Input.GetAxis("direction" + controller) > 0.5 || (controller == 0 && Input.GetKey(KeyCode.D)))
                {
                    NextSegment = TrackManager.instance.segments.IndexOf(TrackManager.instance.segments[wagon.currentSegment].Next[1]);
                }
                else
                {
                    NextSegment = TrackManager.instance.segments.IndexOf(TrackManager.instance.segments[wagon.currentSegment].Next[0]);
                }

            }
            else if (TrackManager.instance.segments[wagon.currentSegment].Next.Count == 3)
            {
                if (controller == -1)
                {
                    NextSegment = TrackManager.instance.segments.IndexOf(TrackManager.instance.segments[wagon.currentSegment].Next[0]);
                }
                else if (Input.GetAxis("direction" + controller) < -0.5 || (controller == 0 && Input.GetKey(KeyCode.A)))
                {
                    NextSegment = TrackManager.instance.segments.IndexOf(TrackManager.instance.segments[wagon.currentSegment].Next[0]);
                }
                else if (Input.GetAxis("direction" + controller) > 0.5 || (controller == 0 && Input.GetKey(KeyCode.D)))
                {
                    NextSegment = TrackManager.instance.segments.IndexOf(TrackManager.instance.segments[wagon.currentSegment].Next[2]);
                }
                else
                {
                    NextSegment = TrackManager.instance.segments.IndexOf(TrackManager.instance.segments[wagon.currentSegment].Next[1]);
                }
            }
            //If the current segment is a merge with another track
            else if (TrackManager.instance.segments[wagon.currentSegment].Next.Count == 1)
            {
                NextSegment = TrackManager.instance.segments.IndexOf(TrackManager.instance.segments[wagon.currentSegment].Next[0]);
            }

            wagon.currentSegment = NextSegment;

            Vector2 endA = TrackManager.instance.segments[NextSegment].points[0].position;
            Vector2 endB = TrackManager.instance.segments[NextSegment].points.Last().position;

            float distA = Vector2.Distance(currentEnd, endA);
            float distB = Vector2.Distance(currentEnd, endB);

            if (distA > distB)
                wagon.isInversedOnSegment = !wagon.isInversedOnSegment;
        }
		else
		{
            Vector2 currentEnd = TrackManager.instance.segments[wagon.currentSegment].points[0].position;

            //If there is a switch at the end of the current segment
            if (TrackManager.instance.segments[wagon.currentSegment].Prev.Count == 2)
            {
                if(controller == -1)
				{
                    NextSegment = TrackManager.instance.segments.IndexOf(TrackManager.instance.segments[wagon.currentSegment].Prev[0]);
                }
                else if (Input.GetAxis("direction" + controller) < -0.5 || (controller == 0 && Input.GetKey(KeyCode.A)))
                {
                    NextSegment = TrackManager.instance.segments.IndexOf(TrackManager.instance.segments[wagon.currentSegment].Prev[0]);
                }
                else if (Input.GetAxis("direction" + controller) > 0.5 || (controller == 0 && Input.GetKey(KeyCode.D)))
                {
                    NextSegment = TrackManager.instance.segments.IndexOf(TrackManager.instance.segments[wagon.currentSegment].Prev[1]);
                }
                else
                {
                    NextSegment = TrackManager.instance.segments.IndexOf(TrackManager.instance.segments[wagon.currentSegment].Prev[0]);
                }

            }
            else if (TrackManager.instance.segments[wagon.currentSegment].Prev.Count == 3)
            {
                if (controller == -1)
                {
                    NextSegment = TrackManager.instance.segments.IndexOf(TrackManager.instance.segments[wagon.currentSegment].Prev[0]);
                }
                else if (Input.GetAxis("direction" + controller) < -0.5 || (controller == 0 && Input.GetKey(KeyCode.A)))
                {
                    NextSegment = TrackManager.instance.segments.IndexOf(TrackManager.instance.segments[wagon.currentSegment].Prev[0]);
                }
                else if (Input.GetAxis("direction" + controller) > 0.5 || (controller == 0 && Input.GetKey(KeyCode.D)))
                {
                    NextSegment = TrackManager.instance.segments.IndexOf(TrackManager.instance.segments[wagon.currentSegment].Prev[2]);
                }
                else
                {
                    NextSegment = TrackManager.instance.segments.IndexOf(TrackManager.instance.segments[wagon.currentSegment].Prev[1]);
                }
            }
            //If the current segment is a merge with another track
            else if (TrackManager.instance.segments[wagon.currentSegment].Prev.Count == 1)
            {
                NextSegment = TrackManager.instance.segments.IndexOf(TrackManager.instance.segments[wagon.currentSegment].Prev[0]);
            }

            wagon.currentSegment = NextSegment;

            Vector2 endA = TrackManager.instance.segments[NextSegment].points[0].position;
            Vector2 endB = TrackManager.instance.segments[NextSegment].points.Last().position;

            float distA = Vector2.Distance(currentEnd, endA);
            float distB = Vector2.Distance(currentEnd, endB);

            if(distA < distB)
                wagon.isInversedOnSegment = !wagon.isInversedOnSegment;
        }
    }

    public void UpdatePositions()
    {
        int mainWagonIndex = wagons.FindIndex(x => x.isLocomotive);
        if (mainWagonIndex == -1)
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

                    GetNextTrack(mainWagon, mainWagon.distanceAlongSegment);
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

                    GetNextTrack(mainWagon, mainWagon.distanceAlongSegment);
                    currentSegment = TrackManager.instance.segments[mainWagon.currentSegment];

                    if (mainWagon.isInversedOnSegment != prevInversed)
                    	mainWagon.distanceAlongSegment = currentSegment.length - distanceIntoNext;
					else
						mainWagon.distanceAlongSegment = distanceIntoNext;
                }
            }

			float remaining = mainWagon.distanceAlongSegment;
            int currentPointIndex = 0;
            while (remaining > 0)
            {
                remaining -= currentSegment.points[currentPointIndex].nextDist;
                currentPointIndex++;
            }
            mainWagon.transform.position = currentSegment.points[currentPointIndex].position;
            mainWagon.SetHeading(currentSegment.points[currentPointIndex].tangent * (mainWagon.isInversedOnSegment ? -1 : 1));
        }

        void UpdateWagonPosition(Wagon wagon, int wagonIndex, Wagon otherWagon, bool behind)
        {
            Vector2 otherAnchorPos = (Vector2)otherWagon.transform.position + (Vector2)otherWagon.transform.right * trainSettings.trainAnchorOffset * (behind ? -1 : 1);


            var currentSegment = TrackManager.instance.segments[wagon.currentSegment];
            int currentPointIndex = 0;

            float remaining = wagon.distanceAlongSegment;
            while(remaining > 0)
            {
                remaining -= currentSegment.points[currentPointIndex].nextDist;
                currentPointIndex++;
                if (currentPointIndex >= currentSegment.points.Length)
				{
                    currentPointIndex = 0;
                    return;
				}
            }

            if (speed * (wagon.isInversedOnSegment ? -1 : 1) > 0)
                currentPointIndex--;
            else
                currentPointIndex++;
            if (currentPointIndex < 0)
            {
                GetNextTrack(wagon, -1);
                currentSegment = TrackManager.instance.segments[wagon.currentSegment];

                currentPointIndex = !wagon.isInversedOnSegment ? currentSegment.points.Length - 1 : 0;
            }
            if (currentPointIndex >= currentSegment.points.Length)
            {
                GetNextTrack(wagon, 1);
                currentSegment = TrackManager.instance.segments[wagon.currentSegment];

                currentPointIndex = !wagon.isInversedOnSegment ? 0 : currentSegment.points.Length - 1;
            }

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
                    GetNextTrack(wagon, -1);
                    currentSegment = TrackManager.instance.segments[wagon.currentSegment];

                    currentPointIndex = !wagon.isInversedOnSegment ? currentSegment.points.Length-1 : 0;
                }
                if(currentPointIndex >= currentSegment.points.Length)
				{
                    GetNextTrack(wagon, 1);
                    currentSegment = TrackManager.instance.segments[wagon.currentSegment];

                    currentPointIndex = !wagon.isInversedOnSegment ? 0 : currentSegment.points.Length - 1;
                }

                currentAnchorPos = currentSegment.points[currentPointIndex].position + currentSegment.points[currentPointIndex].tangent * trainSettings.trainAnchorOffset * (behind ? 1 : -1);
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

    public void ResolveCollisions()
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
