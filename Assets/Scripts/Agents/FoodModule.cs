using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodModule : MonoBehaviour {

    //public Material material;
    //public MeshRenderer meshRendererBeauty;
    //public MeshRenderer meshRendererFluidCollider;
    //public Texture2D texture;

    public int index;

    public CapsuleCollider2D collisionCollider;
    public CapsuleCollider2D collisionTrigger;

    public FoodLifeStage curLifeStage;
    public enum FoodLifeStage {
        Growing,
        Mature,
        Decaying,
        Null
    }
    private int growDurationTimeSteps = 240;
    public int _GrowDurationTimeSteps
    {
        get
        {
            return growDurationTimeSteps;
        }
        set
        {

        }
    }
    private int matureDurationTimeSteps = 10000;  // max oldAge
    public int _MatureDurationTimeSteps
    {
        get
        {
            return matureDurationTimeSteps;
        }
        set
        {

        }
    }
    private int decayDurationTimeSteps = 240;
    public int _DecayDurationTimeSteps
    {
        get
        {
            return decayDurationTimeSteps;
        }
        set
        {

        }
    }

    public float amountR;
    public float amountG;
    public float amountB;

    private int colliderCount = 0;

    private float feedingRate = 0.005f;

    public bool isDepleted = false;

    public Vector2 fullSize;
    public Vector2 curSize;

    private Vector2 minSize = new Vector2(0.25f, 0.25f);
    //public float curScale = 1f;
    //private float maxScale = 4.5f;

    private float minMass = 0.25f;
    private float maxMass = 25f;

    private float isBeingEaten = 0f;
    private float isBeingDamaged = 0f;
    public float healthStructural = 1f;

    public int ageCounterMature = 0; // only counts when Food is fully grown
    public int lifeStageTransitionTimeStepCounter = 0; // keeps track of how long food has been in its current lifeStage

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

    public Vector2 facingDirection;

    // Use this for initialization
    void Start() {
        //Respawn();
    }

    // Update is called once per frame
    void Update () {
		
	}

    /*public void Respawn() {
        amountR = UnityEngine.Random.Range(0f, 1f);
        amountG = UnityEngine.Random.Range(0f, 1f);
        amountB = UnityEngine.Random.Range(0f, 1f);
        isDepleted = false;
        prevPos = transform.localPosition;
    }*/

    public void InitializeFoodFromGenome(FoodGenome genome, StartPositionGenome startPos) {
        curLifeStage = FoodLifeStage.Growing;

        index = genome.index;
        this.fullSize = genome.fullSize;
        amountR = UnityEngine.Random.Range(0f, 1f); // ** revisit eventually
        amountG = UnityEngine.Random.Range(0f, 1f);
        amountB = UnityEngine.Random.Range(0f, 1f);

        lifeStageTransitionTimeStepCounter = 0;
        ageCounterMature = 0;

        this.transform.localPosition = startPos.startPosition;
        this.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

        isDepleted = false;
        prevPos = transform.localPosition;

        //curLifeStage = AgentLifeStage.Egg;
        //this.fullSize = genome.fullSize;
        //isNull = false;
        //lifeStageTransitionTimeStepCounter = 0;
        //ageCounterMature = 0;
        //this.transform.localPosition = startPos.agentStartPosition;
        //this.transform.localScale = new Vector3(size.x, size.y, 1f);
        //InitializeModules(genome, this, startPos);      // Modules need to be created first so that Brain can map its neurons to existing modules  
        //brain = new Brain(genome.brainGenome, this);
        //facingDirection = new Vector2(0f, 1f);
        //throttle = Vector2.zero;
        //smoothedThrottle = Vector2.zero;
        //prevPos = transform.localPosition;
    }

    private void CheckForLifeStageTransition() {
        switch(curLifeStage) {
            case FoodLifeStage.Growing:
                //
                if(lifeStageTransitionTimeStepCounter >= growDurationTimeSteps) {
                    curLifeStage = FoodLifeStage.Mature;
                    //Debug.Log("EGG HATCHED!");
                    lifeStageTransitionTimeStepCounter = 0;
                }
                float maxFoodAvg = Mathf.Max(Mathf.Max(amountR, amountG), amountB);
                if (maxFoodAvg <= 0f) {
                    curLifeStage = FoodLifeStage.Decaying;
                    //isNull = true;
                }
                if (healthStructural <= 0f) {
                    curLifeStage = FoodLifeStage.Decaying;
                }
                break;
            case FoodLifeStage.Mature:
                //
                // Check for Death:
                //float minFood = Mathf.Min(Mathf.Min(testModule.foodAmountR[0], testModule.foodAmountG[0]), testModule.foodAmountB[0]);
                float maxFood = Mathf.Max(Mathf.Max(amountR, amountG), amountB);
                if (maxFood <= 0f) {
                    curLifeStage = FoodLifeStage.Decaying;
                    //isNull = true;
                }
                if (healthStructural <= 0f) {
                    curLifeStage = FoodLifeStage.Decaying;
                }
                break;
            case FoodLifeStage.Decaying:
                //
                if(lifeStageTransitionTimeStepCounter >= decayDurationTimeSteps) {
                    curLifeStage = FoodLifeStage.Null;
                    //Debug.Log("FOOD NO LONGER EXISTS!");
                    lifeStageTransitionTimeStepCounter = 0;
                    isDepleted = true;  // flagged for respawn
                }
                break;
            case FoodLifeStage.Null:
                //
                Debug.Log("FoodLifeStage is null - probably shouldn't have gotten to this point...;");
                break;
            default:
                Debug.LogError("NO SUCH ENUM ENTRY IMPLEMENTED, YOU FOOL!!! (" + curLifeStage.ToString() + ")");
                break;
        }

        //if (testModule.hitPoints[0] <= 0f) {

            //curLifeStage = AgentLifeStage.Decaying;
            //lifeStageTransitionTimeStepCounter = 0;
            //Debug.Log("Agent DEAD!");
        //}
    }
    public void Tick() {
        facingDirection = new Vector2(Mathf.Cos(transform.localRotation.z), Mathf.Sin(transform.localRotation.z));
        // Check for StateChange:
        CheckForLifeStageTransition();
        
        switch(curLifeStage) {
            case FoodLifeStage.Growing:
                //
                TickGrowing();
                break;
            case FoodLifeStage.Mature:
                //
                TickMature();
                break;
            case FoodLifeStage.Decaying:
                //
                TickDecaying();
                break;
            case FoodLifeStage.Null:
                //
                //Debug.Log("agent is null - probably shouldn't have gotten to this point...;");
                break;
            default:
                Debug.LogError("NO SUCH ENUM ENTRY IMPLEMENTED, YOU FOOL!!! (" + curLifeStage.ToString() + ")");
                break;
        }
        
        Vector3 curPos = transform.localPosition;        
        prevPos = curPos;

        isBeingEaten = 0.0f;
    }

    private void TickGrowing() {
        lifeStageTransitionTimeStepCounter++;

        curSize = Vector2.Lerp(new Vector3(0.1f, 0.1f), fullSize, (float)lifeStageTransitionTimeStepCounter / (float)growDurationTimeSteps);
        transform.localScale = new Vector3(curSize.x, curSize.y, 1f);
                

        //Debug.Log("TickGrowing growDurationTimeSteps++");
    }
    private void TickMature() {
        
        float avgAmount = (amountR + amountG + amountB) / 3.0f;
        float lerpAmount = Mathf.Sqrt(avgAmount);

        curSize = Vector2.Lerp(new Vector3(0.1f, 0.1f), fullSize, 1f); // lerpAmount);  // *** <<< REVISIT!!! ****
        //float mass = Mathf.Lerp(minMass, maxMass, lerpAmount);  // *** <<< REVISIT!!! ****
        transform.localScale = new Vector3(curSize.x, curSize.y, 1f);
        //GetComponent<Rigidbody2D>().mass = mass;

        isDepleted = CheckIfDepleted();

        ageCounterMature++;
        
        //Vector3 curPos = transform.localPosition;        
        //prevPos = curPos;
        
        

    }
    private void TickDecaying() {

        lifeStageTransitionTimeStepCounter++;
    }
    
    private void ComputeCollisionDamage(Collider2D coll) {
        isBeingDamaged = 1.0f;
        healthStructural -= 0.002f;
    }

    private void OnTriggerEnter2D(Collider2D coll) {
        Agent collidingAgent = coll.gameObject.GetComponentInParent<Agent>();
        if (collidingAgent != null) {
            ComputeCollisionDamage(coll);
        }

        colliderCount++;
        //Debug.Log("Food Collision! OnCollisionEnter colliderCount: " + colliderCount.ToString());
    }
    private void OnTriggerStay2D(Collider2D coll) {
        
        Agent collidingAgent = coll.gameObject.GetComponentInParent<Agent>();
        if (collidingAgent != null) {

            ComputeCollisionDamage(coll);

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
    private void OnTriggerExit2D(Collider2D coll) {
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

    private void FixedUpdate() {
        /*float avgAmount = (amountR + amountG + amountB) / 3.0f;
        float lerpAmount = Mathf.Sqrt(avgAmount);

        curScale = Mathf.Lerp(minScale, maxScale, lerpAmount);
        float mass = Mathf.Lerp(minMass, maxMass, lerpAmount);

        transform.localScale = new Vector3(curScale, curScale, curScale);
        GetComponent<Rigidbody2D>().mass = mass;

        isDepleted = CheckIfDepleted();
        */
        /*if(colliderCount > 0) {
            isBeingEaten = 1.0f;
        }
        else {
            isBeingEaten = 0f;
        }*/
        /*
        meshRendererBeauty.material.SetFloat("_FoodAmountR", amountR);
        meshRendererBeauty.material.SetFloat("_FoodAmountG", amountG);
        meshRendererBeauty.material.SetFloat("_FoodAmountB", amountB);
        meshRendererBeauty.material.SetFloat("_Scale", curScale);
        meshRendererBeauty.material.SetFloat("_IsBeingEaten", isBeingEaten);

        Vector3 curPos = transform.localPosition;
        */
        /*//if (rigidBody2D != null) {
        float velScale = 0.17f; ; // Time.fixedDeltaTime * 0.17f; // approx guess for now
        meshRendererFluidCollider.material.SetFloat("_VelX", (curPos.x - prevPos.x) * velScale);
        meshRendererFluidCollider.material.SetFloat("_VelY", (curPos.y - prevPos.y) * velScale);
        //}*/


        //prevPos = curPos;
        
        //isBeingEaten = 0.0f;
    }
}
