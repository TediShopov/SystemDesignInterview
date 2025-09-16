using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolbarVisuals : MonoBehaviour
{
    private ToolVisuals[] _toolVisuals;
    // Start is called before the first frame update
    void Awake()
    {
         LevelData.Toolbox.OnToolboxUpdated += RefreshVisuals;
        LevelData.Toolbox.OnLoaded += OnToolboxLoaded;
    }

    void OnDestroy()
    {
        LevelData.Toolbox.OnToolboxUpdated -= RefreshVisuals;
        LevelData.Toolbox.OnLoaded -= OnToolboxLoaded;

    }

    public void OnToolboxLoaded(GameData data)
    {
        RefreshVisuals();
    }

    public void RefreshVisuals()
    {
        this._toolVisuals = this.gameObject.GetComponentsInChildren<ToolVisuals>();

        for (int i = 0; i < LevelData.Toolbox.Slots; i++)
        {
            var tool = LevelData.Toolbox.GetTool(i+1);
            if (tool != null)
            {
                if (i >= 0 && i < _toolVisuals.Length)
                {
                    this._toolVisuals[i].SetupImage(tool);
                }
            }
            else
            {
                this._toolVisuals[i].Reset();
            }
        }
    }


}
