using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;



//Responsilbe for generating the hash of the current game state
public class StateManager : MonoBehaviour
{
    List<GameObject> GameObjectStates;
    private HealthScript PlayerRBHealth => StaticBuffers.Instance.PlayerOneRB.GetComponent<HealthScript>();
    private HealthScript EnemyRBHealth => StaticBuffers.Instance.PlayerTwoRB.GetComponent<HealthScript>();
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
     public  byte[] GenerateStateHash()
    {
        var PlayerPosition = StaticBuffers.Instance.PlayerOne.transform; 
        var EnemyPosition = StaticBuffers.Instance.PlayerTwo.transform; 
        string stateData = $"{PlayerPosition.position.x},{PlayerPosition.position.y},{PlayerPosition.position.z}," +
                           $"{EnemyPosition.position.x},{EnemyPosition.position.y},{EnemyPosition.position.z}," +
                           $"{PlayerRBHealth.MinHealth},{PlayerRBHealth.CurrentHealth},{PlayerRBHealth.MaxHealth}" +
                           $"{EnemyRBHealth.MinHealth},{EnemyRBHealth.CurrentHealth},{EnemyRBHealth.MaxHealth}";

        // Using a common encryption algorithm 
        using (MD5 md = MD5.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(stateData);
            byte[] hash = md.ComputeHash(bytes);
            return hash;

        }
    }
    public GameStatePacket GetGameState()
    {
        GameStatePacket gsp = new GameStatePacket()
        {
            PlayerOne = StaticBuffers.Instance.PlayerOneRB.GetComponent<FighterController>().State.Position,
            PlayerTwo = StaticBuffers.Instance.PlayerTwoRB.GetComponent<FighterController>().State.Position,
            HealtPlayerOne = PlayerRBHealth.CurrentHealth,
            HealtPlayerTwo = EnemyRBHealth.CurrentHealth,
        };
        return gsp;

    }
    //This is usefull when trying to see what is the exact thing 
    //making the difference in the checkum. (E.g if box 2d is introducing non-deterministic float 
    // operations)
    public string GameStateString() 
    {
        return GameStateString(GetGameState());
    }
    public string GameStateString(GameStatePacket  gsp) 
    { 
        string playerPos = "Player Position: " +gsp.PlayerOne.ToString() +"\n";
        string enemyPos = "Enemy Position: " +gsp.PlayerTwo.ToString() +"\n";
        string playerHealth = "Player Health: " +gsp.HealtPlayerOne  +"\n";
        string enemyHealth = "Enemy Health: " + gsp.HealtPlayerTwo + "\n";
        string checksum = "Checksum: " + GenerateChecksum(gsp) + "\n";
        return playerPos + enemyPos + playerHealth + enemyHealth + checksum;
    }

    public int GenerateChecksum(GameStatePacket gsp)
    {
        int hash = 17;
        hash = hash * 31 + gsp.PlayerOne.ToString().GetHashCode();
        //hash = hash * 31 + StaticBuffers.Instance.PlayerTwo.transform.position.GetHashCode();
        hash = hash * 31 + gsp.PlayerTwo.ToString().GetHashCode();
        hash = hash * 31 + gsp.HealtPlayerOne;
        hash = hash * 31 + gsp.HealtPlayerTwo;
        return hash;


    }
    public int LastConfirmFrameCheckSum = 0;
    //We cannot simply generate checksum from the current game state
    // as in the current game state we might have prediction which might be false.
    public int TryUpdateCheckRelevantChecksum()
{
        LastConfirmFrameCheckSum = GenerateChecksum(GetGameState());
        return LastConfirmFrameCheckSum;
    }
    public  string FormatHasString(byte[] hash)
    {
        StringBuilder hashString = new StringBuilder();
        foreach (byte b in hash)
        {
            hashString.Append(b.ToString("x2"));
        }

        return hashString.ToString();
    }
    public  bool AreHashesEqual(byte[] hash1, byte[] hash2)
    {
        if (hash1 == null || hash2 == null) return false;
        if (hash1.Length != hash2.Length) return false;

        return hash1.SequenceEqual(hash2);
    }
}
