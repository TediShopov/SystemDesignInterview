using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateShaderAnimationCustomTime : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}
    public string materialProperty = "_CustomTime";


    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Get normalized time (can exceed 1 if looping or not clamped)
        float normalizedTime = stateInfo.normalizedTime % 1f;

        // Get the Renderer component
        Renderer renderer = animator.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.SetFloat(materialProperty, normalizedTime);
        }
        else
        {
            // Optional: Log or handle fallback
            Debug.LogWarning("Renderer or Material not found on animated object.");
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
