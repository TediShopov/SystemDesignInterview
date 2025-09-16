using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateChangeNotificaiton : MonoBehaviour
{
    public float DurationOfScale;
    public float DurationOfStay;
    public Vector3 Scale;
    public LeanTweenType EaseType;
    void Start()
    {
        LeanTween.scale(this.gameObject, Scale, DurationOfScale).setOnComplete(DestroyAfter).setEase(EaseType);
    }

    public void DestroyAfter()
    {
        Destroy(this.gameObject, DurationOfStay);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
