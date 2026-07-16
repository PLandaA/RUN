using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekBehavior : StateMachineBehaviour
{
    public Transform target;
    private float attackRange;
    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var controller = animator.GetComponent<ZombieAnimController>();
        GameObject player = controller.getPlayerInVision();
        if (player != null)
        {
            target = player.transform;
        }
        attackRange = controller.attackRange;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (target == null)
        {
            return;
        }

        animator.GetComponent<ZombieAnimController>().Seek(target.position);
        if ((target.position - animator.transform.position).magnitude <= attackRange)
        {
            animator.SetBool("isAttacking", true);
        }
    }
}
