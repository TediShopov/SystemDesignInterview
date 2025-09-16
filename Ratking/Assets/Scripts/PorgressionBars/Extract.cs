using UnityEngine;

public class Extract : MonoBehaviour
{
    public Canvas ExtractionMethodCanvas;
    // Start is called before the first frame update

    void OnTriggerEnter2D(Collider2D collider2D)
    {

        if (collider2D.gameObject.layer == 7)
        {
            //SceneLoader.Load(SceneLoader.Scene.GameOverScene);
            ExtractionMethodCanvas.GetComponent<ExtractionMethods>().ExtractionPoint = this.gameObject;

            ExtractionMethodCanvas.gameObject.SetActive(true);

            //SceneManager.LoadScene("GameOverScene");
        }
    }

    void OnTriggerExit2D(Collider2D collider2D)
    {

        if (collider2D.gameObject.layer == 7)
        {
            //SceneLoader.Load(SceneLoader.Scene.GameOverScene);
            ExtractionMethodCanvas.GetComponent<ExtractionMethods>().ExtractionPoint = null;

            ExtractionMethodCanvas.gameObject.SetActive(false);

            //SceneManager.LoadScene("GameOverScene");
        }
    }
}
