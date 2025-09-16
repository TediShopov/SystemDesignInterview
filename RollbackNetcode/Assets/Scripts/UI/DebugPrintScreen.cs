using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPrintScreen : MonoBehaviour
{
    public int qsize = 15;  // number of messages to keep
    public int OffsetRightPixels = 600;  
    public int Size = 600;  
    public int FontSize = 35;
    Queue myLogQueue = new Queue();

    void Awake()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        myLogQueue.Enqueue("[" + type + "] : " + logString);
        if (type == LogType.Warning)
            myLogQueue.Enqueue(stackTrace);
        while (myLogQueue.Count > qsize)
            myLogQueue.Dequeue();
    }

    void OnGUI()
    {
        if(ClientData.IsDebugModeEnabled)
        {
            GUIStyle headStyle = new GUIStyle();
            headStyle.fontSize = FontSize;
            GUILayout.BeginArea(new Rect(Screen.width - OffsetRightPixels, 0, Size, Screen.height));
            GUILayout.Label("\n" + string.Join("\n", myLogQueue.ToArray()), headStyle);
            GUILayout.EndArea();

        }
    }
}
