using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChecksumChecker : MonoBehaviour
{
    private InputBuffer BufferedInputFramesCurrent;
    private InputBuffer BufferedInputFramesOther;
    private StateManager stateManager;
    public ChecksumVisualizer checksumVisualizer;

    private void Start()
    {
        BufferedInputFramesCurrent = new InputBuffer();
        BufferedInputFramesCurrent.DelayInput = 150;

        BufferedInputFramesOther = new InputBuffer();
        BufferedInputFramesOther.DelayInput = 150;
        FighterBufferMono = StaticBuffers.Instance.Player.GetComponent<FighterBufferMono>();
        FighterBufferMono.InputFrameOnAppendedFromKeyboard += AttachToPlayerCheckInput;
        NetworkGamePacket.OnNetworkGamePacketReceived += AttachToEnemyCkeckInput;
    }
    private FighterBufferMono FighterBufferMono;
    public void AssignToNewPlayerObject()
    {
        var currentFighterBufferMono = StaticBuffers.Instance.Player.GetComponent<FighterBufferMono>();
        if (currentFighterBufferMono != FighterBufferMono)
        {
            Debug.Log("Assigned To New");
            FighterBufferMono = currentFighterBufferMono;
            FighterBufferMono.InputFrameOnAppendedFromKeyboard += AttachToPlayerCheckInput;
        }
    }

    private void AttachToEnemyCkeckInput(object e, InputFramePacket packet)
    {
        if (packet.FrameStamp < 5) return;
        BufferedInputFramesOther.Enqueue(
        new InputFrame(packet.InputElements, packet.FrameStamp, packet.Checksum));
    }

    private void AttachToPlayerCheckInput(InputFrame inputFrame)
    {
        if (inputFrame.FrameStamp < 5) return;
        BufferedInputFramesCurrent.Enqueue(inputFrame);
    }
    public bool checksumsMatchingLastFrame = true;
    public int LastSeenStateFrame = 0;

    // Update is called once per frame
    private void FixedUpdate()
    {
        AssignToNewPlayerObject();
        //Return if buffered inputs on either player are null
        // or not initialized
        if (BufferedInputFramesCurrent == null || BufferedInputFramesOther == null) { return; }
        if (BufferedInputFramesCurrent.IsEmpty || BufferedInputFramesOther.IsEmpty) { return; }

        var currentFrame = BufferedInputFramesCurrent.Peek();
        InputFrame otherFrame = BufferedInputFramesOther.Peek();
        //When scheduled for the same frame and not predicted
        //compate timestamps
        if (currentFrame.FrameStamp == otherFrame.FrameStamp)
        {
            bool checksumsMatchingThisFrame = currentFrame.Checksum == otherFrame.Checksum;
            checksumVisualizer.VizualizChecksums(currentFrame, otherFrame);

            BufferedInputFramesCurrent.Dequeue();
            BufferedInputFramesOther.Dequeue();

            checksumsMatchingLastFrame = checksumsMatchingThisFrame;
            LastSeenStateFrame++;
            if (checksumsMatchingThisFrame == false)
            {
                //If there is a chekcsum mismatch,
                //game is online and checkum is not predicted
                if (ClientData.SoloPlay == false
                    && ClientData.GameState == GameState.Runing
                    )
                {
                    //Terminate the game as soon as possible
                    ClientData.GameState = GameState.Finished;
                }
            }
        }
    }
}