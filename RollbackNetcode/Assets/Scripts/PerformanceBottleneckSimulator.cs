
using UnityEngine;
using System.Threading;

public class PerformanceBottleneckSimulator : MonoBehaviour
{
    [SerializeField] private int bottleneckFrames = 10;        // Number of frames to apply the bottleneck
    [SerializeField] private int severityInMilliseconds = 5;   // Delay in ms for each bottleneck frame

    private int updateFrameCounter = 0;
    private int fixedUpdateFrameCounter = 0;

    void Update()
    {
        // Reset Update bottleneck frame counter on "T" key release
        if (Input.GetKeyUp(KeyCode.T))
        {
            updateFrameCounter = 0;
        }

        // Apply bottleneck in Update if within frame limit
        if (Input.GetKey(KeyCode.T) && updateFrameCounter < bottleneckFrames)
        {
            Thread.Sleep(severityInMilliseconds);
            updateFrameCounter++;
        }
    }

    void FixedUpdate()
    {
        // Reset FixedUpdate bottleneck frame counter on "R" key release
        if (Input.GetKeyUp(KeyCode.R))
        {
            fixedUpdateFrameCounter = 0;
        }

        // Apply bottleneck in FixedUpdate if within frame limit
        if (Input.GetKey(KeyCode.R) && fixedUpdateFrameCounter < bottleneckFrames)
        {
            Thread.Sleep(severityInMilliseconds);
            fixedUpdateFrameCounter++;
        }
    }
}
