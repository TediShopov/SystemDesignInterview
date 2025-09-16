using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.CompilerServices;

public class PlayerData : TMPro.TMP_Dropdown.OptionData
{
    public string PlayerName;
    public string IP;
    public int SocketReceiverPort;
    public int SocketSenderPort;
    public int SocketChatReceiverPort;
    public bool CreatedLobby;

    
    public PlayerData()
    {
        

    }
    public PlayerData(string playerName)
    {
        this.text = playerName;
        PlayerName = playerName;
    }
    public PlayerData(string playerName, string ip, int socketReceiver, int socketSender, int socketChatReceiver, bool lobby)
    {
        this.text = playerName;
        PlayerName = playerName;
        IP = ip;
        SocketReceiverPort = socketReceiver;
        SocketSenderPort = socketSender;
        SocketChatReceiverPort = socketChatReceiver;
        CreatedLobby = lobby;
    }

}
public class PlayerDatabase : MonoBehaviour
{
//    public PlayerData ClientData.Player;
    public List<PlayerData> PlayerDataList;
    private static string filePath;



    public void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "PlayerData.txt");


    }

    public void CreateNewPlayerDataInDatabase()
    {
        string playerName = GeneratePlayerName();
        ClientData.Player.PlayerName = playerName;
        ClientData.Player.text = playerName;
    }

    public void OnDestroy()
    {

    }


    

    public string GeneratePlayerName()
    {
        int playerNumber =  new System.Random((int)DateTime.Now.Ticks).Next(); 
        string playerName = $"Player{playerNumber}";
        return playerName;
    }
    public bool UpdatePlayerLog(PlayerData data = null) 
    {
        if (data == null)
            data = ClientData.Player;
        if (data == null)
        {
            Debug.LogError("Cannot create a lobby for uninitialized player");
            return false;
        }

        int playerNumber =FindLineOfPlayerData(data);
        ReplaceLineInFile(filePath, playerNumber, GetDatabaseLogForPlyaer(ClientData.Player));
        return ClientData.Player.CreatedLobby;

    }




     void ReplaceLineInFile(string path, int lineNum, string newContent)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("File not found: " + path);
        }

        // Read all lines into a list
        List<string> lines = new List<string>(File.ReadAllLines(path));

        if (lineNum < 1 || lineNum > lines.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(lineNum), "Line number out of range.");
        }

        // Replace the specified line
        lines[lineNum - 1] = newContent;

        // Write the lines back to the file
        File.WriteAllLines(path, lines.ToArray());
    }

    public  string WritePlayerData(PlayerData data = null)
    {
        if (data == null)
            data = ClientData.Player;
        if (data == null)
        {
            Debug.LogError($"Error writing player to mock database.  Current player is not yet initialized");
            return "";
        }
        string playerData = GetDatabaseLogForPlyaer(data);

        try
        {
            // Append the new player data to the file.
            File.AppendAllText(filePath, playerData + Environment.NewLine);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error writing player file: {e.Message}");
        }

        return playerData;
    }

    public static string GetDatabaseLogForPlyaer(PlayerData data)
    {
        return $"{data.PlayerName},{data.IP},{data.SocketReceiverPort},{data.SocketSenderPort},{data.SocketChatReceiverPort},{data.CreatedLobby}";
    }

    public void UpdatePlayersData()
    {
        PlayerDataList = ReadPlayerData()
            .Where(x => x.text != ClientData.Player.text)
            .Where(x => x.CreatedLobby == true).ToList();
    }

    public  int FindLineOfPlayerData(PlayerData data)
    {
        List<PlayerData> playerDataList = new List<PlayerData>();

        if (!File.Exists(filePath))
            return -1;

        try
        {
            // Read all lines from the file and parse each line into a PlayerData struct
            string[] lines = File.ReadAllLines(filePath);

            int counter = 0;
            foreach (string line in lines)
            {
                counter++;
                PlayerData playerData = GetPlayerDataFromLine(line);
                if(playerData is not null && playerData.PlayerName.Equals(data.PlayerName))
                {
                    return counter;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reading player data file: {e.Message}");
        }
        return -1;
    }
    public  List<PlayerData> ReadPlayerData()
    {
        List<PlayerData> playerDataList = new List<PlayerData>();

        if (!File.Exists(filePath))
        {
            return playerDataList; // Return empty list if file doesn't exist
        }

        try
        {
            // Read all lines from the file and parse each line into a PlayerData struct
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                PlayerData playerData = GetPlayerDataFromLine(line);
                if(playerData != null) 
                {
                    playerDataList.Add(playerData);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reading player data file: {e.Message}");
        }

        return playerDataList;
    }
    public static PlayerData GetPlayerDataFromLine(string line) 
    {
        PlayerData data = null;
        string[] parts = line.Split(',');
        if (parts.Length == 6)
        {
            string playerName = parts[0];
            string ip = parts[1];
            if (int.TryParse(parts[2], out int socketReceiver) &&
                int.TryParse(parts[3], out int socketSender) &&
                bool.TryParse(parts[5], out bool hasLobby) &&
                int.TryParse(parts[4], out int socketChatReceiver))
            {
                 data = new PlayerData(playerName, ip, socketReceiver, socketSender, socketChatReceiver, hasLobby);
            }

        }
        return data;
    }
    private  int GetNextPlayerNumber()
    {
        if (!File.Exists(filePath))
        {
            return 1;
        }

        try
        {
            // Read the file to get the last line and parse the player number.
            string[] lines = File.ReadAllLines(filePath);
            return lines.Length + 1;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reading player file: {e.Message}");
        }

        return 1; // Default to Player1 if there's an error.
    }

     public static void DeletePlayerData(string playerName)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Player data file does not exist.");
            return;
        }

        // Read all lines from the file
        var lines = File.ReadAllLines(filePath);

        // Filter out the line that contains the specified player name
        var updatedLines = lines.Where(line => !line.Contains(playerName)).ToArray();

        // Write the remaining lines back to the file
        File.WriteAllLines(filePath, updatedLines);

        Console.WriteLine($"Deleted data for player: {playerName}");
    }
}
