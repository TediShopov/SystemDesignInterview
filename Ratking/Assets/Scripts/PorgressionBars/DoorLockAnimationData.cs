using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Lock Animation Data", menuName = "DoorLockAnimationData")]

public class DoorLockAnimationData : ScriptableObject
{
    [Header("On Failed")] 
    [SerializeField] public LeanTweenType OnFailTweenType;
    [SerializeField] public float OnFailAnimationDuration;
    [SerializeField] public float OnFailMoveDistanceX;

    [Header("On Succeeded")]
    [SerializeField] public LeanTweenType OnSucceedTweenType;
    [SerializeField] public float OnSucceedDuration;
    [SerializeField] public float OnSucceedRotateDegreesZ;

    [SerializeField] public Vector2 OnSucceedMoveTo;

     public void OnFailedAnimation(GameObject obj)
     {
         if (!LeanTween.isTweening(obj))
         {
             LeanTween.moveLocalX(obj, -OnFailMoveDistanceX, OnFailAnimationDuration).setEase(OnFailTweenType);
         }
     }

    public void OnSucceededAnimation(GameObject obj)
    {
        if (!LeanTween.isTweening(obj))
        {
            LeanTween.moveX(obj, obj.transform.position.x + OnSucceedMoveTo.x, OnSucceedDuration)
                .setEase(OnSucceedTweenType).setLoopClamp();
            LeanTween.moveY(obj, obj.transform.position.y + OnSucceedMoveTo.y, OnSucceedDuration)
                .setEase(OnSucceedTweenType).setLoopClamp();
            LeanTween.rotateAroundLocal(obj, Vector3.forward, OnSucceedRotateDegreesZ, OnSucceedDuration / 2.0f)
                .setLoopClamp();
            LeanTween.alpha(obj, 0, OnSucceedDuration).setEase(OnSucceedTweenType).setLoopClamp();
        }
    }


}
