﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_Behavior : StateMachineBehaviour
{
    [SerializeField] private float timer;

    [SerializeField] private List<int> eventTimeFrames = new List<int>();

    [SerializeField] private int currentEventTimeIndex;
    [SerializeField] private int functionToBeCalled;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0;
        currentEventTimeIndex = 0;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer += Time.deltaTime;

        if(timer >= stateInfo.length)
        {
            timer = 0;
            currentEventTimeIndex = 0;
        }
        else if(currentEventTimeIndex < eventTimeFrames.Count)
        {
            if(timer >= (float)eventTimeFrames[currentEventTimeIndex] / 60)
            {
                switch (functionToBeCalled)
                {
                    case 0:
                        SwordSwingSFX();
                        break;
                    case 1:
                        FootstepSFX();
                        break;
                    case 2:
                        SetCrushFalse(animator);
                        break;
                }

                currentEventTimeIndex++;
            }
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

    public void SwordSwingSFX()
    {
        SoundManager.PlaySound("SwordSwing");
    }

    public void FootstepSFX()
    {
        SoundManager.PlaySound("Landing");
    }

    public void SetCrushFalse(Animator animator)
    {
        animator.SetBool("Crush", false);
    }
}
