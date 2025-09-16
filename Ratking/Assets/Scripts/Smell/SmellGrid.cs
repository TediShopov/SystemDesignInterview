using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Grid))]
public class SmellGrid : MonoBehaviour
{
    public Vector2Int GridDimensions;
    public float MaxVisualDensity;
    public float TickSeconds;
    public float FixedDeltaTime;
    public float DiffusionFactor;
    public float FlatDensityRemove;

    public float AssigneToCellOnClick;
[Range(1,10)]
    public int RangeOfAssignedVelocities;
    public Grid Grid;

    private Vector3 _world_position;
    private Vector3Int cell_pointed;
    private Vector3Int prev_cell_pointed;

    private float[,] obstacles;
    private float[,] smells_prev;
    private float[,] smells;

    private Vector2[,] vel_field;
    //private float[,] def_kernel;

    // Start is called before the first frame kupdate
    void Awake()
    {
        this.Grid = GetComponent<Grid>();
        obstacles = new float[GridDimensions.x, GridDimensions.y];
        for (int i = 1; i < GridDimensions.x-1; i++)
        {
            for (int j = 1; j < GridDimensions.y-1; j++)
            {

                obstacles[i, j] = 1;
            }
        }

        smells_prev = new float[GridDimensions.x, GridDimensions.y];
        smells = new float[GridDimensions.x, GridDimensions.y];
        vel_field = new Vector2[GridDimensions.x, GridDimensions.y];
//        def_kernel = CreateCrossKernel(PortionTransferPerCell);
        this.StartCoroutine(UpdateSmellGrid());

    }

    //Assigns the ratio of density share per cell to a 3x3 kernel
    float[,] CreateCrossKernel(int partitions)
    {
        float densitySharePerCell =1/( (float)partitions * 4); 
        float[,] toReturn = new float[3, 3];
        toReturn[0, 1] = densitySharePerCell;
        toReturn[1, 0] = densitySharePerCell;
        toReturn[1, 2] = densitySharePerCell;
        toReturn[2, 1] = densitySharePerCell;
        return toReturn;
    }

    void MatchGridAtoB(float[,] a, float[,] b)
    {
        for (int i = 0; i < GridDimensions.x; i++)
        {
            for (int j = 0; j < GridDimensions.y; j++)
            {
                a[i, j] = b[i, j];
            } 
        }
    }
    
    //Transfer amount of density in a cell from a given grid toa adjacent cell in another grid.
    //Useful when updating density for the next loop
    //Does not update remove the density
//    void UpdateCellsFrom(int x, int y, float[,] from, float[,] to, float[,] kernel)
//    { 
//        float originalDensity = from[y,x];
//        if (originalDensity>0)
//        {
//         Debug.Log($"Transfering From: X:{x} Y:{y}");
//        float densityToRemove = 0;
//        for (int i = 0; i < kernel.GetLength(1); i++)
//        {
//            for (int j = 0; j < kernel.GetLength(0); j++)
//            {
//                //TODO we only transfer a certain percent of the density value to the adjacent cells, so the value
//                // never reaches zero 
//                float transferAmount = originalDensity * kernel[i,j];
//                int coordRow = y + (i - 1);
//                int coordCol = x + (j - 1);
//                to[y + (i - 1), x + (j - 1)] += transferAmount;
//                densityToRemove += transferAmount;
//                Debug.Log($"     To: X:{coordCol} Y:{coordRow}");
//            }
//        }
//        }
//   }
//

    float PossibleDiffuse(int x, int y)
    {
        return smells_prev[x, y] * obstacles[x, y];
    }

    void Diffuse(int x, int y )
    { 
         float transferToAdjacent =
            obstacles[x, y + 1] + obstacles[x, y - 1] + obstacles[x + 1, y] + obstacles[x - 1, y];
        float t = PossibleDiffuse(x, y + 1);
        float b = PossibleDiffuse(x, y - 1);
        float l = PossibleDiffuse(x + 1, y);
        float r = PossibleDiffuse(x - 1, y);
        smells[x, y] = smells_prev[x, y] + DiffusionFactor * (
            t + b + l + r); //add smell from adjacent grid cells
       smells[x, y] -= smells_prev[x,y] * DiffusionFactor * transferToAdjacent;
    }

    void Advection(int x, int y)
    {
        Vector2Int current = new Vector2Int(x,y);
        Vector2Int targetCell = current;
        targetCell.x += (int)(Math.Ceiling(vel_field[x,y].x)) ;
        targetCell.y += (int)(Math.Ceiling(vel_field[x,y].y)) ;
        
        smells[targetCell.x, targetCell.y] = smells_prev[x, y];

    }

    void RemoveFlat(int x, int y)
    {
        smells[x, y] = Math.Max(0, smells[x, y] - FlatDensityRemove);
            //if (smells[x,y]<FlatDensityRemove)
            //{
            //    smells[x,y]=0;
            //}
    }

    void ForEachInnerCell(Action<int, int> action)
    {
        for (int i = 1; i < GridDimensions.x-1; i++)
        {
            for (int j = 1; j < GridDimensions.y-1; j++)
            {
                action(j,i);
            }
        }


    }

    void UpdateSmellDensities()
    {
        MatchGridAtoB(smells,smells_prev);
        ForEachInnerCell(Diffuse);
        MatchGridAtoB(smells_prev,smells);
        ForEachInnerCell(Advection);
        ForEachInnerCell(RemoveFlat);
        MatchGridAtoB(smells_prev,smells);
    }


    void OnDrawGizmos()
    {
        if (Application.isPlaying==true)
        {
            Gizmos.DrawWireSphere(_world_position, 1.0f);
            for (int i = 0; i < GridDimensions.x; i++)
            {
                for (int j = 0; j < GridDimensions.x; j++)
                {
                    if (obstacles[i,j]==0)
                    {
                        Gizmos.color = Color.blue;
                    }
                    else
                    {
                        Color densityColor = new Color(smells_prev[i, j] / MaxVisualDensity, 0, 0); 
                        Gizmos.color = densityColor;
                    }
                    
                    Vector3 worldPos = Grid.GetCellCenterWorld(new Vector3Int(i, j, 0));
                    Gizmos.DrawWireSphere(worldPos,Grid.cellSize.x/2.0f);
                    //Handles.Label(worldPos, $"{smells_prev[i, j] }");
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(worldPos, vel_field[i,j]);
                }
            }
        }
    }

    void AssignVelocities(int x, int y, Vector2Int dir)
    {
        int leftB = Math.Max(0, (int)(x - RangeOfAssignedVelocities / 2));
        int rightB = Math.Min(GridDimensions.x-1, (int)(x + RangeOfAssignedVelocities / 2));
        int bottomB = Math.Max(0, (int)(y - RangeOfAssignedVelocities / 2));
        int topB = Math.Min(GridDimensions.y-1, (int)(y + RangeOfAssignedVelocities / 2));
        for (int i = bottomB; i <= topB; i++)
        {
            for (int j = leftB; j <= rightB; j++)
            {
                vel_field[j, i] = dir;
            } 
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = Camera.main.nearClipPlane;
        _world_position = Camera.main.ScreenToWorldPoint(mousePos);
        _world_position.z = 0;
        cell_pointed = Grid.WorldToCell(_world_position);

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            smells_prev[cell_pointed.x, cell_pointed.y] = AssigneToCellOnClick;
        }
        
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            if (prev_cell_pointed != cell_pointed)
            {
                Vector3Int dirVector3 = cell_pointed - prev_cell_pointed;
                Vector2Int dirVector2 = new Vector2Int(dirVector3.x, dirVector3.y);
                AssignVelocities(cell_pointed.x,cell_pointed.y,dirVector2);

            }
        }
        float densityToRemove = 0;
        prev_cell_pointed=cell_pointed;

   }
     IEnumerator UpdateSmellGrid() {
     while(true) {
        UpdateSmellDensities();
        yield return new WaitForSeconds(TickSeconds);
     }
 }
}