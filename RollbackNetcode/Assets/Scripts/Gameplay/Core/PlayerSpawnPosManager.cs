using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPosManager : MonoBehaviour
{


    public Transform LeftSpawnPos;
    public Transform RightSpawnPos;

    


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"Character Index is {ClientData.CharacterIndex}");
        //If 0 default left side start
        if (ClientData.CharacterIndex == false)
        {
            StaticBuffers.Instance.Player.transform.position = LeftSpawnPos.position;
            StaticBuffers.Instance.PlayerRB.transform.position = LeftSpawnPos.position;

            StaticBuffers.Instance.Enemy.transform.position = RightSpawnPos.position;
            StaticBuffers.Instance.EnemyRB.transform.position = RightSpawnPos.position;


        }
        else
        {
            StaticBuffers.Instance.Enemy.transform.position = LeftSpawnPos.position;
            StaticBuffers.Instance.EnemyRB.transform.position = LeftSpawnPos.position;


            StaticBuffers.Instance.Player.transform.position = RightSpawnPos.position;
            StaticBuffers.Instance.PlayerRB.transform.position = RightSpawnPos.position;

        }


    }

   
}
