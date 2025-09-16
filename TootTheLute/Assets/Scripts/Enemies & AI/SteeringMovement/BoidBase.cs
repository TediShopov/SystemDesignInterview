using System.Collections.Generic;
using System.Security;
using Unity.VisualScripting;
using UnityEngine;

     [System.Serializable]
    public struct BehaviorWeight
    {
        public SteeringBehavior behavior;
        public float weight;
    }
public class BoidBase : MonoBehaviour
{
    public float MaxSpeed = 5f;
    public float MaxForce = 2f;
    public float DebugMagnitudeMultiplier = 4.0f;

    public Vector2 Velocity;
    public LayerMask Layer;

    protected Rigidbody2D rb;

    [SerializeReference]
    public List<SteeringBehavior> Behaviors = new List<SteeringBehavior>();

    public Vector2 GetVelocity() => Velocity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Velocity = Vector2.zero;

    }



    public Vector2 CalculateSteering( Vector2 target)
    {
        Vector2 finalSteering = Vector2.zero;
        foreach (SteeringBehavior behavior in Behaviors) 
        {
            finalSteering += behavior.CalculateSteering(this, target) * behavior.Weight;
            
        }
        return Vector2.ClampMagnitude(finalSteering, this.MaxForce);
    }

    public void ApplySteering(Vector2 force)
    {
        Vector2 acceleration = Vector2.ClampMagnitude(force, MaxForce);
        Velocity = Vector2.ClampMagnitude(Velocity + acceleration, MaxSpeed);
        rb.velocity = Velocity;
    }
    private void OnDrawGizmos()
    {
        foreach (SteeringBehavior behavior in Behaviors) 
        {
            behavior.OnDrawGUI(this) ;
        }
    }
}
