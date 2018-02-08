using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodModule : MonoBehaviour {

    public Material material;
    //public Texture2D texture;

    public int index;

    public float amountR;
    public float amountG;
    public float amountB;

    private int colliderCount = 0;

    private float feedingRate = 0.01f;

    public bool isDepleted = false;

    private float minScale = 0.5f;
    private float maxScale = 4.5f;

    private float minMass = 0.1f;
    private float maxMass = 25f;

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
        float avgAmount = (amountR + amountG + amountB) / 3.0f;
        float lerpAmount = Mathf.Sqrt(avgAmount);

        float scale = Mathf.Lerp(minScale, maxScale, lerpAmount);
        float mass = Mathf.Lerp(minMass, maxMass, lerpAmount);

        transform.localScale = new Vector3(scale, scale, scale);
        GetComponent<Rigidbody2D>().mass = mass;

        isDepleted = CheckIfDepleted();

        material.SetFloat("_FoodAmountR", amountR);
        material.SetFloat("_FoodAmountG", amountG);
        material.SetFloat("_FoodAmountB", amountB);
        material.SetFloat("_Scale", scale);
    }

    private void OnCollisionEnter2D(Collision2D coll) {
        
        colliderCount++;
        //Debug.Log("Food Collision! OnCollisionEnter colliderCount: " + colliderCount.ToString());
    }
    private void OnCollisionStay2D(Collision2D coll) {
        

        Agent collidingAgent = coll.collider.gameObject.GetComponent<Agent>();
        if (collidingAgent != null) {

            float flow = feedingRate; // / colliderCount;
            if(colliderCount == 0) {
                Debug.LogError("DIVIDE BY ZERO!!!");
            }

            float flowR = Mathf.Min(amountR, flow);
            collidingAgent.testModule.foodAmountR[0] += flowR * 2f;  // make sure Agent doesn't receive food from empty dispenser
            amountR -= flowR;
            if (amountR < 0f)
                amountR = 0f;

            float flowG = Mathf.Min(amountG, flow);
            collidingAgent.testModule.foodAmountG[0] += flowG * 2f;  // make sure Agent doesn't receive food from empty dispenser
            amountG -= flowG;
            if (amountG < 0f)
                amountG = 0f;

            float flowB = Mathf.Min(amountB, flow);
            collidingAgent.testModule.foodAmountB[0] += flowB * 2f;  // make sure Agent doesn't receive food from empty dispenser
            amountB -= flowB;
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
