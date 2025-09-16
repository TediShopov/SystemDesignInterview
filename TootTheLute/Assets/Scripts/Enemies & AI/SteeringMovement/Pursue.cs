using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pursue : SteeringBehavior
{
    public override Vector2 CalculateSteering(BoidBase boid, Vector2 target)
    {
        Vector2 targetVelocity = target - (Vector2)boid.transform.position;
        float predictionTime = targetVelocity.magnitude / boid.MaxSpeed;
        Vector2 predictedPosition = target + targetVelocity * predictionTime;

        return new Seek().CalculateSteering(boid, predictedPosition);
    }
}
