using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public struct Lobby
{
    public PlayerData Current;
    public PlayerData Other;


    //Current user must be on 
    public bool LobbyMinimal => Current != null ;
    public bool LobbyFull => Current != null && Other != null && !Current.Equals(Other);

    //If not connected this data structure only the hold the information
    //of the participants
    public bool IsConnected { get; set; }

    public void SetCurrent(PlayerData current) { this.Current = current; } 
    public void SetOther(PlayerData other) { this.Other = other; } 
    public void SetConnectionStatus(bool status) { this.IsConnected = status; } 
    public Lobby( PlayerData curr, PlayerData other, bool con = false)
    {
        this.Current = curr;
        this.Other = other;
        this.IsConnected = con;
        
    }
    public override string ToString()
    {
        string currentName = Current == null ? "None" : Current.PlayerName;
        string otherName = Other == null ? "None" : Other.PlayerName;
        return $"Current:{currentName}, Other:{otherName}, IsConnected:{IsConnected}\n";
    }

}

public class MainMenu: MonoBehaviour
{
    [SerializeField]
    public TMPro.TMP_Text chatTextBox;

    [SerializeField]
    public TMPro.TMP_InputField sendTextMessageInput;


    [SerializeField]
    public TMPro.TMP_InputField listenOnPortField;
    [SerializeField]
    public TMPro.TMP_Dropdown playerSelectDropdown;
    [SerializeField]
    public Button createLobbyButton;
    [SerializeField]
    public Button joinLobbyButton;
    [SerializeField]
    public Button sendMessageButton;
    TwoWayConnectionEstablisher twoWayConnection;
    ChatSocketCommunication chatConnection;
    [SerializeField]
    public PlayerDatabase PlayerDatabaseInstance;
    string bufferedChatMessages;
//    public bool InLobby => LobbyData.HasValue;
    public Lobby LobbyData;

    private bool _characterIndex;

    public bool CharacterIndex
    {
        get { return _characterIndex; }
        set { _characterIndex = value; }
    }

    public void Start()
    {
        twoWayConnection = FindObjectOfType<TwoWayConnectionEstablisher>();
        chatConnection = FindObjectOfType<ChatSocketCommunication>();
        chatConnection.OnTextMessageReceived += AppendToTextBox;
        chatConnection.OnTextMessageSend += AppendToTextBox;
        chatTextBox.text = "Chat Initialized";

        if (ClientData.Player is null)
        {
            ClientData.Player = new PlayerData();
            PlayerDatabaseInstance.CreateNewPlayerDataInDatabase();

            //PlayerDatabase.CurrentPlayerData.SocketSenderPort = ((IPEndPoint) (SocketComunication.Sender.LocalEndPoint)).Port;
            ClientData.Player.SocketReceiverPort = ((IPEndPoint)(SocketComunication.ConnectionListener.LocalEndPoint)).Port;
            ClientData.Player.IP = ((IPEndPoint)(SocketComunication.ConnectionListener.LocalEndPoint)).Address.ToString();
            ClientData.Player.SocketReceiverPort = ((IPEndPoint)(SocketComunication.ConnectionListener.LocalEndPoint)).Port;
            ClientData.Player.SocketChatReceiverPort = ((IPEndPoint)(ChatSocketCommunication.ChatListener.LocalEndPoint)).Port;
            PlayerDatabaseInstance.WritePlayerData(ClientData.Player);
        }
        else
        {
            //PlayerDatabase.CurrentPlayerData.SocketSenderPort = ((IPEndPoint) (SocketComunication.Sender.LocalEndPoint)).Port;
            ClientData.Player.SocketReceiverPort = ((IPEndPoint)(SocketComunication.ConnectionListener.LocalEndPoint)).Port;
            ClientData.Player.IP = ((IPEndPoint)(SocketComunication.ConnectionListener.LocalEndPoint)).Address.ToString();
            ClientData.Player.SocketReceiverPort = ((IPEndPoint)(SocketComunication.ConnectionListener.LocalEndPoint)).Port;
            ClientData.Player.SocketChatReceiverPort = ((IPEndPoint)(ChatSocketCommunication.ChatListener.LocalEndPoint)).Port;
            ClientData.Player.CreatedLobby = false;
            PlayerDatabaseInstance.UpdatePlayerLog(ClientData.Player);

        }

        chatConnection.OnChatSocketDisconnnected += OnChatExit;
        StartCoroutine(UpdatePlayerListCoroutine());
    }
    //Do not set any unity components directly through the event 
    // as the event may fire on a separated thread and that is 
    // operation would silenetly fail.
    public void OnChatExit()
    {

        PlayerData player =ClientData.Player;
        if (player != null) 
        {
            LobbyData.SetConnectionStatus(false);
            //If not the host of the lobby and the lobby is closed by the host
            if (player.CreatedLobby == false)
            {
                LobbyData.SetCurrent(null);
                //LobbyData.SetOther(null);
            }
            else
            {
                LobbyData.SetOther(null);
            }

        }



    }
    void Update()
    {

        if(Input.GetKeyDown(KeyCode.X))
        {
            ClientData.IsDebugModeEnabled = !ClientData.IsDebugModeEnabled;

        }
        if (SocketComunication.ConnectionListener.IsBound)
        {
            IPEndPoint ip = SocketComunication.ConnectionListener.LocalEndPoint as IPEndPoint;

        }
        createLobbyButton.interactable = !LobbyData.LobbyMinimal;
        joinLobbyButton.interactable =  !LobbyData.LobbyMinimal;
        playerSelectDropdown.interactable = !LobbyData.LobbyMinimal;
        sendMessageButton.interactable = LobbyData.IsConnected;
        // When not on lobby or not connected to another person in the lobby
        if (!LobbyData.IsConnected)
        {
            //Reset chat and buffed messages
            chatTextBox.text = "";
            sendTextMessageInput.text = "";
            bufferedChatMessages = "";

        }
        else 
        {
            chatTextBox.text = bufferedChatMessages;
        }

        string statusString ="";
        if (ClientData.TwoWayConnectionEstablished())
        {
            statusString += $"TWO WAY CONNECTION ESTABLISEHD";
            StartGame();
            return;
        }
        if (SocketComunication.Receiver.Connected)
        {
            IPEndPoint remoteIp = SocketComunication.Receiver.RemoteEndPoint as IPEndPoint;
            IPEndPoint localIp = SocketComunication.Receiver.LocalEndPoint as IPEndPoint;

            statusString += $" {remoteIp.Port} -> {localIp.Port}";

        }

        if (SocketComunication.Sender.Connected)
        {
            IPEndPoint remoteIp = SocketComunication.Sender.RemoteEndPoint as IPEndPoint;
            IPEndPoint localIp = SocketComunication.Sender.LocalEndPoint as IPEndPoint;


            statusString += $" {remoteIp.Port} -> {localIp.Port}";

        }
    }

    public void JoinLobby()
    {
        if (LobbyData.LobbyFull ) { return; }
        if(chatConnection.EstablishChatConnection(LobbyData.Other.SocketChatReceiverPort))
        {
            chatConnection.SendMessageToChat(
                PlayerDatabase.GetDatabaseLogForPlyaer(ClientData.Player));
            Debug.Log("Established Chat Connection in Lobby");
            //Only if joining lobby share the current user details with the host
            LobbyData.SetCurrent(ClientData.Player);
            LobbyData.SetConnectionStatus(true);
        }

    }
    public void ExitLobby()
    {
        //It is possible to not have a player name. Only players with lobbies have names
        //        if(Lobby.Value.Current.PlayerName is not null)
        //            PlayerDatabase.DeletePlayerData(Lobby.Value.Current.PlayerName);
        LobbyData.SetCurrent(null);
        LobbyData.SetConnectionStatus(false);
        if (ClientData.Player != null) 
        {
            if (ClientData.Player.CreatedLobby)
            {
                ClientData.Player.CreatedLobby = false;
                PlayerDatabaseInstance.UpdatePlayerLog(ClientData.Player);

            }
        }
        chatConnection.ResetChatRelatedSockets();
    }
    public void CreateLobby() 
    {
        //This should happen at the beginning 
        //        string playerName = PlayerDatabase.GeneratePlayerName();
        //        PlayerDatabase.CurrentPlayerData.PlayerName = playerName;


        ClientData.Player.CreatedLobby = true;
        if(PlayerDatabaseInstance.UpdatePlayerLog(ClientData.Player))
        {
            LobbyData = new Lobby() { Current = ClientData.Player };
            this.CharacterIndex = true;
        }

    }
    IEnumerator UpdatePlayerListCoroutine()
    {
        while (true)
        {


            string prevSelected = "";
            if(playerSelectDropdown.options.Count != 0)
                 prevSelected = playerSelectDropdown.options[playerSelectDropdown.value].text;
            PlayerDatabaseInstance.UpdatePlayersData();

            playerSelectDropdown.ClearOptions();
            foreach (var playerOption in PlayerDatabaseInstance.PlayerDataList)
            {
                playerSelectDropdown.options.Add(playerOption);

            }
            if(playerSelectDropdown.options.Count == 0)
            {
                playerSelectDropdown.enabled = false;
                if(ClientData.Player.CreatedLobby == false)
                {
                    //Remove the value the other peer ONLY if
                    // this peer has not created the lobby
                    LobbyData.Other = null;

                }
            }
            else
                playerSelectDropdown.value = playerSelectDropdown.options.FindIndex(item => item.text == prevSelected);

            playerSelectDropdown.RefreshShownValue();
            yield return new WaitForSeconds(1);


        }
    }
    public void OnLobbySelected()
    {
        PlayerData option = (PlayerData)playerSelectDropdown.options[playerSelectDropdown.value];
        Debug.Log($"Selected Player Name:{option.PlayerName}, IP:{option.IP}, Port{option.SocketReceiverPort}");
        LobbyData.SetOther(option);

    }


    public void SendTextMessage()
    {
        if(chatConnection == null) return;

        string FormatedMessage = $"[{ClientData.Player.PlayerName}] " +
            $"({ClientData.Player.SocketChatReceiverPort}) " +
            $":: {sendTextMessageInput.text}\n";
        chatConnection.SendMessageToChat(FormatedMessage);
        sendTextMessageInput.text = "";
        
    }

    private void AppendToTextBox(object sender, TextMessageEventArgs e)
    {
        if(e.IsIntroductionMessage)
        {
            Debug.Log($"Introduction Message Is: {e.Message}");
            PlayerData Other = PlayerDatabase.GetPlayerDataFromLine(e.Message);
            if(Other == null) { return; }
            if(Other.PlayerName != LobbyData.Current.PlayerName) 
            {
                LobbyData.SetOther(Other);
                LobbyData.SetConnectionStatus(true);
            }

        }
        else 
        {
            Debug.Log("Message Appended To Box");
            bufferedChatMessages += e.Message;
        }

//        chatTextBox.text += e.Message;
//        chatTextBox.ForceMeshUpdate();
    }

    public void StartGame() 
    {
        ClientData.CharacterIndex = CharacterIndex;
        TwoWayConnection();


        if (ClientData.Player != null) 
        {
            LobbyData.SetConnectionStatus(false);
            ClientData.Player.CreatedLobby = false;
            PlayerDatabaseInstance.UpdatePlayerLog(ClientData.Player);

        }



        if(ClientData.TwoWayConnectionEstablished())
        {
            ClientData.SoloPlay = false;
            SceneManager.LoadScene(1);
        }
        else
        {
            Debug.Log("Two way connection could not be established. ");
        }



    }

    public void StartSoloGame() 
    {
        ClientData.CharacterIndex = CharacterIndex;

        ClientData.SoloPlay=true;
        SceneManager.LoadScene(1);
    }

    
   

    public void EstablishChatConnection() 
    {
        if  (LobbyData.Other != null && LobbyData.IsConnected == false)
        {
            string connectionStr;
            try
            {
                chatConnection.EstablishChatConnection(LobbyData.Other.SocketChatReceiverPort);
            }
            catch (System.Exception)
            {

                connectionStr = "";
                connectionStr += $"Connection failed!";
            }
        }
    }
    public void TwoWayConnection() 
    {
        if ( LobbyData.Other != null && LobbyData.IsConnected == true)
        {
            string connectionStr;
            try
            {
                twoWayConnection.EstablishTwoWayConnection(LobbyData.Other.SocketReceiverPort);
            }
            catch (System.Exception)
            {
                connectionStr = "";
                connectionStr += $"Connection failed!";
            }
        }
    }
}
