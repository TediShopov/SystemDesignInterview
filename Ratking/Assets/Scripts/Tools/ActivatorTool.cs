using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ActivatorTool : ToolBase
{
    public Activatable Activatable;
    

    public override bool CanBeSelected()
    {
        return true;
    }

    public override void OnSelected()
    {
        this.Activatable.Activate();
        this._toolbox.RemoveTool(this._index);
    }

    
}
