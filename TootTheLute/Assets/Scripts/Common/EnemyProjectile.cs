using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public int Damage;
    public float MaxSpeed= 5;
    public Vector2 Direction = Vector2.zero;
    public void FixedUpdate()
    {
        this.transform.position += (Vector3)(Direction * MaxSpeed*Time.fixedDeltaTime);
         
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(this.gameObject);
    }


}

