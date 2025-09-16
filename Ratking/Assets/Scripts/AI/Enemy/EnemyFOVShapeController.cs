using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(SpriteShapeController))]
[RequireComponent(typeof(EnemySensor))]
public class EnemyFOVShapeController : MonoBehaviour
{
    private EnemySensor _enemySensor;
    private SpriteShapeController _shapeController;

    void Awake()
    {
        _enemySensor = GetComponent<EnemySensor>();
        
        _shapeController = GetComponent<SpriteShapeController>();
        DrawFieldOfView();

    }
    void DrawFieldOfView()
    {
        Vector3 viewAngle1 = DirectionFromAngle(-_enemySensor.ViewAngle / 2);
        Vector3 viewAngle2 = DirectionFromAngle(_enemySensor.ViewAngle / 2);

        Vector3 position1 = viewAngle1 * _enemySensor.ViewDiameter / 2;
        Vector3 position2 = viewAngle2 * _enemySensor.ViewDiameter / 2;
        _shapeController.spline.InsertPointAt(0,Vector3.zero);
        _shapeController.spline.InsertPointAt(1,position1);
        _shapeController.spline.InsertPointAt(2, position2);

    }



    private Vector2 DirectionFromAngle(float angleInDegrees)
    {
        _shapeController.spline.Clear();
        angleInDegrees -= transform.eulerAngles.z - 90;

        if (transform.localScale.x < 0)
            angleInDegrees += 180;
        return new Vector2(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
