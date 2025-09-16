using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

public struct SyncClock
{
    public bool isActive;               // If Client or initiator is responsible for filling timestamps
    public double unpauseGameLocalTime; // If set to > 0, Client has to wait for that amount of ticks
    public long initiatorSend;          // Time stamp on initiator send
    public long clientReceive;          // Time Stamp on client receive
    public long clientSend;             // Time stmap on client send
    public long initiatorReceive;       // TIme stamp on initiator receive
}

//Class that is called when the game is paused at start of round to caluclate
// RTT and send unpause command at the correct time
// Uses a request-resposne singel step delay mechanism to sync the clocks
//https://support.hpe.com/techhub/eginfolib/networking/docs/switches/5700/5998-5594r_nmm_cg/content/446947155.htm
public class SyncClocks : MonoBehaviour
{
    public int TimesToCalculateRTT;
    public float AverageWaitMultiplier;

    private List<double> _calculatedOneWayLatency;
    private List<double> _calculatedOffset;
    private double AverageLatency => Queryable.Average(_calculatedOneWayLatency.AsQueryable());
    private double AverageOffset => Queryable.Average(_calculatedOffset.AsQueryable());
    private byte[] bufferBytes;
    int byteCount;

    // Start is called before the first frame update
    private void Start()
    {
        //Init
        _calculatedOneWayLatency = new List<double>();
        _calculatedOffset = new List<double>();

        if (ClientData.SoloPlay)
        {
            ClientData.GameState = GameState.Runing;
            return;
        }

        SyncClock syncClockMockPacket = DefaultSyncClockBuffer();
        byteCount = System.Runtime.InteropServices.Marshal.SizeOf(syncClockMockPacket);
        bufferBytes = SocketUtility.RawSerialize(syncClockMockPacket);
        if (!ClientData.TwoWayConnectionEstablished())
        {
            throw new System.Exception("Problem establishing connection between sockets");
        }

        FrameLimiter.Instance.enabled = true;
        if (ClientData.IsClientInitiator)
        {
            InitiatorSend();
        }
        else
        {
            ClientReceiveAndSend();
        }
    }
    private SyncClock DefaultSyncClockBuffer()
    {
        SyncClock syncClock;
        syncClock.isActive = false;
        syncClock.unpauseGameLocalTime = 0;
        syncClock.initiatorSend = 0;
        syncClock.clientReceive = 0;
        syncClock.clientSend = 0;
        syncClock.initiatorReceive = 0;
        return syncClock;
    }
    private void InitiatorSend()
    {
        SyncClock syncClock = DefaultSyncClockBuffer();

        //
        if (_calculatedOneWayLatency.Count < TimesToCalculateRTT)
        {
            //Setup the sync clock packet to send
            syncClock.isActive = true;
            syncClock.initiatorSend = FrameLimiter.Instance.GetUncaledTicksSinceSceneStarted();

            //Seriazlie it to the byte buffer
            bufferBytes = SocketUtility.RawSerialize(syncClock);
            SocketUtility.DefaultSend(
                SocketComunication.Sender, bufferBytes);

            InitiatorReceive();
        }
        else
        {
            //Final send
            //Add extra time to wait based on the avg one way latency
            double waitTime = AverageLatency * AverageWaitMultiplier;

            //Convert to peer local time
            double otherTime = FrameLimiter.Instance.GetUncaledTicksSinceSceneStarted() + AverageOffset;
            double startAtInPeersLocalTime = otherTime + waitTime;
            //Setup the sync clock packet to send
            syncClock.unpauseGameLocalTime = startAtInPeersLocalTime;

            //Set the waiting time for this peer
            FrameLimiter.Instance.unpauseGameAt = FrameLimiter.Instance.GetUncaledTicksSinceSceneStarted() + waitTime;

            Debug.Log($"Scheduled Time of start Self:{FrameLimiter.Instance.unpauseGameAt} Other: {syncClock.unpauseGameLocalTime} Current Time{(double)FrameLimiter.Instance.GetUncaledTicksSinceSceneStarted()}");

            //Seriazlie it to the byte buffer
            bufferBytes = SocketUtility.RawSerialize(syncClock);
            SocketUtility.DefaultSend(
                SocketComunication.Sender,
                bufferBytes, InitiatorFinalSend_Completed);
        }
    }

    private void InitiatorReceive()
    {
        SocketUtility.DefaultReceive(
            SocketComunication.Receiver,
            bufferBytes, InitiatorReceive_Completed);
    }

    private void ClientReceiveAndSend()
    {
        SocketUtility.DefaultReceive(SocketComunication.Receiver, bufferBytes, ClientReceive_Completed);
    }
    private void ClientSend(SyncClock clock)
    {
        if (FrameLimiter.Instance != null)
        {
            //Stamp the time off receive
            clock.clientSend = FrameLimiter.Instance.GetUncaledTicksSinceSceneStarted();
        }

        //Seriazlie it to the byte buffer
        bufferBytes = SocketUtility.RawSerialize(clock);

        SocketUtility.DefaultSend(
            SocketComunication.Sender,
            bufferBytes,
            ClientSend_Completed);
    }

    private void ClientSend_Completed(object sender, SocketAsyncEventArgs e)
    {
        ClientReceiveAndSend();
    }

    private void InitiatorFinalSend_Completed(object sender, SocketAsyncEventArgs e)
    {
        Debug.LogError("Final Sync Clock Packet send");
        //Millisecond to wait for the information to arrive to the other peer
    }
    private void ClientReceive_Completed(object sender, SocketAsyncEventArgs e)
    {
        //Deserialzie the received buffer
        SyncClock syncClock = SocketUtility.RawDeserialize<SyncClock>(bufferBytes, 0);

        if (syncClock.unpauseGameLocalTime > 0)
        {
            FrameLimiter.Instance.unpauseGameAt = syncClock.unpauseGameLocalTime;
        }
        else
        {
            if (FrameLimiter.Instance != null)
            {
                //Stamp the time off receive
                syncClock.clientReceive = FrameLimiter.Instance.GetUncaledTicksSinceSceneStarted();
            }

            // Try to convert to send async
            ClientSend(syncClock);
        }
    }

    private void InitiatorReceive_Completed(object sender, SocketAsyncEventArgs e)
    {
        SyncClock syncClock = SocketUtility.RawDeserialize<SyncClock>(bufferBytes, 0);
        syncClock.initiatorReceive = FrameLimiter.Instance.GetUncaledTicksSinceSceneStarted();

        long t1 = syncClock.initiatorSend;
        long t2 = syncClock.clientReceive;
        long t3 = syncClock.clientSend;
        long t4 = syncClock.initiatorReceive;

        Debug.Log($"T1[is]: {t1}, " +
            $"T2[cr]: {t2}, " +
            $"T3[cs]: {t3}, " +
            $"T4[ir]: {t4}, " +
            $"");

        double RTT = (t4 - t1) - (t3 - t2);
        double OneWayDelay = RTT / 2.0;
        double offset = ((t2 - t1) - (t4 - t3)) / 2;

        Debug.Log($"RTT(ms): {FrameLimiter.Instance.TickToMilliseconds(RTT)}, RTT: {RTT}, RTT/2: {OneWayDelay}, Offset: {offset}");
        this._calculatedOneWayLatency.Add(OneWayDelay);
        this._calculatedOffset.Add(offset);

        Debug.Log($"LatencyAvg: {AverageLatency} OffsetAvg:{AverageOffset} Time:{_calculatedOneWayLatency.Count}");

        InitiatorSend();
    }
}