using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Seek : SteeringBehavior
{
    public override Vector2 CalculateSteering(BoidBase boid, Vector2 target)
    {
        Vector2 desired = (target - (Vector2)boid.transform.position).normalized * boid.MaxSpeed;
        return desired - boid.GetComponent<Rigidbody2D>().velocity;
    }

}
