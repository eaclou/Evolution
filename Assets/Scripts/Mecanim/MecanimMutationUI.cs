using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MecanimMutationUI : StateMachineBehaviour {
        
	//public GameObject particle;
    //public float radius;
    //public float power;

    //protected GameObject clone;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("_TriggerMutate");
        //animator.SetBool("_AnimFinished", false);
        //clone = Instantiate(particle, animator.rootPosition, Quaternion.identity) as GameObject;
        //Rigidbody rb = clone.GetComponent<Rigidbody>();
        //rb.AddExplosionForce(power, animator.rootPosition, radius, 3.0f);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log("OnStateExit Animator: " + animator.ToString() + ", si: " + stateInfo.ToString());

        //animator.SetBool("_AnimFinished", true);
    }

    /*override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("On Attack Update ");
    }

    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("On Attack Move ");
    }

    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("On Attack IK ");
    }*/
}
