using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

//A static class containing all the input buffers 
//and references to the fighters and their rollback "shadows"
public class StaticBuffers : MonoBehaviour
{
    [SerializeField]
    public GameObject Player;
    [SerializeField]
    public GameObject Enemy;
    [SerializeField]
    public GameObject EnemyRB;
    [SerializeField]
    public StateManager StateManager;

    //This is the player that initiated the connection
    public GameObject PlayerOne => ClientData.CharacterIndex ? Player : Enemy;
    public GameObject PlayerTwo => ClientData.CharacterIndex ? Enemy : Player;
    public GameObject PlayerOneRB => ClientData.CharacterIndex ? PlayerRB : EnemyRB;
    public GameObject PlayerTwoRB => ClientData.CharacterIndex ? EnemyRB : PlayerRB;


    [SerializeField]
    public GameObject PlayerRB;

    private InputBuffer _serializedPlayerBuffer=null;
     public InputBuffer PlayerBuffer { get 
        {
            return _serializedPlayerBuffer; 
        } }
    private InputBuffer _serializedEnemyBuffer=null;

    private InputBuffer _serializedEnemyRBBuffer = null;

    //Do not use unity API here
    public InputBuffer EnemyBuffer { get 
        {
            return _serializedEnemyBuffer;
        }
    }
    public InputBuffer EnemyRBBuffer { get 
        {
            return _serializedEnemyRBBuffer;
        }
    }

    public void RenewBuffers() 
    {
        _serializedPlayerBuffer = Player.GetComponent<FighterController>().InputBuffer;
        _serializedEnemyBuffer = Enemy.GetComponent<FighterController>().InputBuffer;
        _serializedEnemyRBBuffer = EnemyRB.GetComponent<FighterController>().InputBuffer;
    }

    public static StaticBuffers Instance;
    void Awake()
    {
        Instance = this;
        RenewBuffers();
    }

}

