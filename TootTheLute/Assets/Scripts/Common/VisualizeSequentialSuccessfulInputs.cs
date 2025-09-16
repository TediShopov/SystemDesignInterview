using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Diagnostics;

public class VisualizeSequentialSuccessfulInputs : MonoBehaviour
{
    public PlayerController2D Player;

    //Objects to signalize player successful sequntial input
    //Objects are for now simpy update
    public List<GameObject> SignalObjects;

    public GameObject FinalSignalObject;

    private void Update()
    {
        int successes = Player.SuccessfulBeatClicksInSuccession;
        //Iterate over all the signal object
        for (int i = 0; i < SignalObjects.Count; i++) 
        {
            //If their index is less that the success inputs - 1
            if(i < successes)
            {
                SignalObjects[i].SetActive(true);
            }
            else
            {
                SignalObjects[i].SetActive(false);

            }
        }
        if(SignalObjects.Count <= successes)
        {
            FinalSignalObject.SetActive(true);
        }
        else { FinalSignalObject.SetActive(false); }
    }
}
