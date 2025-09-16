using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeInstanceVisual : MonoBehaviour
{
    [SerializeField] private Button CraftButton;
    [SerializeField] private Button UnlockButton;
    [SerializeField] private TMP_Text RecipeName;
    [SerializeField] private TMP_Text IronLabel;
    [SerializeField] private TMP_Text StringLabel;
    [SerializeField] private TMP_Text FoodLabel;

    public Color InsufficeintColor;
    public Color SufficientColor;

    private CraftingRecipe _recipe;
    private int _index;

    public void UpdateRecipeInfo(CraftingRecipe recipe,int index = 0, bool isUnlocked=false)
    {

        CraftButton.onClick.RemoveAllListeners();
        UnlockButton.onClick.RemoveAllListeners();

        _recipe = recipe;
        _index= index;
        RecipeName.text = recipe.Name;
        UnlockButton.GetComponentInChildren<TMP_Text>().text = recipe.CostOfUnlocking.ToString();

        SetupLabel(IronLabel,CraftingReosurceType.Iron);
        SetupLabel(StringLabel, CraftingReosurceType.String);
        SetupLabel(FoodLabel, CraftingReosurceType.Food);

        CraftButton.interactable = isUnlocked;
        UnlockButton.gameObject.SetActive(!isUnlocked);
        UnlockButton.interactable = LevelData.CraftingSystem.CanUnlockRecipe(recipe);

        CraftButton.onClick.AddListener(CraftRecipe);
        UnlockButton.onClick.AddListener(UnlockRecipe);
    }

    void SetupLabel(TMP_Text Label, CraftingReosurceType type)
    {
        Label.text = _recipe.Requirements[type].ToString();
        Label.color = ColorBasedOnAvailability(type, LevelData.CraftingResourceHolder);
    }

    Color ColorBasedOnAvailability(CraftingReosurceType type, CraftingResourceHolder holder)
    {
        int needed = _recipe.Requirements[type];
        int inStorage = holder.Resources[type];
        if (inStorage>=needed)
        {
            return SufficientColor;
        }

        return InsufficeintColor;
    }

    void CraftRecipe()
    {
        LevelData.CraftingSystem.CraftRecipe(_index);
    }

    void UnlockRecipe()
    {
        //Actually update the recipe as unlocked ...
       
        if (_index >= 0)
        {
            LevelData.CraftingSystem.UnlockRecipe(_index);
            UpdateRecipeInfo(_recipe, _index, true);
        }
    }

   
}
