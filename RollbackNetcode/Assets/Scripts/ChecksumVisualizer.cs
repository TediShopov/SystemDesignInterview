using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChecksumVisualizer : MonoBehaviour
{
    public StateManager stateManager;
    public Text Label;
    public Text MyState;
    public Text OpponentsState;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        
    }
    public void VizualizChecksums(InputFrame current,InputFrame other )
    {
        if(ClientData.IsDebugModeEnabled == false)
        {
            Label.text = "";
            MyState.text = "";
            OpponentsState.text = "";

        }
        else
        {
            Label.text = "Chekcsum and State";
            //The game state for the other player is not sent through the
            //network if it is not in explicitly enabled. 
            //So all the other times just the default value is visulized
            MyState.text = "Checksum" + current.Checksum.ToString() + "\n";
            MyState.text += stateManager.GameStateString(current.State);
            OpponentsState.text = other.Checksum.ToString() + "\n";
            OpponentsState.text += "Checksum" + stateManager.GameStateString(other.State);
        }
    }
//    public void VizualizChecksums(GameStatePacket current, GameStatePacket other)
//    {
//        MyState.text = stateManager.GameStateString(current);
//        OpponentsState.text = stateManager.GameStateString(other);
//    }
}
