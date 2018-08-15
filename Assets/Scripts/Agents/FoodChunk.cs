using UnityEngine;

public class FoodChunk : MonoBehaviour {
        
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
    private int growDurationTimeSteps = 120;
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
    private int matureDurationTimeSteps = 3600;  // max oldAge Time to rot
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
    private int decayDurationTimeSteps = 60;
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

    public float foodAmount;
    //public float amountG;
    //public float amountB;

    private int colliderCount = 0;

    //private float feedingRate = 0.05f;

    public float growthStatus = 0f;  // 0-1 born --> mature
    public float decayStatus = 0f;

    public bool isDepleted = false;

    public Vector2 fullSize;
    public Vector2 curSize;

    private Vector2 minSize = new Vector2(0.1f, 0.1f);
    
    private float minMass = 0.25f;
    private float maxMass = 5f;

    public float isBeingEaten = 0f;
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
    
    public void InitializeFoodFromGenome(FoodGenome genome, StartPositionGenome startPos, FoodChunk parentFood) {
        curLifeStage = FoodLifeStage.Growing;

        index = genome.index;
        this.fullSize = genome.fullSize;
        foodAmount = this.fullSize.x * this.fullSize.y;
        //amountG = 0f;
        //amountB = 0f; 

        lifeStageTransitionTimeStepCounter = 0;
        ageCounterMature = 0;
        growthStatus = 0f;
        decayStatus = 0f;

        this.transform.localPosition = startPos.startPosition; // + new Vector3(0f, 0.25f * 0.5f, 0f);
        this.transform.localScale = new Vector3(0.1f, 0.1f, 1f);

        Rigidbody2D rigidBody = this.GetComponent<Rigidbody2D>();
        //HingeJoint2D joint = this.GetComponent<HingeJoint2D>();
                
        rigidBody.velocity = Vector2.zero;
        rigidBody.angularVelocity = 0f;
        //rigidBody.

        isDepleted = false;
        healthStructural = 1f;
        prevPos = transform.localPosition;        
    }

    private void CheckForLifeStageTransition() {
        switch(curLifeStage) {
            case FoodLifeStage.Growing:
                //
                if(lifeStageTransitionTimeStepCounter >= growDurationTimeSteps) {
                    curLifeStage = FoodLifeStage.Mature;
                    //Debug.Log("EGG HATCHED!");
                    lifeStageTransitionTimeStepCounter = 0;
                    //this.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    //this.GetComponent<Rigidbody2D>().velocity *= 0.36f;
                    //foodArray[i].GetComponent<Rigidbody2D>().AddForce(new Vector2(1f, 1f), ForceMode2D.Force); //
                    // Looks like AddForce has less of an effect on a GO/Rigidbody2D that is being scaled through a script... ??
                    // Feels like rigidbody is accumulating velocity which is then released all at once when the scaling stops??
                    // Hacking through it by increasign force on growing food:
                }
                float maxFoodAvg = foodAmount; // Mathf.Max(Mathf.Max(amountR, amountG), amountB);
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
                float maxFood = foodAmount; // Mathf.Max(Mathf.Max(amountR, amountG), amountB);
                if (maxFood <= 0f) {
                    curLifeStage = FoodLifeStage.Decaying;
                    //isNull = true;
                }
                if (healthStructural <= 0f) {
                    curLifeStage = FoodLifeStage.Decaying;
                }
                if(ageCounterMature >= matureDurationTimeSteps) {
                    curLifeStage = FoodLifeStage.Decaying;                    
                }
                if(transform.position.x > SimulationManager._MapSize || transform.position.x < 0f || transform.position.y > SimulationManager._MapSize || transform.position.y < 0f) {
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
                    //HingeJoint2D joint = this.GetComponent<HingeJoint2D>();
                    //joint.enabled = false;
                    //joint.connectedBody = null;
                }
                break;
            case FoodLifeStage.Null:
                //
                //Debug.Log("FoodLifeStage is null - probably shouldn't have gotten to this point...;");
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
        facingDirection = new Vector2(Mathf.Cos(Mathf.Deg2Rad * transform.localEulerAngles.z + Mathf.PI * 0.5f), Mathf.Sin(Mathf.Deg2Rad * transform.localEulerAngles.z + Mathf.PI * 0.5f));
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

        float growthPercentage = (float)lifeStageTransitionTimeStepCounter / (float)growDurationTimeSteps;
        growthStatus = growthPercentage;

        float mass = Mathf.Lerp(minMass, maxMass, growthPercentage);  // *** <<< REVISIT!!! ****
        GetComponent<Rigidbody2D>().mass = mass;

        curSize = Vector2.Lerp(new Vector3(0.1f, 0.1f), fullSize, growthStatus);
        transform.localScale = new Vector3(curSize.x, curSize.y, 1f);
                

        //Debug.Log("TickGrowing growDurationTimeSteps++");
    }
    private void TickMature() {
        growthStatus = 1f;

        //float avgAmount = amountR; // (amountR + amountG + amountB) / 3.0f;
        //float lerpAmount = Mathf.Sqrt(avgAmount);

        //curSize = Vector2.Lerp(new Vector3(0.1f, 0.1f), fullSize, Mathf.Clamp01(avgAmount + 0.4f)); // lerpAmount);  // *** <<< REVISIT!!! ****
        //float mass = Mathf.Lerp(minMass, maxMass, lerpAmount);  // *** <<< REVISIT!!! ****
        //GetComponent<Rigidbody2D>().mass = mass;

        float sidesRatio = fullSize.x / fullSize.y;
        float sideY = Mathf.Sqrt(foodAmount / sidesRatio);
        float sideX = sideY * sidesRatio;
        curSize = new Vector3(sideX, sideY);
        transform.localScale = new Vector3(curSize.x, curSize.y, 1f);
        
        isDepleted = CheckIfDepleted();

        ageCounterMature++;        
    }
    private void TickDecaying() {
        float decayPercentage = (float)lifeStageTransitionTimeStepCounter / (float)decayDurationTimeSteps;
        decayStatus = decayPercentage;
        //curSize = Vector2.Lerp(fullSize * 0.4f, new Vector3(0.0f, 0.0f), Mathf.Clamp01(decayStatus * 6f));
        //transform.localScale = new Vector3(curSize.x, curSize.y, 1f);
        lifeStageTransitionTimeStepCounter++;
    }
    
    private void ComputeCollisionDamage(Collider2D coll) {
        
        //healthStructural -= 0.002f;
        Agent collidingAgent = coll.gameObject.GetComponentInParent<Agent>();
        //collidingAgent.GetComponent<Rigidbody2D>().drag = 100f;
        Vector2 agentHeading = collidingAgent.facingDirection;
        Vector2 agentToFoodDir = new Vector2(transform.position.x - collidingAgent.transform.position.x, transform.position.y - collidingAgent.transform.position.y).normalized;

        float dotProd = Vector2.Dot(agentHeading, agentToFoodDir);

        if(dotProd > 0f) {
            isBeingDamaged = 1.0f;
            collidingAgent.isInsideFood = true;
        }        
    }
    /*
    private void OnCollisionEnter2D(Collision2D coll) {
        Agent collidingAgent = coll.gameObject.GetComponentInParent<Agent>();
        if (collidingAgent != null) {
            ComputeCollisionDamage(coll.collider);
            //collidingAgent.GetComponent<Rigidbody2D>().velocity *= 0.1f;            
        }
        colliderCount++;
        //Debug.Log("Food Collision! OnCollisionEnter colliderCount: " + colliderCount.ToString());
    }
    private void OnCollisionStay2D(Collision2D coll) {
        
        Agent collidingAgent = coll.gameObject.GetComponentInParent<Agent>();
        if (collidingAgent != null) {

            ComputeCollisionDamage(coll.collider);

            isBeingEaten = 1.0f;

            float flow = feedingRate; // / colliderCount;

            if(colliderCount == 0) {
                Debug.LogError("DIVIDE BY ZERO!!!");
            }

            float flowR = Mathf.Min(amountR, flow);
            //collidingAgent.testModule.foodAmountR[0] += flowR * 2f;  // make sure Agent doesn't receive food from empty dispenser

            collidingAgent.EatFood(flowR * 1f); // assumes all foodAmounts are equal !! *****
    
            amountR -= flowR;
            if (amountR < 0f) {
                amountR = 0f;
            }                
            //if(collidingAgent.testModule.foodAmountR[0] > 1f) {
            //    collidingAgent.testModule.foodAmountR[0] = 1f;
            //}
            float flowG = Mathf.Min(amountG, flow);
            //collidingAgent.testModule.foodAmountG[0] += flowG * 2f;  // make sure Agent doesn't receive food from empty dispenser
            amountG -= flowG;
            if (amountG < 0f) {
                amountG = 0f;
            }                
            //if(collidingAgent.testModule.foodAmountG[0] > 1f) {
            //    collidingAgent.testModule.foodAmountG[0] = 1f;
            //}
            float flowB = Mathf.Min(amountB, flow);
            //collidingAgent.testModule.foodAmountB[0] += flowB * 2f;  // make sure Agent doesn't receive food from empty dispenser
            amountB -= flowB;
            if (amountB < 0f) {
                amountB = 0f;
            }                
            //if(collidingAgent.testModule.foodAmountB[0] > 1f) {
            //    collidingAgent.testModule.foodAmountB[0] = 1f;
            //}
            //Debug.Log("OnCollisionSTAY colliderCount: " + colliderCount.ToString() + " collider: " + coll.collider.ToString() + ", amountR: " + amountR.ToString());
        }
    }
    private void OnCollisionExit2D(Collision2D coll) {
        colliderCount--;
    }
    */


    private bool CheckIfDepleted() {
        bool depleted = true;
        if (foodAmount > 0f)
            depleted = false;
        //if (amountG > 0f)
        //    depleted = false;
        //if (amountB > 0f)
        //    depleted = false;
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
