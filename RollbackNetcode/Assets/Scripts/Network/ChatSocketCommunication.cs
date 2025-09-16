using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class TextMessageEventArgs : EventArgs
{
    public string Message { get; }
    public bool IsIntroductionMessage { get; }

    public TextMessageEventArgs(string message, bool isIntroductionMessage = false)
    {
        Message = message;
        IsIntroductionMessage = isIntroductionMessage;
    }
}

public class ChatSocketCommunication : SocketComunication
{
    //Ports that the chat connetion would preffer
    private readonly int[] DEFAULT_CHAT_PORTS = { 13000, 13001, 13002 };

    private byte[] _chatBuffer;
    private static readonly int MESSAGE_BUFFER_SIZE = 1024;

    //Chat lock needed because it be accessed from async event not on the unity thread.
    //And socket state might be used on the unity thread. E.g printing the state ot the sockets
    public static readonly object __chatLock = new object();

    //Chat Sockets
    public static Socket ChatListener;

    public static Socket ChatClient;

    //Important callback events used by the UI to handle SEND, RECEIVE and DISCONECT
    public delegate void TextMessageReceivedHandler(object sender, TextMessageEventArgs e);

    public event TextMessageReceivedHandler OnTextMessageReceived;

    public event TextMessageReceivedHandler OnTextMessageSend;

    public delegate void ChatSocketDisconnectHandler(); // Define the delegate type

    public event ChatSocketDisconnectHandler OnChatSocketDisconnnected;        // Declare the event

    //Instance of the singleton
    public static ChatSocketCommunication Instance;

    //[IntrodoctionMessage] : a message which carries data about the the other peer.
    // Such as the user name and the endpoints of his sockets
    public static bool IsNextMessageIntroduction = true;

    #region Initializatoin
    private void Awake()
    {
        if (ChatSocketCommunication.Instance == null)
        {
            ChatSocketCommunication.Instance = this;
            _chatBuffer = new byte[MESSAGE_BUFFER_SIZE];
            //Initialize the socket objects. Not yet bounded
            InitSocketListener();
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public void ResetChatRelatedSockets()
    {
        //if (ChatListener is not null )
        //SocketUtility.ShutdownAndCloseSocket(ChatListener, "ChatListener");

        if (ChatClient is not null)
            ChatClient.Close();

        //ChatListener = null;
        ChatClient = null;

        if (ChatListener is null)
            InitSocketListener();
        IsNextMessageIntroduction = true;
    }

    private void InitSocketListener()
    {
        if (ChatListener is null)
        {
            ChatListener = InitializeTCPSocket();
        }

        if (ChatListener is not null && ChatListener.IsBound == false)
        {
            //Assign listening port as soon as obkect is contructed in scene
            SocketUtility.TryListenOnPortNums(ChatListener, DEFAULT_CHAT_PORTS);

            SocketAsyncEventArgs accceptArgs = new SocketAsyncEventArgs();
            accceptArgs.Completed += Chat_OnAcceptCompleted;
            ChatListener.AcceptAsync(accceptArgs);

            if (ChatListener is not null && ChatListener.IsBound)
                Debug.Log($"Chat Listening on port {ChatListener.LocalEndPoint.ToString()}");
            else
                Debug.Log($"Chat Couldnt initialize on default ports");
        }
    }

    #endregion


    //All the callbacks related to chat functionality
    #region ChatCallbackEvents
    private void Chat_OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
    {
        lock (__chatLock)
        {
            if (e.SocketError == SocketError.Success)
            {
                ChatClient = e.AcceptSocket;
                Debug.Log($"Chat Client Incoming Connection: Local:{ChatClient.LocalEndPoint.ToString()} Remote:{ChatClient.RemoteEndPoint.ToString()}");

                SocketUtility.DefaultReceive(ChatClient, _chatBuffer, Chat_ReceivedOnComplete);
                e.AcceptSocket = null; // Reset for reuse
                ChatListener.AcceptAsync(e);

                //SocketAsyncEventArgs accceptArgs = new SocketAsyncEventArgs();
                //accceptArgs.Completed += Chat_OnAcceptCompleted;
                //ChatListener.AcceptAsync(accceptArgs);
            }
            else
            {
                Debug.Log($"Error accepting connection: {e.SocketError}");
            }
        }
    }

    private void Chat_ReceivedOnComplete(object sender, SocketAsyncEventArgs e)
    {
        //        Socket clientSocket = (Socket)e.UserToken;
        lock (__chatLock)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                string receivedText = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);

                Debug.Log("Client: " + receivedText);
                OnTextMessageReceived?.Invoke(this, new TextMessageEventArgs(receivedText, IsNextMessageIntroduction));
                //Every other message is treated as normal
                IsNextMessageIntroduction = false;

                SocketUtility.DefaultReceive(ChatClient, _chatBuffer, Chat_ReceivedOnComplete);
            }
            else
            {
                ResetChatRelatedSockets();
                OnChatSocketDisconnnected?.Invoke();
            }
        }
    }
    private void Chat_SendOnComplete(object sender, SocketAsyncEventArgs e)
    {
        Console.WriteLine("Server message sent.");
        string sentText = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);
        OnTextMessageSend?.Invoke(this, new TextMessageEventArgs(sentText, IsNextMessageIntroduction));
        //Every other message is treated as normal
        IsNextMessageIntroduction = false;
    }

    #endregion

    //Attemps to to establish chat connection
    //Returns true if the connection was established
    public bool EstablishChatConnection(int portNum)
    {
        try
        {
            IPAddress hostIP;
            //Bind ChatListener socket
            IPAddress.TryParse("127.0.0.1", out hostIP);
            IPEndPoint ip = new IPEndPoint(hostIP, portNum);

            ChatClient = InitializeTCPSocket();
            ChatClient.ConnectAsync(ip).Wait();

            SocketUtility.DefaultReceive(ChatClient, _chatBuffer, Chat_ReceivedOnComplete);
            Debug.Log("Trying to connect to ChatListener");
            Debug.Log($"Chat Client  Connected To: Local:{ChatClient.LocalEndPoint.ToString()} " +
                $"Remote:{ChatClient.RemoteEndPoint.ToString()}");
        }
        catch (Exception e)
        {
            Debug.Log($"Trying to connect throwed exception");
        }
        return ChatClient is not null && ChatClient.Connected;
    }

    //Encode a string to UTF-8 text and send it through the chat client
    public void SendMessageToChat(string message)
    {
        if (ChatClient == null) return;
        _chatBuffer = UTF8Encoding.Default.GetBytes(message);
        SocketUtility.DefaultSend(ChatClient, _chatBuffer, Chat_SendOnComplete);
        Debug.Log($"Chat Client Socket Debug When Sendign:    Local:{ChatClient.LocalEndPoint.ToString()} Remote:{ChatClient.RemoteEndPoint.ToString()}");
    }
    private void OnDestroy()
    {
        SocketUtility.ShutdownAndCloseSocket(ChatClient, "ChatClient");
        ChatClient = null;
        SocketUtility.ShutdownAndCloseSocket(ChatListener, "ChatClient");
        ChatListener = null;
    }

    private void OnApplicationQuit()
    {
        SocketUtility.ShutdownAndCloseSocket(ChatClient, "ChatClient");
        SocketUtility.ShutdownAndCloseSocket(ChatListener, "ChatClient");
    }
}