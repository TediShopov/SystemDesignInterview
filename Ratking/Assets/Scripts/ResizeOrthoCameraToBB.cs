using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ResizeOrthoCameraToBB : MonoBehaviour
{
    public SmellGrid SmellGrid;

    Bounds  GetSmellGridBounds()
    {
        Bounds bounds = new Bounds();
        bounds.center = this.SmellGrid.transform.position + this.SmellGrid.Grid.cellSize.x * new Vector3(this.SmellGrid.GridDimensions.x/2,this.SmellGrid.GridDimensions.y/2);
        bounds.extents = new Vector3(this.SmellGrid.Grid.cellSize.x * this.SmellGrid.GridDimensions.x,
            this.SmellGrid.Grid.cellSize.x * this.SmellGrid.GridDimensions.y); 
        return bounds;
    }
void OnDrawGizmos()
{
    if (Application.isPlaying == true)
    {


        var b = GetSmellGridBounds();
        Gizmos.DrawWireCube(b.center, b.extents);
    }
}



    void Update()
    {
        var bb = GetSmellGridBounds(); 
        Camera.main.orthographicSize = bb.size.y/4.0f;
        Camera.main.transform.position = bb.center; 
        Camera.main.transform.position += new Vector3(0, 0, -1);
    }
}
