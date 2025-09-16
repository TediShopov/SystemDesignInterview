using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CraftingRecipe
{
    public string Name;
    public int CostOfUnlocking;
   
    //Create Custom Editor To Coontrol
    [SerializeField] private List<CraftingReosurceType> ResourceTypes = new List<CraftingReosurceType>();
    [SerializeField] private List<int> ResourceAmounts = new List<int>();


    public void SetValues()
    {
        for (int i = 0; i < this.ResourceTypes.Count; i++)
        {
            if (Requirements.ContainsKey(ResourceTypes[i]))
            {
                Requirements[ResourceTypes[i]] = ResourceAmounts[i];
            }
            else
            {
                Requirements.Add(ResourceTypes[i], ResourceAmounts[i]);

            }
        }

    }


    [HideInInspector]
    public Dictionary<CraftingReosurceType, int> Requirements = new Dictionary<CraftingReosurceType, int>()
    {
        { CraftingReosurceType.Iron ,0},
        { CraftingReosurceType.String ,0},
        { CraftingReosurceType.Food , 0}
    };

    public bool HasSufficientMaterials(Dictionary<CraftingReosurceType, int> Resources)
    {
        foreach (var requirement in Requirements)
        {
            if (requirement.Value > Resources[requirement.Key])
            {
                return false;
            }
        }
        return true;
    }

    public ToolBase CraftRecipe(in Dictionary<CraftingReosurceType, int> Resources)
    {
        if (HasSufficientMaterials(Resources))
        {
            foreach (var requirement in Requirements)
            {
                Resources[requirement.Key]-=requirement.Value;
            }
        }

        return MonoBehaviour.Instantiate(ProductType);
    }

    public ToolBase ProductType;
}

[CreateAssetMenu(fileName = "New Crafting Recipes", menuName = "Crafting Recipes")]

public class CraftingRecipes : ScriptableObject
{

    [SerializeField] public List<CraftingRecipe> CraftingRecipeList = new List<CraftingRecipe>();

    void OnEnable()
    {
        foreach (var craftingRecipe in CraftingRecipeList)
        {
            craftingRecipe.SetValues();
        }
    }
}
