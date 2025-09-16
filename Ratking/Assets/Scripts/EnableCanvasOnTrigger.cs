using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableCanvasOnTrigger : MonoBehaviour
{
    public Canvas Canvas;

    public LayerMask LayerMask;
    // Start is called before the first frame update

    void Start()
    {
        if (this.Canvas.gameObject.activeSelf==true)
        {
            this.Canvas.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D collider2D)
    {
     
        if (Helpers.LayerContainedInMask(collider2D.gameObject.layer,LayerMask))
        {
            Canvas.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D collider2D)
    {

        if (Helpers.LayerContainedInMask(collider2D.gameObject.layer, LayerMask))
        {
            Canvas.gameObject.SetActive(false);
        }
    }
}
