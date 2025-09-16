using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Door : MonoBehaviour
{
    public SpriteRenderer DoorVinesSprite;
    public Collider2D DoorCollider;

    public void SetState(bool t)
    {
        if(DoorVinesSprite != null)
            DoorVinesSprite.enabled = t;
        if(DoorCollider != null)
            DoorCollider.enabled = t;

    }


}
