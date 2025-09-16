using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingSystemVisuals : MonoBehaviour
{

    private RecipeInstanceVisual[] RecipeVisuals;

   
    // Start is called before the first frame update
    void Start()
    {
        var craftingSystem = LevelData.CraftingSystem;
        var craftingRecipes = craftingSystem.CraftingRecipes.CraftingRecipeList;
        this.RecipeVisuals = this.GetComponentsInChildren<RecipeInstanceVisual>();
        for (int i = 0; i < RecipeVisuals.Length; i++)
        {
            
            if (i < craftingRecipes.Count)
            {
                CraftingRecipe recipe = craftingRecipes[i];
                bool isUnlocked = DataPersistenceManager._gameData.UmlockedRecipesData.UnlockedRecipesIndexes.Contains(i);
                RecipeVisuals[i].UpdateRecipeInfo(recipe, i, isUnlocked);
            }
            else
            {
                RecipeVisuals[i].gameObject.SetActive(false);
            }
        }
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
