using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;


public class TutorialBox : MonoBehaviour
{
    // Start is called before the first frame update

    public static TutorialBox Instance { get; private set; }
    private TextMeshProUGUI _myText = null;
    private List<string> _tutorialLines;
    public int CurrentElement;

    [SerializeField]private ForwardButton _forwardButton;
    [SerializeField]private BackwardsButton _backwardsButton;
    public enum TutorialAction
    {
        ItemInRange,
        OpenInventory,
        Detected,
        Suspicion
    }

    [SerializeField] TooltipTexts _tooltipTexts;
    public static Dictionary<TutorialAction, string[]> TutorialDictionary = new Dictionary<TutorialAction, string[]>();

    void Awake()
    {

        if (Instance != null && Instance != this)
        {
            
            Destroy(this);
        }
        else
        {

            Instance = this;
        }
        //TutorialDictionary.Add(TutorialAction.ItemInRange, _tooltipTexts.ItemPickupText);
        //TutorialDictionary.Add(TutorialAction.OpenInventory, _tooltipTexts.OpenInventoryText);
        //TutorialDictionary.Add(TutorialAction.Detected, _tooltipTexts.DetectedText);
        //TutorialDictionary.Add(TutorialAction.Suspicion, _suspicionText);
        //_forwardButton = GetComponentInChildren<ForwardButton>();
        //_backwardsButton = GetComponentInChildren<BackwardsButton>();
        this.gameObject.SetActive(false);

    }

    public void ActivateTutorialBox(string[] tutorialText)
    {
        Time.timeScale = 0;
        _tutorialLines = new List<string>();
        foreach (var element in tutorialText)
        {
            _tutorialLines.Add(element);
        }
        
        CurrentElement = 0;
        _myText = GetComponentInChildren<TextMeshProUGUI>();
        _myText.text = _tutorialLines[CurrentElement];
        UpdateTutorialNavigationButtons();
        Instance.gameObject.SetActive(true);
    }

    public void NextDialogueLine()
    {
        ++CurrentElement;
        _myText.text = _tutorialLines[CurrentElement];

        UpdateTutorialNavigationButtons();
       //_forwardButton.gameObject.SetActive(true);
    }

    public void PreviousDialogueLine()
    {
        --CurrentElement;
        _myText.text = _tutorialLines[CurrentElement];

        UpdateTutorialNavigationButtons();
    }
    public void SetActionTutorialText(TutorialAction key)
    {
        if (TutorialDictionary.ContainsKey(key))
        {
            ActivateTutorialBox(TutorialDictionary[key]);
            TutorialDictionary.Remove(key);
        }
    }

    private void UpdateTutorialNavigationButtons()
    {
        if (CurrentElement == 0)
            _backwardsButton.gameObject.SetActive(false);
        else if (!_backwardsButton.isActiveAndEnabled)
            _backwardsButton.gameObject.SetActive(true);

        if (CurrentElement == _tutorialLines.Count - 1)
            _forwardButton.gameObject.SetActive(false);
        else if (!_forwardButton.isActiveAndEnabled)
            _forwardButton.gameObject.SetActive(true);
    }
}
