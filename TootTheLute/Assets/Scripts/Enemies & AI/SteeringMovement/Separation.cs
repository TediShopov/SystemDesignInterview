using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Separation : SteeringBehavior
{
    public List<Collider2D> neighbors;
    public float separationDistance = 2f;
    public float detectionRadius = 5f;
    public int updateNeighboursEachXTicks = 10;
    public int neighbourTickUpdateCount = 10;
    public float disregardCollidersToCloseToTarget = 0.5f;
    public LayerMask AvoidLayer;

    public bool  TooCloseToTarget(Vector2 pos, Vector2 target)
    {
        return Vector2.Distance(pos, target) < disregardCollidersToCloseToTarget;

    }
    public List<Collider2D> GetNeighbors(BoidBase boid,Vector2 target)
    {
        Collider2D[] neighborColliders = Physics2D.OverlapCircleAll(boid.gameObject.transform.position, this.detectionRadius, AvoidLayer);
        var colliderList = neighborColliders.Where(
            x =>
            x.isTrigger == false // Not a trigger
            && x.gameObject != boid.gameObject // Not self
            && TooCloseToTarget(x.gameObject.transform.position, target) == false
            ).ToList();
        
       // List<BoidBase> neighbors = new List<BoidBase>();
       // foreach (Collider2D c in neighborColliders)
       // {
       //     BoidBase otherBoid =  c.gameObject.GetComponent<BoidBase>();
       //     if (otherBoid == null)
       //         continue;
       //     if (otherBoid == boid)
       //         continue;
       //     neighbors.Add(otherBoid);
       //     Debug.Log("Found neighbor: " + otherBoid.name);
       // }
        return colliderList;

    }
    public override Vector2 CalculateSteering(BoidBase boid, Vector2 target)
    {
        neighbourTickUpdateCount++;
        if(neighbourTickUpdateCount >= updateNeighboursEachXTicks)
        {
            Debug.Log("Updated Neighbours");
            this.neighbors = GetNeighbors(boid,target);
            neighbourTickUpdateCount = 0;
        }


        if(neighbors.Count <= 0) { return Vector2.zero; }

        Vector2 repulsion = Vector2.zero;
        foreach (Collider2D neighbor in neighbors)
        {
            Vector2 diff = (Vector2)boid.transform.position - (Vector2)neighbor.transform.position;
            if (diff.magnitude < separationDistance)
                repulsion += diff.normalized / diff.magnitude;
        }
        return repulsion.normalized * boid.MaxSpeed;
    }
}
