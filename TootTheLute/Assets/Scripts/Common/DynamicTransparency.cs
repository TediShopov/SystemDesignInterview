using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicTransparency : MonoBehaviour
{
    public bool IsOnTopOfPlayerSprite = true;
    public bool FromDistanceOnly = false;
    public float WantedTransparency = 0;
    public Vector3 CustomSortingAxis;
    public Vector2 TransparencyRange;
    public Vector2 DistanceRange;
    public SpriteRenderer PlayerSprite;
    public SpriteRenderer SpriteRenderer;

    public float GetTransparency()
    {
        if(FromDistanceOnly)
        {
            float d =Vector3.Distance(this.PlayerSprite.transform.position, this.SpriteRenderer.transform.position);
            return Remap(d, DistanceRange.x, DistanceRange.y,TransparencyRange.x, TransparencyRange.y);
        }

        if(IsOnTopOfPlayerSprite) 
        {
            float d =Vector3.Distance(this.PlayerSprite.transform.position, this.SpriteRenderer.transform.position);
            return Remap(d, DistanceRange.x, DistanceRange.y,TransparencyRange.x, TransparencyRange.y);
        }
        {
            return 1;
        }

    }
    float Remap(float value, float fromMin, float fromMax, float toMin=0, float toMax = 1)
{
    return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
}
    public bool IsOnTopOfSprite(SpriteRenderer spriteA, SpriteRenderer spriteB)
    {
        if (spriteA.sortingLayerID == spriteB.sortingLayerID)
        {
            if (spriteA.sortingOrder >= spriteB.sortingOrder)
            {
                float aDepth = GetSortDepth(spriteA.transform, CustomSortingAxis);
                float bDepth = GetSortDepth(spriteB.transform, CustomSortingAxis);

                if (aDepth > bDepth)
                {
                    return false ;
                }

                else
                {
                    //return false;
                    return true;

                }

            }
            else
            {
                Debug.Log("B is in front of A");
                return false;

            }
        }
        return false;
    }

    float GetSortDepth(Transform t, Vector3 sortAxis)
    {
        return Vector3.Dot(t.position, sortAxis);
    }

    // Start is called before the first frame update
    void Start()
    {
        var player = FindObjectOfType<PlayerController2D>().gameObject;
        PlayerSprite = player.GetComponentInChildren<SpriteRenderer>();
        this.SpriteRenderer = this.GetComponent<SpriteRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        //IsOnTopOfPlayerSprite = IsOnTopOfSprite(this.SpriteRenderer, this.PlayerSprite);
        IsOnTopOfPlayerSprite = IsOnTopOfSprite(this.SpriteRenderer, this.PlayerSprite);
        var c =this.SpriteRenderer.color;
        WantedTransparency = GetTransparency();
        this.SpriteRenderer.color = new Color(c.r,c.g,c.b,GetTransparency());

    }
}
