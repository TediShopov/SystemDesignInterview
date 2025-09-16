using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activatable : MonoBehaviour
{
    public virtual void Activate()
    {
        Debug.Log("Activatable");
    }
}
