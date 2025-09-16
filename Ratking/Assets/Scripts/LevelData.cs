using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>
{
    private static T _instance;

    /// <summary>
    /// Returns if there is an instance of this
    /// </summary>
    public static bool HasInstance => _instance != null;

    /// <summary>
    /// Access the singleton instance, will create one if it doesn't exist yet
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_instance == null)
                throw new ArgumentException("No Instance of level data");
                //new GameObject(typeof(T).Name).AddComponent<T>();
            return _instance;
        }
        set
        {
            _instance=value;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this as T;
        DontDestroyOnLoad(this.gameObject);
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}
public class LevelData : MonoBehaviour
{
    [SerializeField] public static GameObject PlayerObject; 
    [SerializeField] public static Inventory Inventory;
    [SerializeField] public static InventoryGridView InventoryView;
    [SerializeField] public static KeyBasedProgression RewardBar;
    [SerializeField] public static KeyBasedProgression DangerBar;
    [SerializeField] public static InteractActionManager InteractActionManager;
    [SerializeField] public static ExtractionMethods ExtractionMethods;
    [SerializeField] public static ButtonPromptManager ButtonPromptManager;
    [SerializeField] public static CraftingSystem CraftingSystem;
    [SerializeField] public static CraftingResourceHolder CraftingResourceHolder;
    [SerializeField] public static Toolbox Toolbox;





    [SerializeField] public  GameObject PlayerObjectStatic;
    [SerializeField] public  Inventory InventoryStatic;
    [SerializeField] public  InventoryGridView InventoryViewStatic;
    [SerializeField] public  KeyBasedProgression RewardBarStatic;
    [SerializeField] public  KeyBasedProgression DangerBarStatic;
    [SerializeField] public  InteractActionManager InteractActionManagerStatic;
    [SerializeField] public  ExtractionMethods ExtractionMethodsStatic;
    [SerializeField] public  ButtonPromptManager ButtonPromptManagerStatic;
    [SerializeField] public  CraftingSystem CraftingSystemStatic;
    [SerializeField] public  CraftingResourceHolder CraftingResourceHolderStatic;
    [SerializeField] public Toolbox ToolboxStatic;




    void Awake()
    {
        LevelData.PlayerObject = this.PlayerObjectStatic;
        LevelData.Inventory = this.InventoryStatic;
        LevelData.InventoryView = this.InventoryViewStatic;
        LevelData.RewardBar = this.RewardBarStatic;
        LevelData.DangerBar = this.DangerBarStatic;
        LevelData.InteractActionManager= this.InteractActionManagerStatic;
        LevelData.ExtractionMethods = this.ExtractionMethodsStatic;
        LevelData.ButtonPromptManager=this.ButtonPromptManagerStatic;
        LevelData.CraftingSystem = this.CraftingSystemStatic;
        LevelData.CraftingResourceHolder = this.CraftingResourceHolderStatic;
        LevelData.Toolbox=this.ToolboxStatic;
    }

    public static KeyBasedProgression GetProgressBar(bool isDanger)
    {
        if (isDanger)
        {
           return DangerBar;
        }
        else
        { 
            return RewardBar;
        }
    }

   // public static Inventory Inventory => PlayerObject.GetComponent<Inventory>();

    // public static PlayerProgressionTracker PlayerProgressionTracker =>  LevelData.PlayerObject.GetComponent<PlayerProgressionTracker>();
    //public static CraftingResourceHolder ResourceHolder =>
    //    LevelData.PlayerObject.GetComponent<CraftingResourceHolder>();


}
