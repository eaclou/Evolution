using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodModule : MonoBehaviour {

    //public Material material;
    public MeshRenderer meshRendererBeauty;
    public MeshRenderer meshRendererFluidCollider;
    //public Texture2D texture;

    public int index;

    public float amountR;
    public float amountG;
    public float amountB;

    private int colliderCount = 0;

    private float feedingRate = 0.025f;

    public bool isDepleted = false;

    private float minScale = 0.5f;
    public float curScale = 1f;
    private float maxScale = 4.5f;

    private float minMass = 0.1f;
    private float maxMass = 25f;

    private float isBeingEaten = 0f;

    private Vector2 prevPos;
    public Vector3 _PrevPos
    {
        get
        {
            return prevPos;
        }
        set
        {

        }
    }

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
        prevPos = transform.localPosition;
    }

    private void FixedUpdate() {
        float avgAmount = (amountR + amountG + amountB) / 3.0f;
        float lerpAmount = Mathf.Sqrt(avgAmount);

        curScale = Mathf.Lerp(minScale, maxScale, lerpAmount);
        float mass = Mathf.Lerp(minMass, maxMass, lerpAmount);

        transform.localScale = new Vector3(curScale, curScale, curScale);
        GetComponent<Rigidbody2D>().mass = mass;

        isDepleted = CheckIfDepleted();

        /*if(colliderCount > 0) {
            isBeingEaten = 1.0f;
        }
        else {
            isBeingEaten = 0f;
        }*/

        meshRendererBeauty.material.SetFloat("_FoodAmountR", amountR);
        meshRendererBeauty.material.SetFloat("_FoodAmountG", amountG);
        meshRendererBeauty.material.SetFloat("_FoodAmountB", amountB);
        meshRendererBeauty.material.SetFloat("_Scale", curScale);
        meshRendererBeauty.material.SetFloat("_IsBeingEaten", isBeingEaten);

        Vector3 curPos = transform.localPosition;
        
        /*//if (rigidBody2D != null) {
        float velScale = 0.17f; ; // Time.fixedDeltaTime * 0.17f; // approx guess for now
        meshRendererFluidCollider.material.SetFloat("_VelX", (curPos.x - prevPos.x) * velScale);
        meshRendererFluidCollider.material.SetFloat("_VelY", (curPos.y - prevPos.y) * velScale);
        //}*/
        prevPos = curPos;
        
        isBeingEaten = 0.0f;
    }

    private void OnCollisionEnter2D(Collision2D coll) {
        
        colliderCount++;
        //Debug.Log("Food Collision! OnCollisionEnter colliderCount: " + colliderCount.ToString());
    }
    private void OnCollisionStay2D(Collision2D coll) {


        Agent collidingAgent = coll.collider.gameObject.GetComponentInParent<Agent>();
        if (collidingAgent != null) {

            isBeingEaten = 1.0f;

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
