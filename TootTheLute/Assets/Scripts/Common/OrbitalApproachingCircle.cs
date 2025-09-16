using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script should be applied on the origin of the component
public class OrbitalApproachingCircle : MonoBehaviour
{
    public float OrbitalSpeed = 1.0f;
    public float InitalAngle = 0.0f;
    public GameObject Origin;
    private Vector3 rotationAxis = Vector3.forward;
    public GameObject ApproachingCircle;
    public List<GameObject> TargetCircles;
    public List<float> TargetAngles;
    public float ApproachingCircleAcceptableError = 5f;
    public static float GetRotationAngle(GameObject from, GameObject to)
    {
        return GetRotationAngle(from.transform.position, to.transform.position);
    }
    public static float GetRotationAngle(Vector2 fromPosition, Vector2 targetPosition)
    {
        // Calculate the direction vector from the object to the target
        Vector2 direction = targetPosition - fromPosition;
        // Compute the angle in radians and convert to degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return angle;
    }
    public static float ConvertToPositiveAngle(float angle )
    {
        if (angle > 0)
            return angle;
        else
            return angle + 360;

    }
    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject targetCircle in TargetCircles) 
        {
            float angle = GetRotationAngle(Origin.transform.position, targetCircle.transform.position);
            TargetAngles.Add(angle);
            Debug.Log($"Target Angles is {angle}");
        }
        
    }

    //Check if the approaching circle is in the target spot or close enough
    public bool IsAttackInputValid()
    {
        float currentAngle = ConvertToPositiveAngle(GetRotationAngle(Origin,ApproachingCircle));
        //Check against all target circles
        foreach (GameObject targetCircle in TargetCircles)
        {
            float targetAngle = ConvertToPositiveAngle(GetRotationAngle(Origin, TargetCircles[0]));
            float erroer = Mathf.Abs((currentAngle - targetAngle));

            Debug.Log($"Angle At: {currentAngle} Angle Desired{targetAngle}: Error:{erroer}");
            if (erroer< ApproachingCircleAcceptableError)
                return true;
        }
        return false;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float rotationStep = Time.fixedDeltaTime * OrbitalSpeed;
        Origin.transform.Rotate(rotationAxis, rotationStep,Space.Self);
    }
}
