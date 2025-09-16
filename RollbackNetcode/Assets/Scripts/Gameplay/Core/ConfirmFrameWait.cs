//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Threading;
//using UnityEngine;

//public class ConfirmFrameWait : MonoBehaviour
//{

//    private static bool WaitForConfirmFrame=false;
//    public static bool IsEnabled=true;

//    static void OnBufferUpdate(InputFrame frame) 
//    {
//        //Buffer Updating
//        WaitForConfirmFrame= FrameLimiter.Instance.FramesInPlay >
//               GetConfirmFrame(StaticBuffers.Instance.PlayerBuffer,
//                               StaticBuffers.Instance.EnemyBuffer);
       
       
//    }

//    static int GetConfirmFrame(InputBuffer b, InputBuffer b2)
//    {
//        if (b2.LastFrame==null && b.LastFrame==null )
//        {
//            return -1;
//        }
//        return b2.LastFrame.FrameStamp;
//        ////Have to be algigned perfectly 
//        //if (b.Peek().FrameStamp != b2.Peek().FrameStamp)
//        //{
//        //    return -1;
//        //}
//        //else
//        //{
//        //   return b2.LastFrame.FrameStamp;
//        //}
       

//    }

   

//    private TimeSpan _indefiniteWaitTime=new TimeSpan(0,0,0,0,-1);

//     void Awake()
//    {
//        StaticBuffers.Instance.EnemyBuffer.OnInputFrameAdded+=OnBufferUpdate;
//        StaticBuffers.Instance.PlayerBuffer.OnInputFrameAdded += OnBufferUpdate;
//        IsEnabled = !ClientData.SoloPlay;
//    }
//    void Update()
//    {

      
//        //If next frame is bigger than the confirm frame
//        //not enough input to proceed simulation
//        if (!ClientData.GameState == GameState.Paused)
//        {
//            if (WaitForConfirmFrame && IsEnabled)
//            {
            
//                //Wait till frames in play
//                SpinWait.SpinUntil((() => { return (!WaitForConfirmFrame || !IsEnabled); }), _indefiniteWaitTime);

//            }
//        }
//    }

//}
