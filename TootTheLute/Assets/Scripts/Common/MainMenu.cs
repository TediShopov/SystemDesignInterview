using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public MainMenuSounds Sounds;
    public bool TryPlayMenuSound=true;
    public bool UseLevelLoader=true;
    //public MainMenuAudioPlayer AudioPlayer;
    // Start is called before the first frame update
    void Start()
    {
        if (TryPlayMenuSound && Sounds.SoundtrackPlay != null)
        {
            if (MainMenuAudioPlayer.Instance.IsMainMenuPlaying == false)
            {
                Sounds.SoundtrackPlay.Post(MainMenuAudioPlayer.Instance.gameObject);
                MainMenuAudioPlayer.Instance.IsMainMenuPlaying = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTestMenuClick()
    {
        if(Sounds)
        {
            uint id = Sounds.OnMenuClick.Post(MainMenuAudioPlayer.Instance.gameObject);
        }

        Debug.Log("Test");
    }
    public void OnInputCalibrationClicked()
    {
        if(Sounds)
        {
            uint id = Sounds.OnMenuClick.Post(MainMenuAudioPlayer.Instance.gameObject);
        }

        if (UseLevelLoader && LevelLoader.Instance != null)
            LevelLoader.Instance.LoadLevel(4);
        else
            SceneManager.LoadScene(4);


        Debug.Log("Input Calibration");

    }
    public void OnStartButtonClicked()
    {
        if(TryPlayMenuSound && Sounds)
        {
            uint id = Sounds.OnMenuClick.Post(MainMenuAudioPlayer.Instance.gameObject);
        }

        if(UseLevelLoader && LevelLoader.Instance != null)
            LevelLoader.Instance.LoadLevel(1);
        else
            SceneManager.LoadScene(1);


        Debug.Log("Start");
    }
    public void OnInfoButtonClicked()
    {
        if(TryPlayMenuSound && Sounds)
        {
            uint id = Sounds.OnMenuClick.Post(MainMenuAudioPlayer.Instance.gameObject);
        }

        if(UseLevelLoader && LevelLoader.Instance != null)
            LevelLoader.Instance.LoadLevel(5);
        else
            SceneManager.LoadScene(5);


        Debug.Log("Start");
    }
    public void OnExitButtonClicked()
    {

        if(TryPlayMenuSound && Sounds)
        {
            uint id = Sounds.OnMenuBack.Post(MainMenuAudioPlayer.Instance.gameObject);
        }
        Application.Quit();
        Debug.Log("Exti");
    }
    public void OnBackToMenuClicked()
    {
        if(TryPlayMenuSound && Sounds && MainMenuAudioPlayer.Instance != null)
        {
            uint id = Sounds.OnMenuBack.Post(MainMenuAudioPlayer.Instance.gameObject);
        }

        if(UseLevelLoader   &&    LevelLoader.Instance != null)
            LevelLoader.Instance.LoadLevel(0);
        else
            SceneManager.LoadScene(0);

    }
}
