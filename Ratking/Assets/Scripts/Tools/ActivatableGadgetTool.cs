using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatableGadgetTool : ThrowableTool
{
    public ActivatorTool ActivatorTool;
    protected override void ThrownItem(GameObject thrownItem, Vector2 impulse)
    {
        base.ThrownItem(thrownItem, impulse);

        ActivatorTool = Instantiate(ActivatorTool);
        ActivatorTool.Activatable = thrownItem.GetComponent<Activatable>();
        this._toolbox.PlaceTool(ActivatorTool, this._index);
    }
}
