using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConeUtils2D
{
    // Returns N evenly spaced unit vectors between minAngle and maxAngle around baseDirection
    public static List<Vector2> EvenlySpacedDirectionsInCone2D(Vector2 baseDirection, float minAngleDeg, float maxAngleDeg, int count)
    {
        baseDirection.Normalize();

        List<Vector2> directions = new List<Vector2>(count);

        // Base angle in degrees from Vector2.right
        float baseAngle = Vector2.SignedAngle(Vector2.right, baseDirection);

        // Even step between min and max
        float step = (maxAngleDeg - minAngleDeg) / Mathf.Max(count - 1, 1);

        for (int i = 0; i < count; i++)
        {
            float angleOffset = minAngleDeg + step * i;
            float finalAngle = baseAngle + angleOffset;
            float rad = finalAngle * Mathf.Deg2Rad;

            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            directions.Add(dir.normalized);
        }

        return directions;
    }
}
public class SlimeSpawner : MonoBehaviour
{
    private Vector2 baseDirection = Vector2.right;
    //The projectile/droplet that would come out a the slime when dying and spawn the new slime 
    public GameObject SlimeDroplet;
    public float MinConeAngle = -15;
    public float MaxConeAngle = 15;
    public float SpawnRadius;
    public int DropletCount;

    void Start()
    {
        Spawn();
    }

    [ContextMenu("Spawn Now")]
    void Spawn()
    {
       List<Vector2> directions =  ConeUtils2D.EvenlySpacedDirectionsInCone2D(Vector2.right, MinConeAngle, MaxConeAngle, DropletCount);
        foreach (Vector2 direction in directions)
        {
            Vector2 worldPos = (Vector2)this.transform.position+ direction *SpawnRadius;   
            Instantiate(SlimeDroplet,worldPos, Quaternion.identity,null);
        }
    }

}
