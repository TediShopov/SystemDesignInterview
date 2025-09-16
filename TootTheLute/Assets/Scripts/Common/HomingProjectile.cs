using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Homing projectile that seeks a target.
//If no target is supplied a direction will be used as fallback functionality
public class HomingProjectile : BoidBase
{
    public Collider2D Target;
    public Vector2 FallbackDirection;
    public bool IsMarkedForDestruction = false;
    //private Seek SeekBehaviour = new Seek();
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Velocity = Vector2.zero;
        this.Behaviors.Add(new Seek());
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            IsMarkedForDestruction=true;
        }
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (IsMarkedForDestruction)
            Destroy(this.gameObject);
        if (rb == null)
            return;
        if (Target == null || Target.enabled == false)
        {
            Debug.Log($"Fallback Direction Homing Projectile {FallbackDirection.ToString()} ") ;
            ApplySteering(FallbackDirection);
            return;

        }

        Vector2 steering = CalculateSteering(this.Target.ClosestPoint(this.transform.position));
        if (steering.magnitude > 0.05f)
        {
            FallbackDirection = steering.normalized;
        }
        ApplySteering(steering);


    }
    // public Vector2 CalculateSteering( Vector2 target)
    // {
    //     Vector2 finalSteering = Vector2.zero;
    //     foreach (SteeringBehavior behavior in Behaviors) 
    //     {
    //         finalSteering += behavior.CalculateSteering(this, target) * behavior.Weight;
    //     }
    //     return Vector2.ClampMagnitude(finalSteering, this.maxForce);
    // }

    // public void ApplySteering(Vector2 force)
    // {
    //     Vector2 acceleration = Vector2.ClampMagnitude(force, maxForce);
    //     velocity = Vector2.ClampMagnitude(velocity + acceleration, maxSpeed);
    //     rb.velocity = velocity;
    // }
}
