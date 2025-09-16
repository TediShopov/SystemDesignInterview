using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraStickToObject : MonoBehaviour
{
    public GameObject FollowObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Maintain the z position of the camera -
        float zPosition = this.transform.position.z;
        this.transform.position = new Vector3(FollowObject.transform.position.x,FollowObject.transform.position.y ,zPosition);
        
    }
}
