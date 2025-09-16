using System.Collections.Generic;
using UnityEngine;



public class ButtonPromptManager : MonoBehaviour
{
    public Dictionary<KeyCode, GameObject> ButtonPrompts;

    public GameObject ButtonPromptPrefab;


    public void SpawnOrUpdate(KeyCode key, Vector3 worldPos, GameObject promptPrefab)
    {
        if (ButtonPrompts.ContainsKey(key))
        {
            var buttonPromptGenerated = Instantiate(promptPrefab, worldPos, Quaternion.identity, this.transform);
            buttonPromptGenerated.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
            Destroy(ButtonPrompts[key]);
            ButtonPrompts[key] = buttonPromptGenerated;
        }
        else
        {
            var buttonPromptGenerated = Instantiate(promptPrefab, worldPos, Quaternion.identity, this.transform);
            buttonPromptGenerated.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
            ButtonPrompts.Add(key, buttonPromptGenerated);
        }
    }

    public void Remove(KeyCode key)
    {
        if (ButtonPrompts.ContainsKey(key))
        {
            Destroy(ButtonPrompts[key]);
            ButtonPrompts.Remove(key);
        }
    }

    void Start()
    {
        ButtonPrompts = new Dictionary<KeyCode, GameObject>();
    }


}
