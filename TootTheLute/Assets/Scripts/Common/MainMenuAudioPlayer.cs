using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public  class MainMenuAudioPlayer : MonoBehaviour
{
    public static MainMenuAudioPlayer Instance = null;
    public  bool IsMainMenuPlaying = false;
    // Start is called before the first frame update
    void Awake()
    {
        //Only replace the instance when it is null
        // Always keeps the oldest instance
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
