using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterBufferMono : MonoBehaviour
{
    //TODO make buffer only accpets keys that do sth in the game
    [SerializeField]
    public bool CollectInputFromKeyboard;

    [SerializeField]
    public int DelayInput;

    [SerializeField]
    public int PressedKeysMaxCount = 5;

    [SerializeField]
    public int RefreshKeyPressedAfterFrames = 120;

    public delegate void InputFrameFromKeyboard(InputFrame frame);

    public event InputFrameFromKeyboard InputFrameOnAppendedFromKeyboard;

    public InputBuffer InputBuffer { get; set; }
    public void Awake()
    {
        InputBuffer = new InputBuffer();
        InputBuffer.DelayInput = DelayInput;
        InputBuffer.PressedKeysMaxCount = PressedKeysMaxCount;
        InputBuffer.RefreshKeyPressedAfterFrames = RefreshKeyPressedAfterFrames;
        this.GetComponent<FighterController>().InputBuffer = InputBuffer;
        StaticBuffers.Instance.RenewBuffers();
    }

    public void FixedUpdate()
    {
        if (CollectInputFromKeyboard && ClientData.GameState == GameState.Runing)
        {
            InputBuffer.Enqueue();
            InputFrameOnAppendedFromKeyboard?.Invoke(InputBuffer.LastFrame);
        }
        InputBuffer.OnUpdate();
    }
}