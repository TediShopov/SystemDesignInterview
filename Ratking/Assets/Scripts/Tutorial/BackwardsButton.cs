using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackwardsButton : MonoBehaviour
{
    void Awake()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
    }
    void TaskOnClick()
    {
        TutorialBox.Instance.PreviousDialogueLine();
    }
}
