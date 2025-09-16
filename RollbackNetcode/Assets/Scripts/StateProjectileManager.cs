using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateProjectileManager : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> ProjectilesInState;

    private void Awake()
    {
            
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var proj in ProjectilesInState)
        {

        }
    }
}
