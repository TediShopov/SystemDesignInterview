using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ResourceLocation
{
    public string ResourceName;
}


[Serializable]
public class ToolBase : MonoBehaviour
{
    protected Toolbox _toolbox = null;
    protected int _index = -1; 
    public string DisplayName = string.Empty;
    public Sprite SpriteInToolbox;
    public string ResourcePath;


    public virtual void SetInToolbox(Toolbox toolbox, int index)
    {
        _toolbox = toolbox;
        _index = index;
    }

    public virtual bool CanBeSelected()
    {
        return true;
      
    }

    public virtual void OnSelected()
    {
        Debug.Log($"Selected item: {this.gameObject.name}");
    }

 

}
