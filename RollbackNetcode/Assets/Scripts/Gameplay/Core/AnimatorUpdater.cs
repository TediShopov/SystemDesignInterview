using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorUpdater : MonoBehaviour
{
    Animator animator;
    private void Awake()
    {
        animator = this.gameObject.GetComponent<Animator>();
        //This script requires an animator. If no found throw an error.
        if (animator == null)
        {
            Debug.LogError($"SimulateAnimator script requires an animator.");
            return;
        }

        // Make the animator disabled so it is
        // now automatically updated in the game loop
        animator.enabled = false;
    }
    //Have a function that can progress the animator outside the standard game loop
    public void ManualUpdateFrame() 
    {
        animator.Update(Time.fixedDeltaTime);
    } 
    private void FixedUpdate()
    {
        if (ClientData.GameState == GameState.Runing)
        {
            ManualUpdateFrame();
        }
    }
}
