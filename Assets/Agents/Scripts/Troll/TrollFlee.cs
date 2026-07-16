using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrollFlee : StateMachineBehaviour
{
    private Transform target;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject player = animator.GetComponent<TrollAnimController>().getPlayerInVision();
        if (player != null)
        {
            target = player.transform;
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (target == null)
        {
            return;
        }

        animator.GetComponent<TrollAnimController>().Flee(target.position);
    }
}
