using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[ExecuteAlways]
public class SceneVerifierBase : MonoBehaviour
{
    [SerializeField] public int sceneIndex;
    public string SceneName;
    [SerializeField] public bool IsValid = false;

    void Start()
    {
        if (IsValid == false)
        {
            Debug.LogError("Invalid Scene Parameter");
        }
    }

    void OnValidate()
    {
        //return -1 before if scene doesnt exist
        string path = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(sceneIndex));
        SceneName = path;
        if (path != "")
        {
            IsValid = true;
        }
        else
        {
            IsValid = false;
        }
    }

}



[ExecuteAlways]
public class StartLevel : SceneVerifierBase
{
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            DataPersistenceManager.Instance.SaveGame();
            SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
        }
    }
}
