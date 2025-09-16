using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TutorialProximitySign : MonoBehaviour
{
    public GameObject Object; 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(Object != null)
            Object.SetActive(true);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(Object != null)
            Object.SetActive(false);
        
    }


}
