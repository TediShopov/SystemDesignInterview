using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rollback : MonoBehaviour
{
    public FighterController playerRB;
    public FighterController enemyRB;

    public bool RollackActive = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    void ResimulateFramesForFighter(FighterController fighter,int frames) 
    {
        Debug.LogError($"Resimulating from{FrameLimiter.Instance.FramesInPlay} " +
            $" {fighter.InputBuffer.BufferedInput.Peek().FrameStamp} Frames");
        fighter.ResimulateInput(
            fighter.InputBuffer);


        for (int i = 0; i < frames; i++)
        {
            fighter.InputBuffer.BufferedInput.Dequeue();
        }

    }


    // Update is called once per frame
    void Update()
    {
        
    }

     
}
