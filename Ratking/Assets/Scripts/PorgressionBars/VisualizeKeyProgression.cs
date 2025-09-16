using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualizeKeyProgression : MonoBehaviour
{
    public bool IsDangerBar=false;
    private KeyBasedProgression Progression;
    public RectTransform Bar;
    public GameObject KeyPrefab;
    public Sprite SpriteOnCompletion;
    public Vector2 KeyScaleMultiplier;
    private List<GameObject> VisualizedKeys;
    void Start()
    {
        Progression = LevelData.GetProgressBar(IsDangerBar);
        VisualizedKeys = new List<GameObject>();
        int keyCount = Progression.MaxKeyCount;
        float keyScale = 1.0f / keyCount;
        float paddingRelValue = (1.0f % keyCount) / (keyCount + 1);
        float positionIncrement = Bar.sizeDelta.x / keyCount;

        float lastPlacedAnchor = 0.0f;
        for (int i = 0; i < keyCount; i++)
        {
            VisualizedKeys.Add(Instantiate(KeyPrefab, this.transform.position, Quaternion.identity, this.transform));
            var progressionSlider = this.VisualizedKeys[i].GetComponentInChildren<Slider>();

            var rectTransformOfKey = VisualizedKeys[i].GetComponent<RectTransform>();

            //Set up anchors 
            rectTransformOfKey.anchorMin = new Vector2(lastPlacedAnchor + 0, 0);
            rectTransformOfKey.anchorMax = new Vector2(lastPlacedAnchor + keyScale, 1);

            rectTransformOfKey.localScale = new Vector3(KeyScaleMultiplier.x, KeyScaleMultiplier.y, 1);
            lastPlacedAnchor = lastPlacedAnchor + keyScale;

            rectTransformOfKey.anchoredPosition = new Vector2(0.5f, 0.5f);

            progressionSlider.minValue = 0;
            progressionSlider.maxValue = Progression.KeyScoreValue;
            progressionSlider.interactable = false;
            //rectTransformOfKey.position =  new Vector3(i,0,0) * positionIncrement;
        }

        //Subscire to KeyBasedProgression ot receive on value changed udpate
        this.Progression.OnProgressionScoreChanged += UpdateKeyCompletionSliders;

    }


    void UpdateKeyCompletionSliders(int currentScore)
    {
        for (int i = 0; i < this.VisualizedKeys.Count; i++)
        {
            var progressionSlider = this.VisualizedKeys[i].GetComponentInChildren<Slider>();
            if (i < Progression.KeysEarned)
            {
                progressionSlider.value = progressionSlider.maxValue;
                // this.VisualizedKeys[i].GetComponent<Image>().sprite = SpriteOnCompletion;
                this.VisualizedKeys[i].GetComponent<Image>().sprite = SpriteOnCompletion;
                this.VisualizedKeys[i].transform.GetChild(0).GetComponent<Image>().sprite = SpriteOnCompletion;
                this.VisualizedKeys[i].transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = SpriteOnCompletion;
            }
            else if (i == Progression.KeysEarned)
            {
                progressionSlider.value = Progression.ProgressionToNextKey();
            }
        }


    }

    // Update is called once per frame
    void Update()
    {

    }
}
