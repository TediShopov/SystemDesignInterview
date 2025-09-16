using FunkyCode.SuperTilemapEditorSupport.Light.Shadow;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Grid = UnityEngine.Grid;

public static class Helpers
{
    private static PointerEventData _eventDataCurrentPosition;
    private static List<RaycastResult> _results;

    private static bool IsOverUI()
    {
        _eventDataCurrentPosition = new PointerEventData(EventSystem.current) { position = Mouse.current.position.ReadValue() };
        _results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(_eventDataCurrentPosition, _results);
        return _results.Count > 0;
    }

    public static Vector2 GetWorldPositionOfCanvasElement(RectTransform element)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(element, element.position, Camera.current, out var result);
        return result;
    }

    public static void DeleteAllChildren(Transform t)
    {
        if (t != null)
        {
            foreach (Transform child in t)
            {
                Object.Destroy(child.gameObject);
            }
        }
    }

    public static bool LayerContainedInMask(int layer,LayerMask layerMask)
    {
        return (layerMask == (layerMask | (1 << layer)));
    }


    public static bool CompareFloats(float a, float b, float bias)
    {
        return Mathf.Abs(a - b) < bias;
    }


    //CompateVectors
    public static bool CompareVectors(Vector3 a, Vector3 b, float bias)
    {
        return CompareFloats(a.x, b.x, bias) && CompareFloats(a.y, b.y, bias) && CompareFloats(a.z, b.z, bias);
    }

    public static float ConvertToNewRange(float value, float oldMin, float oldMax, float newMin = 0, float newMax = 1)
    {
        var oldRange = oldMax - oldMin;
        var newRange = newMax - newMin;
        return (value - oldMin) * newRange / oldRange + newMin;
    }

    public static float ConvertToNewRangeClamped(float value, float oldMin, float oldMax, float newMin = 0, float newMax = 1)
    {
        value = ClampToRange(value, oldMin, oldMax);
        return ConvertToNewRange(value, oldMin, oldMax, newMin, newMax);
    }

    public static float ClampToRange(float value, float min = 0, float max = 1)
    {
        if (value <= min)
            return min;

        if (value >= max)
            return max;

        return value;
    }

    public static int ClampToRangeInt(int value, int min = 0, int max = 1)
    {
        if (value <= min)
            return min;

        if (value >= max)
            return max;

        return value;
    }

    public static Vector3 GetWorldTileSize(Grid grid, Transform t)
    {
        return new Vector3(grid.cellSize.x * t.lossyScale.x, grid.cellSize.y * t.lossyScale.y);
    }

    public static float GetImpactStrength(Collision2D collision)
    {
        ContactPoint2D[] contacts = new ContactPoint2D[collision.contactCount];
        collision.GetContacts(contacts);
        float totalImpulse = 0;
        foreach (ContactPoint2D contact in contacts)
        {
            totalImpulse += contact.normalImpulse;
        }

        return totalImpulse;



        //collision.
        //return Vector2.Dot(collision.GetContact(0).normal, collision.relativeVelocity) *
        //       collision.otherCollider.attachedRigidbody.mass;
    }

    public static Quaternion GetRotationLookAtPosition2D(Vector3 startPosition, Vector3 targetLocation, Vector3 upEuler)
    {
        targetLocation.z = startPosition.z; // ensure there is no 3D rotation by aligning Z position

        // vector from this object towards the target location
        Vector3 vectorToTarget = targetLocation - startPosition;
        // rotate that vector by 90 degrees around the Z axis
        Vector3 rotatedVectorToTarget = Quaternion.Euler(upEuler) * vectorToTarget;

        // get the rotation that points the Z axis forward, and the Y axis 90 degrees away from the target
        // (resulting in the X axis facing the target)
        Quaternion targetRotation = Quaternion.LookRotation(forward: Vector3.forward, upwards: rotatedVectorToTarget);
        return targetRotation;
    }


}
