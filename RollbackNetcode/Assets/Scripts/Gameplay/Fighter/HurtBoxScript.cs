using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBoxScript : MonoBehaviour
{
    public int Damage=5;
    //Possible Add Delay, Blockable and other attack properties


    private void Awake()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        gameObject.SetActive(false);
        var fighter = collision.GetComponent<HealthScript>();
        fighter?.TakeDamage(Damage);
    }
}
