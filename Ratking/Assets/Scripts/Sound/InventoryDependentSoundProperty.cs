using System;
using UnityEngine;

[RequireComponent(typeof(Properties))]
public class InventoryDependentSoundProperty : MonoBehaviour
{

    private Properties _properties;
   

    [SerializeField] public float DefaultSoundProperty;

    [Range(0, 1.0f)]
    [SerializeField] public float MuffleSound;
    // Start is called before the first frame update
    void Start()
    {
        _properties = this.gameObject.GetComponent<Properties>();
        LevelData.Inventory.OnInventoryUpdated += UpdateSoundProperty;
        UpdateSoundProperty();
    }

    public float GetSoundModifier(GameObject obj)
    {
        if (obj == null)
        {
            return 1;
        }

        var properties = obj.GetComponent<Properties>();
        if (properties != null)
        {
            return properties.SoundModifier;
        }



        properties = obj.GetComponentInParent<Properties>();
        if (properties != null)
        {
            return properties.SoundModifier;
        }
        return 1;
    }

    void UpdateSoundProperty()
    {
        var soundModifer = 0.0f;
        foreach (var item in LevelData.Inventory.Items)
        {
            soundModifer += GetSoundModifier(item.ItemData.PrefabReference);
        }
        //TODO make sure that invenotry items still affect sound  produced
        //if (this.Inventory.ItemToPlace != null)
        //{
        //    soundModifer += GetSoundModifier(this.Inventory.ItemToPlace.ItemData.PrefabReference);
        //}

        soundModifer *= MuffleSound;
        soundModifer += DefaultSoundProperty;
        this._properties.SoundModifier = soundModifer;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
