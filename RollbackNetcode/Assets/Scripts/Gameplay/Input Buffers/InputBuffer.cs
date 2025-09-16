using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

//[StructLayout(LayoutKind.Explicit, Size = 4)]
//public struct InputElement 
//{
//    [FieldOffset(0)]
//    public KeyCode key;     //4 bytes
//}


public class InputFrame 
{
    public  const Int32 DEFAULT_CHECKSUM = 0;
    //public InputElement[] _inputInFrame;
    public byte[] Inputs;
    public Int32 FrameStamp { get; set; }
    //The checksum of the simulation before the input is applied.
    public Int32 Checksum { get; set; }
    public GameStatePacket State { get; set; }
    public bool IsPredicted { get; set; }


    //int DelayInput = 0;

    public InputFrame()
    {
        this.FrameStamp = -1;
        this.Inputs = new byte[ClientData.AllowedKeys.Length];
        this.Checksum = DEFAULT_CHECKSUM;
        this.IsPredicted = false;
    }
    public void ClearInputs() 
    {
        for (int i = 0; i < this.Inputs.Length; i++)
            this.Inputs[i] = 0;

    }

   public static InputFrame CaptureInput(int delay) 
    {
        InputFrame captureInputs= new InputFrame();
        captureInputs.Inputs = new byte[ClientData.AllowedKeys.Length];
        //this.DelayInput = delay;
        for (int i = 0; i < ClientData.AllowedKeys.Length; i++)
        {
            if (Input.GetKey(ClientData.AllowedKeys[i]))
            {
                captureInputs.Inputs[i] = 255;
            }
            captureInputs.FrameStamp = FrameLimiter.Instance.FramesInPlay + delay;
        }
        return captureInputs;
    }

    public bool IsKey(KeyCode keyCode)
    {
        int index = ClientData.AllowedKeysIndex[keyCode];
        return Inputs[index] != 0;
        
    }


    public void SetKey(KeyCode code, bool b = true) 
    {
        int index = ClientData.AllowedKeysIndex[code];
        if(b)
            Inputs[index] = 255;
        else
            Inputs[index] = 0;
    }

    public InputFrame(byte[] inputs,Int32 timestamp,int checksum = DEFAULT_CHECKSUM, GameStatePacket state = new GameStatePacket())
    {
        this.FrameStamp = timestamp;
        this.Inputs = inputs;
        this.Checksum = checksum;
        this.State = state;
        this.IsPredicted = false;
        
    }

    public InputFrame(InputFramePacket packet)
    {

        this.FrameStamp = packet.FrameStamp;
        this.Checksum = packet.Checksum;
        //this.State = packet.State;
        this.Inputs = packet.InputElements;
        this.IsPredicted = false;
    }

}


public class InputBuffer
{
    //TODO make buffer only accpets keys that do sth in the game

    public Queue<InputFrame> BufferedInput { get; set; }
    public int DelayInput;

    public delegate void InputFrameDelegate(InputFrame inputFrame);  // delegate
    public event InputFrameDelegate OnInputFrameAdded; // event
    public event InputFrameDelegate OnInputFrameDiscarded; // event

    public Queue<KeyCode> PressedKeys;
    public HashSet<KeyCode> KeyDowned;
    public int PressedKeysMaxCount = 5;
    public int RefreshKeyPressedAfterFrames = 120;
    private int _framesPassedSinceKeyDown = 0;

    public bool IsEmpty => this.BufferedInput.Count <= 0;

    public bool IsOverflow => BufferedInput.Count >= DelayInput;



    public InputFrame LastFrame { get; set; }

    public InputBuffer()
    {
        BufferedInput = new Queue<InputFrame>();
        PressedKeys = new Queue<KeyCode>();
        KeyDowned = new HashSet<KeyCode>();
    }

    public InputBuffer(InputBuffer inputBuffer)
    {
        this.SetTo(inputBuffer);
    }




    public void SetTo(InputBuffer inputBuffer)
    {
        BufferedInput = new Queue<InputFrame>(inputBuffer.BufferedInput);
        PressedKeys = new Queue<KeyCode>(inputBuffer.PressedKeys);
        KeyDowned = new HashSet<KeyCode>(inputBuffer.KeyDowned);
        LastFrame = inputBuffer.LastFrame;
        this.PressedKeysMaxCount = inputBuffer.PressedKeysMaxCount;
        this.RefreshKeyPressedAfterFrames = inputBuffer.RefreshKeyPressedAfterFrames;
        this._framesPassedSinceKeyDown = inputBuffer._framesPassedSinceKeyDown;
        this.DelayInput = inputBuffer.DelayInput;
        //this.OnInputFrameAdded = inputBuffer.OnInputFrameAdded;
        //this.OnInputFrameDiscarded = inputBuffer.OnInputFrameDiscarded;
    }
    public void Clear()
    {
        BufferedInput.Clear();
        PressedKeys.Clear();
        KeyDowned.Clear();
        LastFrame = null;
        this._framesPassedSinceKeyDown = 0;

    }



    //Deque elements into a temporary stack until input frame with the same 
    // frame stamp is reached, then replace with new input frame and reinsert 
    // other elements in correct order
    public void RollbackEnqueue(InputFrame rollbackFrame = null)
    {
        Queue<InputFrame> Temp = new Queue<InputFrame>();


        InputFrame lastConfirmedInput = rollbackFrame;
        while (BufferedInput.Count > 0)
        {
            var inframe = this.BufferedInput.Dequeue();
            if(inframe.FrameStamp == rollbackFrame.FrameStamp) 
            {
                Temp.Enqueue(rollbackFrame);
                if (rollbackFrame.IsPredicted == false) 
                    lastConfirmedInput = rollbackFrame;
            }
            else if (inframe.IsPredicted)
            {
                //Use predicted
                //Repredict that 
                lastConfirmedInput.Inputs.CopyTo(inframe.Inputs, 0);
                Temp.Enqueue(inframe);
            }
            else
            {
                Temp.Enqueue(inframe);
                if (inframe.IsPredicted == false) 
                    lastConfirmedInput = inframe;
            }



        }

        //Reinsert previous input
        while (Temp.Count > 0)
        {
            this.BufferedInput.Enqueue(Temp.Dequeue());
        }
    }

    public void Enqueue(InputFrame inputFrame = null)
    {
        if (inputFrame == null)
        {
            inputFrame = InputFrame.CaptureInput(DelayInput);
            //Generate checksum or get 
            inputFrame.Checksum = StaticBuffers.Instance.StateManager.TryUpdateCheckRelevantChecksum();
            inputFrame.State = StaticBuffers.Instance.StateManager.GetGameState();
        }

        BufferedInput.Enqueue(inputFrame);
     
        LastFrame = inputFrame;

        ////Call on add event
        //RecordKeysDown(inputFrame);

        OnInputFrameAdded?.Invoke(inputFrame);
    }


    public InputFrame Dequeue() 
    {
        if (this.BufferedInput?.Count<=0)
        {
            return null;
        }
        OnInputFrameDiscarded?.Invoke(this.BufferedInput.Peek());
        //Call on add event
        RecordKeysDown(this.BufferedInput.Peek());
        return this.BufferedInput.Dequeue();
    }



    void RecordKeysDown(InputFrame frame) 
    {
        //Already takes into acount pririty of inputs
       
            for (int i = 0; i < ClientData.AllowedKeys.Length; i++)
            {
                var key = ClientData.AllowedKeys[i];
                var inputDown = frame.IsKey(key);
                if (inputDown)
                {
                    if (!KeyDowned.Contains(key))
                    {
                        KeyDowned.Add(key);
                    }
                }
                //K == 0
                else
                {
                    //On Release
                    if (KeyDowned.Contains(key))
                    {
                        if (PressedKeys.Count > PressedKeysMaxCount)
                        {
                            //Test out what is the error dequing empty q
                            PressedKeys.Dequeue();

                        }
                        KeyDowned.Remove(key);
                        PressedKeys.Enqueue(key);
                        _framesPassedSinceKeyDown = 0;
                        break;
                    }

                }
            }
        

    }
    public InputFrame Peek()
    {
        if (this.BufferedInput != null && this.BufferedInput.Count > 0)
        {
            return this.BufferedInput.Peek();
        }
        return new InputFrame();
    }

    public void OnUpdate() 
    
    {
        _framesPassedSinceKeyDown++;
        if (_framesPassedSinceKeyDown >= RefreshKeyPressedAfterFrames)
        {

            PressedKeys.Clear();
            _framesPassedSinceKeyDown = 0;
        }
    }



    public void DebugPrintKeysDown() 
    {
        string strKeyDownBuff = "Key Down Buffer";
        foreach (var el in this.PressedKeys)
        {
            strKeyDownBuff += el.ToString();
        }
        Debug.LogError(strKeyDownBuff);

    }
    public string GetInputBufferString()
    {
        
        string strAllInputBuff = $" Input Buffer({this.BufferedInput.Count}) ";
        foreach (var inputFrame in this.BufferedInput)
        {

            strAllInputBuff += $"TS: {inputFrame.FrameStamp.ToString()}, ";
            foreach (var key in ClientData.AllowedKeys)
            {
                if (inputFrame.IsKey(key))
                {
                    strAllInputBuff += key;
                }
               
            }
            strAllInputBuff += "  ";
        }
            strAllInputBuff += "  ";
            strAllInputBuff += $"this.LastFrame: {this.DebugPrintFrame(LastFrame)} DelayInput: {this.DelayInput} Frames Passed since key down: {this._framesPassedSinceKeyDown}";
        return strAllInputBuff;
    }
    public string DebugPrintFrame(InputFrame inputFrame, string prepend="")
    {
        if (inputFrame == null)
            return "Input NONE";

        string s = $"TS({inputFrame.FrameStamp})";
        foreach (var key in ClientData.AllowedKeys)
        {
            if (inputFrame.IsKey(key))
            {
                s += key;
            }

        }
        return s;
    }


}
