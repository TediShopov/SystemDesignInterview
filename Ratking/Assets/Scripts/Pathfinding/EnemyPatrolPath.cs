using System;
using System.Collections.Generic;
using UnityEngine;

public enum PatrolPathType
{
    Backtracking,
    Looping
}

public class EnemyPatrolPath : MonoBehaviour
{
    [SerializeField] public List<Transform> PathPoints;
    [SerializeField] public PatrolPathType Type;

    private AIPathfinding Pawn;

    private bool _isGoingBackwards;
    private int _indexOfNextPoint;

    // Start is called before the first frame update
    public void RemoveFromPath(AIPathfinding pawn)
    {
        if (Pawn == pawn)
            //Remove 
            Pawn.OnDestinationReached -= SetupNextPatrolPointAsDestination;
    }

    public void StartPatrolPath(AIPathfinding pawn)
    {
        //If previous pawn was subsribed to path
        if (Pawn != null)
            //Remove 
            Pawn.OnDestinationReached -= SetupNextPatrolPointAsDestination;
        Pawn = pawn;
        Pawn.OnDestinationReached += SetupNextPatrolPointAsDestination;
        _indexOfNextPoint = 0;
        Pawn.SetNewPath(PathPoints[_indexOfNextPoint].position);
    }

    private int LoopingNextPointIndex(int currentPointIndex)
    {
        var nextIndex = currentPointIndex + 1;
        if (nextIndex > PathPoints.Count - 1) return 0;

        return nextIndex;
    }

    private int BacktrackingNextPointIndex(int currentPointIndex)
    {
        int nextIndex;
        if (_isGoingBackwards)
        {
            nextIndex = currentPointIndex - 1;
            if (nextIndex < 0)
            {
                _isGoingBackwards = false;
                return 0;
            }
        }
        else
        {
            nextIndex = currentPointIndex + 1;
            if (nextIndex > PathPoints.Count - 1)
            {
                _isGoingBackwards = true;
                return BacktrackingNextPointIndex(currentPointIndex);
            }
        }

        return nextIndex;
    }

    private void SetupNextPatrolPointAsDestination()
    {
        if (Type == PatrolPathType.Backtracking)
            _indexOfNextPoint = BacktrackingNextPointIndex(_indexOfNextPoint);
        else if (Type == PatrolPathType.Looping) _indexOfNextPoint = LoopingNextPointIndex(_indexOfNextPoint);
        Debug.LogWarning($"Go to next path patrol point {_indexOfNextPoint}");
        try
        {
            Pawn.SetNewPath(PathPoints[_indexOfNextPoint].position);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void OnDrawGizmosSelected()
    {

        foreach (var pathPoint in this.PathPoints)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(pathPoint.position, 0.15f);

        }
    }


}