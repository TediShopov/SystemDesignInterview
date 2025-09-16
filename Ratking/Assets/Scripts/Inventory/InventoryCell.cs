using UnityEngine;
using UnityEngine.UI;


public enum InventoryCellState
{
    Default, Valid, Invalid, Occupied
}

public class InventoryCell : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] public Image Borders;
    [SerializeField] public Image Background;

    //Default Colors 
    private Color _defaultBorderColor;
    private Color _defaultBackgroundColor;

    private void Awake()
    {
        _defaultBorderColor = Borders.color;
        _defaultBackgroundColor = Background.color;
    }

    public void ChangeColors(Color bgColor, Color borderColor)
    {
        Borders.color = borderColor;
        Background.color = bgColor;
    }

    public void ResetToDefaultColors()
    {
        Borders.color = _defaultBorderColor;
        Background.color = _defaultBackgroundColor;
    }
}