using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastFireball : StateMachineBehaviour
{
    public float offsetProjectile = 0.5f;
    public GameObject projectilePrefab;

    FighterController Fighter;

    static int FireballsCreated = 0;

    bool _doNotCast;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Fighter == null)
        {
            Fighter = animator.gameObject.GetComponent<FighterController>();
        }
        if (animator.GetBool("CastingFireball")==true)
        {

            _doNotCast = true;
            base.OnStateEnter(animator, stateInfo, layerIndex);
        }
        Fighter.SetCastingFireball(true);
       
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_doNotCast)
        {
            return;
        }
        if (Fighter == null)
        {
            Fighter = animator.gameObject.GetComponent<FighterController>();
        }

        Vector3 dirToEnemy = Fighter.GetDirToEnemy();
        //Make if flat
        dirToEnemy.y = 0;
        dirToEnemy.Normalize();
        dirToEnemy *= offsetProjectile;
        Vector3 pos = animator.transform.position + dirToEnemy;
        var g= Instantiate(projectilePrefab, pos, animator.transform.rotation);
        g.gameObject.layer= Fighter.gameObject.layer;
        FireballsCreated++;
        Debug.LogError($"Created FB {FireballsCreated}");
       
        var proj = g.GetComponent<Projectile>();
        proj.AddToManager(animator.gameObject.transform.parent.gameObject);
        proj.SetVelocity(dirToEnemy);
        Fighter.SetCastingFireball(false);
    }
}
