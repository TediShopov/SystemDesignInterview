using UnityEngine;

[CreateAssetMenu(fileName = "New Interactive Grid Pallete", menuName = "Interactive Grid Pallete")]
public class GridInteractionColors : ScriptableObject
{
    public Color ValidPlacementBorderColor;
    public Color ValidPlacementBackgroundColor;

    public Color InValidPlacementBorderColor;
    public Color InValidPlacementBackgroundColor;

}
