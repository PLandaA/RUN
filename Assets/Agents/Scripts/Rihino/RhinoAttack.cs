using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhinoAttack : StateMachineBehaviour
{
    private Transform target;
    private float attackRange;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var controller = animator.GetComponent<RhinoAnimController>();
        GameObject enemy = controller.getEnemysInVision();
        if (enemy != null)
        {
            target = enemy.transform;
        }
        attackRange = controller.attackRange;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (target == null)
        {
            animator.SetBool("isAttacking", false);
            return;
        }

        if ((target.position - animator.transform.position).magnitude > attackRange)
        {
            animator.SetBool("isAttacking", false);
        }
    }
}
