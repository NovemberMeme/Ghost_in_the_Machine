using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demon_Chase_Behavior : StateMachineBehaviour
{
    Enemy enemy;
    float moveCooldown;
    float chaseTimer;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetInteger("CurrentMove", 0);
        enemy = animator.transform.parent.GetComponent<Enemy>();
        chaseTimer = 0.0f;
        moveCooldown = Random.Range(enemy.moveCooldownMin, enemy.moveCooldownMax);
        //Debug.Log("idle duration = " + moveCooldown);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        chaseTimer += Time.deltaTime;

        if (chaseTimer >= moveCooldown)
        {
            enemy.NextRandomMove();
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemy.movementSpeed = enemy.origMoveSpeed;
    }

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
