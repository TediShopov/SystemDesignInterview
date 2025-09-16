using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A class containing usefull static data for the client
public enum GameState
{
    NotYetStarted = 0,
    Runing = 1,
    Paused = 2,
    Finished = 3,
}

public class ClientData : MonoBehaviour
{
    public static bool SoloPlay = true;
    public static bool CharacterIndex { get; set; }                         //if player plays with first  or second character
    public static bool IsClientInitiator { get; set; } = false;             //if player initiated connection
    public static bool IsDebugModeEnabled { get; set; } = false;            
    public static int PlayerHash { get; set; }

    public static string GameOverMessage = "";

    public static GameState GameState = GameState.NotYetStarted;

    public static PlayerData Player;

    public static readonly KeyCode[] AllowedKeys =
        { KeyCode.Space,
        KeyCode.S,
        KeyCode.A,
        KeyCode.D,
        KeyCode.J,
        KeyCode.K };

    public static readonly Dictionary<KeyCode, int> AllowedKeysIndex = new Dictionary<KeyCode, int>
        { {KeyCode.Space, 0 },
        {KeyCode.S, 1 },
        {KeyCode.A, 2 },
        {KeyCode.D, 3 },
        { KeyCode.J,4 },
     { KeyCode.K,5 }};

    //Returns true if both the sender and receiver socket are bound
    public static bool TwoWayConnectionEstablished()
    {
        if (SocketComunication.Receiver == null || SocketComunication.Sender == null)
        {
            return false;
        }
        return SocketComunication.Receiver.Connected &&
       SocketComunication.Sender.Connected;
    }

}