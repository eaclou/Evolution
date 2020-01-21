using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MecanimSpiritClicked : StateMachineBehaviour {

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("_PlayingUnlockAnim", true);
        //animator.SetBool("_AnimFinished", false);
        //clone = Instantiate(particle, animator.rootPosition, Quaternion.identity) as GameObject;
        //Rigidbody rb = clone.GetComponent<Rigidbody>();
        //rb.AddExplosionForce(power, animator.rootPosition, radius, 3.0f);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("MecanimSpiritClicked: " + animator.ToString() + ", si: " + stateInfo.ToString());
        animator.SetBool("_PlayingUnlockAnim", false);
        animator.SetBool("_AnimFinished", true);
        animator.ResetTrigger("_TriggerClicked");
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
