using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

//This is a packet used only for debug purposes to chech how checksums is calculated
public struct GameStatePacket
{
    public FixedVector2 PlayerOne;
    public FixedVector2 PlayerTwo;
    public int HealtPlayerOne;
    public int HealtPlayerTwo;
}

public struct InputFramePacket
{
    // 8 bytes - the elapsed scaled time when input was sent
    public double SendOnTimestamp;

    //4 bytes - the exact frame when input is supposed to be executed
    public Int32 FrameStamp;

    //4 bytes - checksum of the relevant part of the
    //game state (for the frame the packet was sent)
    public Int32 Checksum;

    //Enable to check the exact game state that is being tested against
    //public GameStatePacket State;

    //The transmitted input data
    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 6)]
    public byte[] InputElements;
}

public struct SocketOperationTiming
{
    public long Begin;
    public long End;
    public long Duration => End - Begin;
    public SocketOperationTiming(float begin)
    {
        Begin = 0;
        End = 0;
    }
}

//The packet used in the game/colre application layer.
//Send and receive are done asyncrhronously C# ScocketAsyncEvent args
//Send is done just after collecting input from the keyboard
public class NetworkGamePacket : MonoBehaviour
{
    private static InputFramePacket _lastReceivedGamePacket;
    private static InputFramePacket _lastSendGamePacket;
    public static InputFramePacket LastReceivedGamePacket 
    {get {return _lastReceivedGamePacket;}private set { _lastReceivedGamePacket = value; }}

    public bool reocuringReceiveEvent { get; set; }
    public bool sendFinished { get; set; }

    public bool isReceiverStarted { get; set; }

    public byte[] receiveByteBuffer { get; set; }

    // Update is called once per frame
    public delegate void NetworkGamePacketReceived(object e, InputFramePacket packet);

    public static event NetworkGamePacketReceived OnNetworkGamePacketReceived;

    public static readonly object receiveLock = new object();
    private StateManager stateManager;
    public static int RollbackFrames = 0;
    public static readonly int MAX_ROLLBACK_SUPPORTED = 7;

    public byte[] sendByteBuffer { get; set; }
    // Start is called before the first frame update
    private void Start()
    {
        sendFinished = true;
        reocuringReceiveEvent = true;

        stateManager = GetComponentInParent<StateManager>();
        if (stateManager == null)
        {
            throw new Exception("State Manager component must be present on the parent gameobject");
        }

        //Default game packet initialization
        InputFramePacket gp;
        gp.SendOnTimestamp = FrameLimiter.Instance.GetScaledTimeSinceFightingSimulationStart();
        gp.FrameStamp = -1;
        gp.Checksum = 0;
        //gp.State = new GameStatePacket();
        gp.InputElements = new byte[4];

        receiveByteBuffer = SocketUtility.RawSerialize(gp);
        sendByteBuffer = SocketUtility.RawSerialize(gp);

    }
    private void FixedUpdate()
    {
        if (ClientData.SoloPlay == true) return;

        bool canSimulationProceed = CanSimulationProceed();
        PauseAndWaitForInputFromPeer(canSimulationProceed);

        if (ClientData.GameState != GameState.Runing || !ClientData.TwoWayConnectionEstablished())
            return;

        //
        if (!isReceiverStarted)
        {
            isReceiverStarted = true;
            Debug.Log("NGP: Receive Started");

            SocketUtility.DefaultReceive(
                SocketComunication.Receiver,
                receiveByteBuffer, ReceiveGamePacket_Reoccur);
        }

        //Send
        var gamePacket = FromInputFrameToPacket(StaticBuffers.Instance.PlayerBuffer.LastFrame);
        bool newInputHasBeenQueued = _lastSendGamePacket.FrameStamp != gamePacket.FrameStamp;
        if (sendFinished && newInputHasBeenQueued)
        {
            LogSendBegin();

            sendFinished = false;
            sendByteBuffer = SocketUtility.RawSerialize(gamePacket);
            _lastSendGamePacket = gamePacket;

            SocketUtility.DefaultSend(
                SocketComunication.Sender
                , sendByteBuffer, SendGamePacket_Completed);
        }
    }

    //In the case the simulation is running
    //and input from peer it has not arrive in time frame
    private static void PauseAndWaitForInputFromPeer(bool canSimulationProceed)
    {
        if (ClientData.GameState != GameState.Finished &&
            ClientData.GameState != GameState.NotYetStarted)
        {
            if (canSimulationProceed)
                ClientData.GameState = GameState.Runing;
            else
                ClientData.GameState = GameState.Paused;
        }
    }

    //--Converstion Bettwen Input Frames and Input Frame Packet

    #region ConverstionBetweenGameDataAndNetworkData

    //From InputFrame To InputFramePacket
    private InputFramePacket FromInputFrameToPacket(InputFrame inputFrame)
    {
        InputFramePacket gp = new InputFramePacket();
        if (inputFrame != null)
        {
            gp.SendOnTimestamp = FrameLimiter.Instance.
                GetScaledTimeSinceFightingSimulationStart();

            gp.FrameStamp = inputFrame.FrameStamp;
            gp.InputElements = inputFrame.Inputs;
            gp.Checksum = inputFrame.Checksum;
        }
        else
            Debug.LogError("Error send empty player buffer");
        return gp;
    }
    private static InputFrame FromPacketToInputFrame(InputFramePacket packet)
    {
        return new InputFrame(packet.InputElements, packet.FrameStamp, packet.Checksum);
    }

    #endregion ConverstionBetweenGameDataAndNetworkData

    //Simulation can proceed only if the is input
    //from the other peer MaxRollbackFrames=7 back
    private static bool CanSimulationProceed()
    {
        int confirmFrame = LastReceivedGamePacket.FrameStamp;
        int minAllowedConfirmFrame = FrameLimiter.Instance.FramesInPlay - MAX_ROLLBACK_SUPPORTED;
        bool canSimulationProceed = minAllowedConfirmFrame <= confirmFrame;
        return canSimulationProceed;
    }

    // --Application Layer Callbacks

    #region ApplicationLayerCallbacks

    public void SendGamePacket_Completed(object e, SocketAsyncEventArgs arg)
    {
        sendFinished = true;
        LogSendEnd();
        OnSendOperationFinished?.Invoke(SendTimingData);
    }

    public void ReceiveGamePacket_Reoccur(object e, SocketAsyncEventArgs arg)
    {
        LogReceiveBegin();
        if (arg.SocketError == SocketError.Success && arg.BytesTransferred != 0)
        {
            if (arg.BytesTransferred >= Marshal.SizeOf(typeof(InputFramePacket)))
            {
                lock (receiveLock)
                {
                    //Assign the last received packet
                    LastReceivedGamePacket = SocketUtility.RawDeserialize<InputFramePacket>(arg.Buffer, 0);

                    //Convert it to input frame
                    InputFrame inputFrame = FromPacketToInputFrame(LastReceivedGamePacket);

                    RollbackFrames = LastReceivedGamePacket.FrameStamp - FrameLimiter.Instance.FramesInPlay;
                    if (RollbackFrames < 0)
                    {
                        StaticBuffers.Instance.EnemyRBBuffer?.
                            RollbackEnqueue(inputFrame);
                    }
                    else
                    {
                        //EDGE CASE: if input arrives on the same frame, it could arrive
                        //before or after prediction
                        if (IsPacketReceivedAfterPredictionAppendedToRollback())
                        {
                            //If prediction already appended to RB Buffer
                            //RollbackEnqueue so it can be substitued with the actual value
                            StaticBuffers.Instance.EnemyRBBuffer?.
                                RollbackEnqueue(inputFrame);
                        }
                        else
                        {
                            //If not yet predicted -> fighter buffer not yet processed the input
                            //just enqueue it
                            StaticBuffers.Instance.EnemyBuffer?.Enqueue(inputFrame);
                        }
                    }

                    //Notify observers that a packet has been received
                    OnNetworkGamePacketReceived?.Invoke(this, LastReceivedGamePacket);

                    //Repeat receive
                    if (reocuringReceiveEvent)
                    {
                        //Finish logging the time of the receive operation
                        LogReceiveEnd();

                        //Invoke event on receive end
                        OnReceiveOperationFinished?.Invoke(SendTimingData);

                        //Repeat receive
                        SocketUtility.DefaultReceive(
                            SocketComunication.Receiver, receiveByteBuffer, ReceiveGamePacket_Reoccur);
                    }
                }
            }
        }
        else
        {
            //Shutdown sockets
            ClientData.GameState = GameState.Finished;
            SocketUtility.ShutdownAndCloseSocket(SocketComunication.Sender, "Sender");
            SocketComunication.Sender = null;
            SocketUtility.ShutdownAndCloseSocket(SocketComunication.Receiver, "Receiver");
            SocketComunication.Receiver = null;

            SocketComunication.TryInitializeSockets();
        }
    }

    private static bool IsPacketReceivedAfterPredictionAppendedToRollback()
    {
        return LastReceivedGamePacket.FrameStamp > 0 &&
            StaticBuffers.Instance.EnemyRBBuffer.LastFrame is not null &&
            StaticBuffers.Instance.EnemyRBBuffer.LastFrame.FrameStamp > 0 &&
            LastReceivedGamePacket.FrameStamp <= StaticBuffers.Instance.EnemyRBBuffer.LastFrame.FrameStamp;
    }

    #endregion ApplicationLayerCallbacks

    //DEBUG LOGING FOR SOCKET OPERATIONS

    #region LoggingSocketOperations

    //Struct to keet data on the timing of receives and sends
    //Usefull for debugging how long the operation took
    private SocketOperationTiming ReceiveTimingData;

    private SocketOperationTiming SendTimingData;

    public delegate void SocketOperationFinished(SocketOperationTiming socketOperationTiming);

    public event SocketOperationFinished OnReceiveOperationFinished;

    public event SocketOperationFinished OnSendOperationFinished;

    // --SEND OPERATION--
    private void LogSendBegin()
    {
        this.SendTimingData.Begin = FrameLimiter.Instance.GetUncaledTicksSinceSceneStarted();
        SocketComunication.Log($"NGP:: Send STARTED {GetGameRelativeTiming(SendTimingData.Begin)}", LogType.Log);
    }
    private void LogSendEnd()
    {
        this.SendTimingData.End = FrameLimiter.Instance.GetUncaledTicksSinceSceneStarted();
        SocketComunication.Log($"NGP:: Send FINISHED {GetGameRelativeTiming(SendTimingData.End)}", LogType.Log);
        SocketComunication.Log($"NGP:: Send DURATION MS{GetGameRelativeTiming(SendTimingData.Duration)}", LogType.Log);
    }

    // --RECEIVE OPERATION--

    private void LogReceiveBegin()
    {
        ReceiveTimingData.Begin = FrameLimiter.Instance.GetUncaledTicksSinceSceneStarted();
        SocketComunication.Log($"NGP:: Receive BEGIN {GetGameRelativeTiming(ReceiveTimingData.Begin)}", LogType.Log);
    }
    private void LogReceiveEnd()
    {
        ReceiveTimingData.End = FrameLimiter.Instance.GetUncaledTicksSinceSceneStarted();
        SocketComunication.Log($"NGP:: Receive END {GetGameRelativeTiming(ReceiveTimingData.End)}", LogType.Log);
        SocketComunication.Log($"NGP:: Receive DURATION {GetGameRelativeTiming(ReceiveTimingData.Duration)}", LogType.Log);
    }

    //Outputs a string that print the time in ticks, ms and game frames
    public static string GetGameRelativeTiming(long ticks)
    {
        return $"T: ({ticks}), MS: ({FrameLimiter.Instance.TickToMilliseconds(ticks)}), F:({FrameLimiter.Instance.TickToMilliseconds(ticks) / 16})";
    }

    #endregion LoggingSocketOperations
}