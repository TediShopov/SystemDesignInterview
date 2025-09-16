using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance = null;
    public Animator Transition;
    public float transitionTime;
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }

    // Update is called once per fram
    void Update()
    {
        
    }
    public void LoadLevel(int sceneIndex)
    {
        Time.timeScale = 1;
        StartCoroutine(LoadLevelCoroutine(sceneIndex));
    }
    public IEnumerator LoadLevelCoroutine(int sceneIndex)
    {
        Transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(sceneIndex);
    }
}
