using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SocketStateVisualizer : MonoBehaviour
{
    public Text SocketStateText;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ClientData.IsDebugModeEnabled == false)
        {
            SocketStateText.text = "";
            return;

        }

        string socketState = "";
        lock(ChatSocketCommunication.__chatLock)
        {
            socketState += SocketUtility.GetSocketStateString(ChatSocketCommunication.ChatClient, "ChatClient") + "\n";
            socketState += SocketUtility.GetSocketStateString(ChatSocketCommunication.ChatListener,"ChatListener")+"\n";
        }
        socketState += SocketUtility.GetSocketStateString(SocketComunication.ConnectionListener,"ConnectionListener")+"\n";
        socketState += SocketUtility.GetSocketStateString(SocketComunication.Sender, "Sender") + "\n";
        socketState += SocketUtility.GetSocketStateString(SocketComunication.Receiver, "Receiver") + "\n";
        socketState += $"Is Next Message Introduction: {ChatSocketCommunication.IsNextMessageIntroduction}";
        SocketStateText.text = socketState;
        
    }
}
