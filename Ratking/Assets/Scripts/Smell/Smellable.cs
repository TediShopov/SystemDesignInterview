using SuperTiled2Unity;
using System;
using UnityEngine;


[ExecuteAlways]
[RequireComponent(typeof(BoxCollider2D))]
public class Smellable : MonoBehaviour
{

    [SerializeField][Range(2, 100)] public int SmellRangeInTiles;
    [SerializeField] public Color SmellPathColor;
    [SerializeField][Range(1, 100)] public int MinVisibleSmellTrailTiles;
    [HideInInspector] public SuperMap PFGrid;
    [SerializeField][Range(0.001f, 30.0f)] public float SpeedOfTrail;

    private Grid _grid;
    private BoxCollider2D _smellArea;
    private Vector3 _tileSize;
    public float GetDiagonalSize => Helpers.GetWorldTileSize(_grid, PFGrid.gameObject.transform).magnitude;
    // Start is called before the first frame update
    void Awake()
    {
        PFGrid = GameObject.FindObjectOfType<PathfindingGrid>().SuperTileMap;
        if (PFGrid == null)
        {
            Debug.LogError("Smell Script did not find any pathfinding grid in this level.");
            return;
        }

        _smellArea = gameObject.GetComponent<BoxCollider2D>();
        _grid = PFGrid.GetComponentInChildren<Grid>();
        _tileSize = new Vector3();
        _tileSize.x = _grid.cellSize.x * PFGrid.transform.localScale.x;
        _tileSize.y = _grid.cellSize.x * PFGrid.transform.localScale.x;
    }

    Vector2 GetSmellAreaWorld()
    {
        return new Vector2(this.SmellRangeInTiles * _tileSize.x, this.SmellRangeInTiles * _tileSize.y);
    }


    // Update is called once per frame
    void Update()
    {
        if (_smellArea != null) _smellArea.size = GetSmellAreaWorld() / this.transform.lossyScale;
    }
    //public float GetSmellRange()
    //{
    //    return SmellRangeInTiles * GetDiagonalSize / transform.lossyScale.x;
    //}

    //private void OnDrawGizmosSelected()
    //{
    //   Gizmos.DrawWireSphere(this.transform.position,GetSmellRange());
    //}

    public void OnDrawGizmos()
    {
       // Gizmos.DrawWireCube(this.transform.position, GetSmellAreaWorld());
    }


    //void OnDrawGizmos
}
