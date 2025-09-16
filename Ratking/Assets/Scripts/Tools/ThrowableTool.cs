using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ThrowableTool : ToolBase
{
    public Thrower Thrower => LevelData.PlayerObject.GetComponent<Thrower>();

    public GameObject ObjectToThrow;
    public override bool CanBeSelected()
    {
        if (this.Thrower != null)
        {
            return this.Thrower.ThrowItemPrefab == null;
        }
        return false;
    }

    public override void OnSelected()
    {

        var thrower = LevelData.PlayerObject.GetComponent<Thrower>();
        if (thrower != null)
        {
            thrower.ThrowItemPrefab =ObjectToThrow;
            thrower.OnItemThow += ThrownItem;
            Debug.Log("ToolBase Item setup");
        }
    }

    protected virtual void ThrownItem(GameObject thrownItem, Vector2 impulse)
    {
        this._toolbox.RemoveTool(this._index);
    }
}
