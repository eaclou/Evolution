using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodModule : MonoBehaviour {

    public int index;

    public float amountR;
    public float amountG;
    public float amountB;

    private int colliderCount = 0;

    private float feedingRate = 0.02f;

    public bool isDepleted = false;

    // Use this for initialization
    void Start() {
        Respawn();
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void Respawn() {
        amountR = UnityEngine.Random.Range(0f, 1f);
        amountG = UnityEngine.Random.Range(0f, 1f);
        amountB = UnityEngine.Random.Range(0f, 1f);
        isDepleted = false;
    }

    private void FixedUpdate() {
        isDepleted = CheckIfDepleted();
    }

    private void OnCollisionEnter2D(Collision2D coll) {
        
        colliderCount++;
        //Debug.Log("Food Collision! OnCollisionEnter colliderCount: " + colliderCount.ToString());
    }
    private void OnCollisionStay2D(Collision2D coll) {
        

        Agent collidingAgent = coll.collider.gameObject.GetComponent<Agent>();
        if (collidingAgent != null) {

            float flow = feedingRate / colliderCount;
            if(colliderCount == 0) {
                Debug.LogError("DIVIDE BY ZERO!!!");
            }

            collidingAgent.testModule.foodAmountR[0] += Mathf.Max(0f, amountR - flow);  // make sure Agent doesn't receive food from empty dispenser
            amountR -= flow;
            if (amountR < 0f)
                amountR = 0f;

            collidingAgent.testModule.foodAmountG[0] += Mathf.Max(0f, amountG - flow);  // make sure Agent doesn't receive food from empty dispenser
            amountG -= flow;
            if (amountG < 0f)
                amountG = 0f;

            collidingAgent.testModule.foodAmountB[0] += Mathf.Max(0f, amountB - flow);  // make sure Agent doesn't receive food from empty dispenser
            amountB -= flow;
            if (amountB < 0f)
                amountB = 0f;

            //Debug.Log("OnCollisionSTAY colliderCount: " + colliderCount.ToString() + " collider: " + coll.collider.ToString() + ", amountR: " + amountR.ToString());
        }
    }
    private void OnCollisionExit2D(Collision2D coll) {
        colliderCount--;
    }

    private bool CheckIfDepleted() {
        bool depleted = true;
        if (amountR > 0f)
            depleted = false;
        if (amountG > 0f)
            depleted = false;
        if (amountB > 0f)
            depleted = false;
        // If any of the the 3 types
        return depleted;
    }
}
