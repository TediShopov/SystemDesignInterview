using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

public class SocketComunication : MonoBehaviour
{
    //Object is SINGLETON. The instance can be accessed from anywhere
    public static SocketComunication Instance;

    //Start listening on Unity Awake
    public static bool IsListenOnAwake = true;

    public static Socket Receiver;
    public static Socket Sender;
    public static Socket ConnectionListener;

    //Default ports to listen on
    private const int DEFAULT_LISTEN_PORT = 12000;
    private const int FALLBACK_LISTEN_PORT = 12001;
    private const int FALLBACKTWO_LISTEN_PORT = 12002;

    private void Awake()
    {
        if (SocketComunication.Instance == null)
        {
            SocketComunication.Instance = this;
            TryInitializeSockets();

            //Set this object as persistent across scenes
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public void FixedUpdate()
    {
        OutputSocketLockToUnityLog();
    }
    protected static Socket InitializeTCPSocket()
    {
        Socket socket;
        try
        {
            socket = new Socket(AddressFamily.InterNetwork,
                                         SocketType.Stream,
                                         ProtocolType.Tcp);
            return socket;
        }
        catch (Exception e)
        {
            Debug.Log($"Couldnt set up ChatListener socket. Error {e.Message.ToString()}");
            throw;
        }
    }

    //Connect Sender to another port by a given port number.
    //Returns if connection was successful or not
    public static bool TryConnectToPort(int portNum)
    {
        try
        {
            IPEndPoint iPEndPoint = SocketUtility.GetLocalEndPoint(portNum);
            Sender.ConnectAsync(iPEndPoint).Wait();
            Debug.Log($"Trying to connect to {iPEndPoint.Address.ToString()}");
            ClientData.IsClientInitiator = !Receiver.Connected;
            return true;
        }
        catch (Exception e)
        {
            Debug.Log($"Trying to connect throwed exception");

            return Sender.Connected;
        }
    }

    public static void TryInitializeSockets()
    {
        if (Receiver == null)
            Receiver = InitializeTCPSocket();

        if (Sender == null)
            Sender = InitializeTCPSocket();

        Sender.NoDelay = true;
        Sender.Blocking = false;
        Receiver.Blocking = false;
        if (ConnectionListener == null)
            ConnectionListener = InitializeTCPSocket();

        //Start listening on awake
        if (IsListenOnAwake && ConnectionListener.IsBound == false)
            SocketUtility.TryListenOnPortNums(ConnectionListener, DEFAULT_LISTEN_PORT, FALLBACK_LISTEN_PORT, FALLBACKTWO_LISTEN_PORT);

        //Todo check if we need a lingering option
        Sender.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(false, 0));
        Receiver.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(false, 0));
    }

    public void RestartSockets()
    {
        SocketUtility.ShutdownAndCloseSocket(Receiver, "TCP NGP Receiver");
        SocketUtility.ShutdownAndCloseSocket(Sender, "TCP NGP Sender");
        Receiver = null;
        Sender = null;
        TryInitializeSockets();
    }

    //Manage sockets when application is exited
    private void OnApplicationQuit()
    {
        SocketUtility.ShutdownAndCloseSocket(Receiver, "TCP NGP Receiver");
        SocketUtility.ShutdownAndCloseSocket(Sender, "TCP NGP Sender");
        SocketUtility.ShutdownAndCloseSocket(ConnectionListener, "TCP NGP ConnectionChatListener");
        if (ClientData.Player is not null)
            PlayerDatabase.DeletePlayerData(ClientData.Player.PlayerName);
    }

    //Custom debug log funcitonality for outputting any relevant information that is not
    // on the Unity Thread

    #region SocketCustomLogging

    public bool DebugLogSocketOperationTiming = false;

    public static ConcurrentQueue<KeyValuePair<string, LogType>> SocketDebugLog;
    public static void Log(string s, LogType logType = LogType.Log)
    {
        if (SocketDebugLog is null)
            SocketDebugLog = new ConcurrentQueue<KeyValuePair<string, LogType>>();
        SocketDebugLog.Enqueue(new KeyValuePair<string, LogType>(s, logType));
    }
    //Output to unity log. WARNING, will only works on on the unity main thread.
    private void OutputSocketLockToUnityLog()
    {
        while (DebugLogSocketOperationTiming && SocketDebugLog is not null && SocketDebugLog.Count > 0)
        {
            KeyValuePair<string, LogType> pair;
            bool success = SocketDebugLog.TryDequeue(out pair);
            if (success)
            {
                if (pair.Value == LogType.Log)
                    UnityEngine.Debug.Log(pair.Key);
                else if (pair.Value == LogType.Warning)
                    UnityEngine.Debug.LogWarning(pair.Key);
                else if (pair.Value == LogType.Error)
                    UnityEngine.Debug.LogError(pair.Key);
            }
        }
    }

    #endregion SocketCustomLogging
}

internal class SocketUtility
{
    #region DefaultSocketOperations

    //Attemps to bind a socket to the local IP with a port number.
    //And make it a listener.
    public static bool BindLocalListener(Socket socket, int portNum)
    {
        try
        {
            if (socket.Connected == false)
            {
                IPAddress hostIP;
                IPEndPoint IPEndPoint;
                //Bind ChatListener socket
                IPAddress.TryParse("127.0.0.1", out hostIP);
                Debug.Log(hostIP.ToString());
                IPEndPoint = new IPEndPoint(hostIP, portNum);

                //Beign Listen
                socket.Bind(IPEndPoint);
                Debug.Log($"Listener Bound Port {IPEndPoint.ToString()}");

                socket.Listen(1);

                Debug.Log($" Listening on Port {portNum}");
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return false;
        }
        return false;
    }

    //Attempts to bind at the specified ports in the order they were provided
    public static bool TryListenOnPortNums(Socket socket, params int[] ports)
    {
        if (socket == null)
            return false;

        foreach (int port in ports)
        {
            //if successful conection established return
            //otherwise keep trying the fallback ports
            if (BindLocalListener(socket, port))
                return true;
        }
        //If no default port could be picked assign a random available socket
        BindLocalListener(socket, 0);
        return false;
    }

    //Default implementation of send and receive socket functions.
    //Implemented with socket asyns evetn args paradigm, supplied with a callback that is called both
    //on synrhonous and asynchronous exection for ease of development
    public static void DefaultSend(Socket Sender, byte[] buff, Action<object, SocketAsyncEventArgs> onCompleteAction = null)
    {
        SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
        arg.SetBuffer(buff, 0, buff.Length);
        if (onCompleteAction != null)
        {
            arg.Completed += new EventHandler<SocketAsyncEventArgs>(onCompleteAction);
        }
        arg.SocketFlags = SocketFlags.None;
        arg.RemoteEndPoint = Sender.RemoteEndPoint;

        //Calls the specified callback function both on synhrounous and
        // ansyncrounous execution.
        bool sendAll = !Sender.SendAsync(arg);
        if (sendAll)
        {
            if (onCompleteAction != null)
            {
                onCompleteAction(Sender, arg);
            }
        }
    }
    public static void DefaultReceive(Socket Receiver, byte[] buff, Action<object, SocketAsyncEventArgs> onCompleteAction)
    {
        SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
        arg.SetBuffer(buff, 0, buff.Length);
        if (onCompleteAction != null)
        {
            arg.Completed += new EventHandler<SocketAsyncEventArgs>(onCompleteAction);
        }
        arg.SocketFlags = SocketFlags.None;

        //Calls the specified callback function both on synhrounous and
        // ansyncrounous execution.
        bool receivedAll = !Receiver.ReceiveAsync(arg);
        if (receivedAll)
        {
            if (onCompleteAction != null)
            {
                onCompleteAction(Receiver, arg);
            }
        }
    }

    //Shutdown and close socket
    public static void ShutdownAndCloseSocket(Socket sock, string socketName)
    {
        if (sock != null)
        {
            if (sock.Connected)
            {
                sock.Shutdown(SocketShutdown.Both);
            }
            sock.Close();
        }
    }

    #endregion DefaultSocketOperations

    //Utility Serializaton. Uses C# Type Marshalling to allocate the coorect number of bytes.

    #region Serialization

    //Utility Serializaton. Uses C# Type Marshalling to allocate the coorect number of bytes.
    //returns the byte array
    public static byte[] RawSerialize(object anything)

    {
        int rawSize = Marshal.SizeOf(anything);
        IntPtr buffer = Marshal.AllocHGlobal(rawSize);
        Marshal.StructureToPtr(anything, buffer, false);
        byte[] rawDatas = new byte[rawSize];
        Marshal.Copy(buffer, rawDatas, 0, rawSize);
        Marshal.FreeHGlobal(buffer);
        return rawDatas;
    }
    //Utility Deserializatoin. Uses C# Type Marshalling to retrieve the coorect number of bytes and
    //and return object of specified type
    public static T RawDeserialize<T>(byte[] rawData, int position)
    {
        int rawsize = Marshal.SizeOf(typeof(T));
        if (rawsize > rawData.Length - position)
            throw new ArgumentException("Not enough data to fill struct. Array length from position: " + (rawData.Length - position) + ", Struct length: " + rawsize);
        IntPtr buffer = Marshal.AllocHGlobal(rawsize);
        Marshal.Copy(rawData, position, buffer, rawsize);
        T retobj = (T)Marshal.PtrToStructure(buffer, typeof(T));
        Marshal.FreeHGlobal(buffer);
        return retobj;
    }

    #endregion Serialization

    //UTILITY getters

    #region UtilityGetters

    public static IPEndPoint GetLocalEndPoint(int port)
    {
        IPAddress hostIP;
        //Bind ChatListener socket
        IPAddress.TryParse("127.0.0.1", out hostIP);
        IPEndPoint ip = new IPEndPoint(hostIP, port);
        return ip;
    }

    //Output the socket state to a formated string. Useful for debugging
    public static string GetSocketStateString(Socket sock, string socketname = "")
    {
        if (sock is null)
            return socketname + "Socket is null";
        //if (sock.IsBound == false)
        //   return $"{socketname}: Bound:{sock.IsBound}";
        else
        {
            if (sock.Connected == false)
                return $"{socketname}: Bound:{sock?.LocalEndPoint?.ToString()} Connected:{sock.Connected} ";
            else
                return $"{socketname}: Bound:{sock?.LocalEndPoint?.ToString()} Connected:{sock?.RemoteEndPoint.ToString()} ";
        }
    }

    #endregion UtilityGetters
}