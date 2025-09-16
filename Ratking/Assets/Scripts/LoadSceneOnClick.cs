using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadSceneOnClick : SceneVerifierBase
{
    //// Start is called before the first frame update
    ////public Scene Scene;
    //public Button YourButton;

    //void Start()
    //{
    //    Button btn = YourButton.GetComponent<Button>();

    //    btn.onClick.AddListener(TaskOnClick);
    //}
    public void LoadScene()
    {
        //SceneLoader.Load(SceneLoader.Scene.FinalScene);
        if (IsValid)
        {
            DataPersistenceManager.Instance.SaveGame();
            SceneManager.LoadScene(this.sceneIndex);
        }
        //SceneManager.LoadScene("Play Party 2");
    }
}
