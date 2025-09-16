using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidance : SteeringBehavior
{
    public BoidBase BoidInstance;
     [Header("Obstacle Avoidance Settings")]
    [Tooltip("Number of rays to cast in a cone pattern.")]
    public int numberOfRays = 7;
    
    [Tooltip("Total cone angle (in degrees) for ray casting.")]
    public float coneAngle = 90f;
    
    [Tooltip("Maximum distance for detecting obstacles.")]
    public float detectionDistance = 5f;
    
    [Tooltip("Multiplier for the repel force.")]
    public float repelForceMultiplier = 1f;
    
    [Tooltip("Layer mask for obstacles.")]
    public LayerMask obstacleLayer;


    public override Vector2 CalculateSteering(BoidBase boid, Vector2 target)
    {
        BoidInstance = boid;
        var repel = GetRepelForce();
        return repel*boid.MaxForce;
    }
    public Vector2 GetRepelForce()
    {
        if (BoidInstance == null) return Vector2.zero;
        Vector2 totalRepelForce = Vector2.zero;
        Vector2 initialRayDirection = BoidInstance.GetVelocity();
        
        // Calculate the starting angle and the step between rays.
        float halfCone = coneAngle * 0.5f;
        float angleStep = numberOfRays > 1 ? coneAngle / (numberOfRays - 1) : 0f;
        
        // Iterate through each ray.
        for (int i = 0; i < numberOfRays; i++)
        {
            // Calculate the angle offset for this ray.
            float angleOffset = -halfCone + i * angleStep;
            
            // Compute the direction by rotating the forward direction (here assumed to be transform.right)
            Vector2 rayDirection = Quaternion.Euler(0, 0, angleOffset) * initialRayDirection;
            
            // Cast the ray
            RaycastHit2D hit = Physics2D.Raycast(BoidInstance.transform.position, rayDirection, detectionDistance, obstacleLayer);
            if (hit.collider != null)
            {
                // Calculate a factor that increases as the hit gets closer.
                float closenessFactor = (detectionDistance - hit.distance) / detectionDistance;
                // The repel force is opposite to the ray's direction
                Vector2 repelForce = -rayDirection * closenessFactor * repelForceMultiplier;
                totalRepelForce += repelForce;

                //// Debug: draw the ray in red up to the hit point.
                //Debug.Log("Drawing Ray");
                //Debug.DrawRay(BoidInstance.transform.position, rayDirection * hit.distance, Color.red);
            }
            else
            {

                //Debug.Log("Drawing Ray");
                //// Debug: draw the ray in green when nothing is hit.
                //Debug.DrawRay(BoidInstance.transform.position, rayDirection * detectionDistance, Color.green);
            }
        }
        
        return totalRepelForce;
    }

    public override void OnDrawGUI(BoidBase boid)
    {
        Vector2 initialRayDirection = boid.GetVelocity();
        // Calculate half the cone angle and the step between each ray.
        float halfCone = coneAngle * 0.5f;
        float angleStep = numberOfRays > 1 ? coneAngle / (numberOfRays - 1) : 0f;

        // Iterate through each ray in the cone.
        for (int i = 0; i < numberOfRays; i++)
        {
            // Calculate the current angle offset.
            float angleOffset = -halfCone + i * angleStep;

            // Compute the ray direction by rotating the object's forward (assumed transform.right) by the angle offset.
            Vector2 rayDirection = Quaternion.Euler(0, 0, angleOffset) * initialRayDirection;

            // Perform the raycast.
            RaycastHit2D hit = Physics2D.Raycast(BoidInstance.transform.position, rayDirection, detectionDistance, obstacleLayer);
            if (hit.collider != null)
            {
                // Draw red line to the hit point and a small sphere at the impact location.
                Gizmos.color = Color.red;
                Gizmos.DrawLine(BoidInstance.transform.position, hit.point);
                Gizmos.DrawSphere(hit.point, 0.1f);
            }
            else
            {
                // Draw green line for rays that do not hit any obstacles.
                Gizmos.color = Color.green;
                Gizmos.DrawLine(BoidInstance.transform.position, (Vector2)BoidInstance.transform.position + rayDirection * detectionDistance);
            }
        }
    }
}
