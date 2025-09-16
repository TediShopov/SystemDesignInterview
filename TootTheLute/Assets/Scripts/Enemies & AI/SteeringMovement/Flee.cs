using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class Flee : SteeringBehavior
{
    public float ReqDistance = 5;
    private BoidBase _pboid;
    private Vector2 desired;

    public override Vector2 CalculateSteering(BoidBase boid, Vector2 target)
    {
        _pboid = boid;
        //Same as seek but inverted

        //Inactive after a certian range
        float d = Vector2.Distance(boid.transform.position, target);
        Vector2 desired = -boid.GetVelocity().normalized;
        //Vector2 desired = Vector2.zero;
       if(d < ReqDistance)
        {
            desired = ( (Vector2)boid.transform.position - target).normalized * boid.MaxSpeed;

        }
        else
        {
            desired = (target - (Vector2)boid.transform.position).normalized * boid.MaxSpeed;

        }

        return desired;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(_pboid.gameObject.transform.position, desired);

    }
}
