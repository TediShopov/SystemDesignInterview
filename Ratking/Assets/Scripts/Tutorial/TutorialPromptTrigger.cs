using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialPromptTrigger : MonoBehaviour
{
    [TextArea(1, 3)]
    [SerializeField] private string[] _tutorialTip;

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (_tutorialTip == null)
        {
            Debug.LogError("Tutorial Tip not found");
            return;
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (TutorialBox.Instance == null)
                Debug.LogError("Tutorial Instance is null");
            else
            {
                TutorialBox.Instance.ActivateTutorialBox(_tutorialTip);
                Destroy(this.gameObject);
            }
        }

    }

   
}
