using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class UmlockedRecipesData
{
    //Hold indices of unlocked recipes
    public List<int> UnlockedRecipesIndexes=new List<int>();
}

public class CraftingSystem : MonoBehaviour
{

    [SerializeField]
    public CraftingRecipes CraftingRecipes;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CraftRecipe(int recipeIndex)
    {
        var recipe = CraftingRecipes.CraftingRecipeList[recipeIndex];
        if (recipe != null)
        {
            if (recipe.HasSufficientMaterials(LevelData.CraftingResourceHolder.Resources))
            {
                if (LevelData.Toolbox.HasFreeSlot())
                {
                    //Use player resources
                    var tool = recipe.CraftRecipe(LevelData.CraftingResourceHolder.Resources);
                    LevelData.Toolbox.PlaceTool(tool);
                    //DataPersistenceManager.Instance.SaveGame();
                    //DataPersistenceManager.Instance.LoadGame();

                    if (LevelData.CraftingResourceHolder.OnUpdate != null)
                    {
                        LevelData.CraftingResourceHolder.OnUpdate.Invoke();
                    }
                   

                    Debug.Log($"Crafted Recipe {recipe.Name} Index: {recipeIndex}");
                }
                else
                {
                    Debug.Log("Cannot craft recipe without a free slot in the toolbox");

                }

            }
            else
            {
                Debug.Log("Cannot craft recipe with insufficient materials.");
            }
            
        }
    }

    public void UnlockRecipe(int recipeIndex)
    {
        var recipe = CraftingRecipes.CraftingRecipeList[recipeIndex];
        if (recipe != null && CanUnlockRecipe(recipe))
        {
            //Use player resources
            LevelData.Inventory.SubtractFromBaseGold(recipe.CostOfUnlocking);
            //LevelData.Inventory.BaseGoldValue -= recipe.CostOfUnlocking;
            
            //Save to game data perssistent object
            DataPersistenceManager._gameData.UmlockedRecipesData.UnlockedRecipesIndexes.Add(recipeIndex);
            Debug.Log($"Unlocked Recipe {recipe.Name} Index: {recipeIndex} For: {recipe.CostOfUnlocking}");
        }
    }

    public bool CanUnlockRecipe(CraftingRecipe recipe)
    {
        return LevelData.Inventory.BaseGoldValue >= recipe.CostOfUnlocking;
    }




}
