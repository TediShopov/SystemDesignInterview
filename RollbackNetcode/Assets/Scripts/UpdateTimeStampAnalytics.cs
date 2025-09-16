using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TimePrecision
{
    Ticks = 0,
    Miliseconds = 1,
    Frames = 2,
}
public class UpdateTimeStampAnalytics : MonoBehaviour
{
    public Text TimeStepDiff;
    public Text AvgTimeStepDiff;
    public Text MaxTimeStepDiff;
    public Text MinTimeStepDiff;
    public Text RollbackFramesText;
    public Text Offset;
    public Text CurrentFrame;
    public Text PlayerBufferFrameToSend;
    public Text LastReceivedText;

    public Text SendTimingInformation;
    public Text ReceiveTimingInformation;

    public List<SocketOperationTiming> RecordedSendOperationTimings;
    public List<SocketOperationTiming> RecordedReceiveOperationTimings;

    public TimePrecision TimePrecision;

    public NetworkGamePacket NetworkGamePacket;
    public Restore RollbackScript;
    

    private static readonly object networkReceiveLock = new object();

    //Collection ot he 
    List<int> FrameDifferences;

    public int PeriodFrames;

    // Start is called before the first frame update
    void Start()
    {
        RecordedReceiveOperationTimings = new List<SocketOperationTiming>();
        RecordedSendOperationTimings = new List<SocketOperationTiming>();
        TimeStepDiff.text = "0";
        AvgTimeStepDiff.text = "0";
        MaxTimeStepDiff.text = "0";
        MinTimeStepDiff.text = "0";
        RollbackFramesText.text = "0";
        PlayerBufferFrameToSend.text = "0";
        CurrentFrame.text = "0";
        NetworkGamePacket.OnNetworkGamePacketReceived += UpdatePacketInformation;
        NetworkGamePacket.OnReceiveOperationFinished += AppendReceiveTimingInfo;
        NetworkGamePacket.OnSendOperationFinished += AppendSendTimingInfo;
        FrameDifferences = new List<int>();
        
    }

    private void AppendReceiveTimingInfo(SocketOperationTiming socketOperationTiming)
    {
        this.RecordedReceiveOperationTimings.Add(socketOperationTiming);
        if (RecordedReceiveOperationTimings.Count > PeriodFrames)
        {
            RecordedReceiveOperationTimings.RemoveAt(0);
        }
    }
    private void AppendSendTimingInfo(SocketOperationTiming socketOperationTiming)
    {
        this.RecordedSendOperationTimings.Add(socketOperationTiming);
        if (RecordedSendOperationTimings.Count > PeriodFrames)
        {
            RecordedSendOperationTimings.RemoveAt(0);
        }
    }

    private void UpdatePacketInformation(object e, InputFramePacket packet)
    {
        lock(networkReceiveLock)
        {
            int timeStepDifference = packet.FrameStamp - FrameLimiter.Instance.FramesInPlay ;
            FrameDifferences.Add(timeStepDifference);
            //Keep the frame different list a fixed size.
            //Append 
            if (FrameDifferences.Count > PeriodFrames) 
            {
                FrameDifferences.RemoveAt(0);
            }
        }


    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if(ClientData.IsDebugModeEnabled)
            this.transform.position = new Vector3(0,0, 0);
        else
            this.transform.position = new Vector3(99990,0, 0);

        if (FrameDifferences is null)
            return;
        //Debug.Log($"Frame Diff Count {FrameDifferences.Count}");
        double peerSimulationOffset = 0;
        //Debuggin the offset of the simulations based on timestamps
        //Negative values means that the current simulation is behind.
        if (FrameLimiter.Instance is not null)
        {
             peerSimulationOffset =
                FrameLimiter.Instance.GetScaledTimeSinceFightingSimulationStart()
                - NetworkGamePacket.LastReceivedGamePacket.SendOnTimestamp;

        }
        lock(networkReceiveLock) 
        {
                CurrentFrame.text = "Frames: " + FrameLimiter.Instance.FramesInPlay.ToString();
            if(StaticBuffers.Instance.PlayerBuffer.LastFrame is not null)
                PlayerBufferFrameToSend.text = "PlayerLastBuffer: " + StaticBuffers.Instance.PlayerBuffer.LastFrame.FrameStamp.ToString();
            LastReceivedText.text = "LastReceived: " + NetworkGamePacket.LastReceivedGamePacket.FrameStamp.ToString();
            if(FrameDifferences.Count > 0) 
            {
                int frameDiff = FrameDifferences[FrameDifferences.Count - 1];
                double avg = FrameDifferences.Average(x=>x);
                int max = FrameDifferences.Max(x=>x);
                int min = FrameDifferences.Min(x=>x);
                TimeStepDiff.text = frameDiff.ToString();
                RollbackFramesText.text = NetworkGamePacket.RollbackFrames.ToString();

                AvgTimeStepDiff.text = $"TimeStamp AVG {avg.ToString()}";
                MaxTimeStepDiff.text = $"TimeStamp MAX {max.ToString()}";
                MinTimeStepDiff.text = $"TimeStamp MIN {min.ToString()}";
                Offset.text= peerSimulationOffset.ToString();


                if (ReceiveTimingInformation is not null)
                {
                    ReceiveTimingInformation.text = "Receive Duration:: " + GetMaxMinAndAverageString(RecordedReceiveOperationTimings);
                }
                if (SendTimingInformation is not null)
                {
                    SendTimingInformation.text = "Send Duration:: " + GetMaxMinAndAverageString(RecordedSendOperationTimings);
                }



            }
        }

        
    }

    public double GetTime(double tick)
    {
        return GetTime(tick, TimePrecision);
    }

    public double GetTime(double tick, TimePrecision precision)
    {
        if (precision == TimePrecision.Ticks)
            return tick;
        if (precision == TimePrecision.Miliseconds)
            return ToMs(tick);
        if (precision == TimePrecision.Frames)
            return ToFrames(tick);
        return 0;
    }
    public double ToMs(double ticks ) => FrameLimiter.Instance.TickToMilliseconds(ticks);
    public double ToFrames(double ticks ) => ToMs(ticks) / 16;

    private string GetMaxMinAndAverageString(IEnumerable<SocketOperationTiming> data)
    {
        if (data is  null || data.Count() <= 0)
            return "";
        double max = data.Max(x => x.Duration);
        double min = data.Min(x => x.Duration);
        double average = data.Average(x => x.Duration);
        string toReturn = $"Max:{GetTime(max)} Min:{GetTime(min)} Average:{GetTime(average)}";
        return toReturn;
    }
}
