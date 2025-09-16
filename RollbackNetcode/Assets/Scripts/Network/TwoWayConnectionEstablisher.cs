using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public struct ChatListenerSocketInfo
{
    //4 bytes
    public Int32 ipAddress;

    //4 bytes accounted
    public Int32 portNum;
}

public class TwoWayConnectionEstablisher : MonoBehaviour
{
    private bool receiveOncePacketInformation = false;
    private ChatListenerSocketInfo ChatListenerSocketInfo;
    private byte[] buffer;
    private int bufferLength;
    public SocketComunication socketComunication;
    // Start is called before the first frame update
    private void Awake()
    {
        buffer = SocketUtility.RawSerialize(ChatListenerSocketInfo);
        bufferLength = buffer.Length;
        buffer = new byte[bufferLength];
        if(SocketComunication.ConnectionListener.IsBound == false)
        {
            SocketUtility.TryListenOnPortNums(
                SocketComunication.ConnectionListener, 12000,12001,12003);
        }

        //Bind sender, ChatListener and receiver for the game packet transmission
        SocketAsyncEventArgs accceptArgs = new SocketAsyncEventArgs();
        accceptArgs.Completed += Accept_Completed;
        SocketComunication.ConnectionListener.AcceptAsync(accceptArgs);
        
    }


    private void Accept_Completed(object sender, SocketAsyncEventArgs e)
    {
        Debug.Log($"Accepted Socket: {e.AcceptSocket.RemoteEndPoint.ToString()}");
        if (e.SocketError == SocketError.Success)
        {
            SocketComunication.Receiver = e.AcceptSocket;

            //Connection established to receiver but not sender
            //Not inittiator
            if (SocketComunication.Receiver.Connected && !SocketComunication.Sender.Connected)
            {
                ClientData.IsClientInitiator = false;

                // NEW CODE
                Debug.Log($"Attaching Default Receive To Socket");
                SocketUtility.DefaultReceive(SocketComunication.Receiver,buffer, ConnectSenderToReceiver_OnReceive);
            }
        }
        else if (e.SocketError == SocketError.ConnectionReset)
        {
            throw new Exception("Socket Error");
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void ConnectSenderToReceiver_OnReceive(object sender, SocketAsyncEventArgs e)
    {
        ChatListenerSocketInfo = SocketUtility.RawDeserialize<ChatListenerSocketInfo>(e.Buffer, 0);
        if (ChatListenerSocketInfo.portNum > 0)
        {
            SocketComunication.TryConnectToPort(ChatListenerSocketInfo.portNum);
        }
    }

    public void EstablishTwoWayConnection(int portNum)
    {
        if (!ClientData.TwoWayConnectionEstablished())
        {
            //Try Establishing One Way Connection First between Initiator Sender
            //and Client Receiver
            bool success = SocketComunication.TryConnectToPort(portNum);

            //If successfull pass the other peer our receiver socket information
            if (success)
            {
                var sender = SocketComunication.Sender;

                //Setup buffer
                IPEndPoint ip = SocketComunication.ConnectionListener.LocalEndPoint as IPEndPoint;

                ChatListenerSocketInfo.ipAddress = (int)ip.Address.Address;
                ChatListenerSocketInfo.portNum = ip.Port;

                buffer = SocketUtility.RawSerialize(ChatListenerSocketInfo);

                SocketUtility.DefaultSend(SocketComunication.Sender,buffer);
            }
        }
    }
}