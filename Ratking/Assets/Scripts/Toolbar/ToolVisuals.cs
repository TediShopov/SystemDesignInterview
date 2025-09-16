using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ToolVisuals : MonoBehaviour
{
    private Image Image;
    private Sprite _defaultSprite;
    private Color _defaultColor;

    void Awake()
    {
        this._defaultSprite = GetComponent<Image>().sprite;
        this._defaultColor = GetComponent<Image>().color;
        this.Image = GetComponent<Image>();
    }

    public void SetupImage(ToolBase tool)
    {
        this.Image.sprite = tool.SpriteInToolbox;
        
    }

    public void Reset()
    {
        this.Image.sprite=this._defaultSprite;
        this.Image.color=this._defaultColor=this._defaultColor;
    }
}
