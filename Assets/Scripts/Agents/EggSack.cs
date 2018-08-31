using UnityEngine;

public class EggSack : MonoBehaviour {
        
    public int index;
    public int speciesIndex; // temp - based on static species
    

    public EggLifeStage curLifeStage;
    public enum EggLifeStage {
        GrowingInsideParent,  // still attached to agent     
        BeingBorn,
        GrowingIndependent,  // eggsack has been laid by parent
        Mature,  // Developed enough for some eggs to start hatching
        Decaying,  // all eggs either hatched or failed
        Null
    }
    private int pregnantDurationTimeSteps = 360;
    public int _PregnantDurationTimeSteps
    {
        get
        {
            return pregnantDurationTimeSteps;
        }
        set
        {

        }
    }
    private int birthDurationTimeSteps = 30;
    private int growDurationTimeSteps = 360;
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
    private int matureDurationTimeSteps = 600;  // max oldAge Time before beginning to rot
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
    private int decayDurationTimeSteps = 360;  // how long it takes to rot/decay
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

    public float foodAmount;  // if eaten, how much nutrients is the entire EggSack worth?
    
    public float growthStatus = 0f;  // 0-1 born --> mature
    public float decayStatus = 0f;

    public bool isDepleted = false;

    public Vector2 fullSize;
    public Vector2 curSize;

    //private Vector2 minSize = new Vector2(0.1f, 0.1f);
    
    private float minMass = 0.33f;
    private float maxMass = 3.33f;

    public float isBeingEaten = 0f;
    //private float isBeingDamaged = 0f;
    public float healthStructural = 1f;

    //public int ageCounterMature = 0; // only counts when Food is fully grown
    public int lifeStageTransitionTimeStepCounter = 0; // keeps track of how long food has been in its current lifeStage

    private int numSkipFramesResize = 7;

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

    //public CapsuleCollider2D colliderBody;
    public int parentCritterIndex = -1;
    public SpringJoint2D springJoint;
    public CapsuleCollider mouseClickCollider;
    public CapsuleCollider2D mainCollider;
    public Rigidbody2D rigidbodyRef;

    public Agent parentAgentRef;
    public bool isAttachedToCritter = false; // while pregnant? is this redumdamt n

    private float springJointMaxStrength = 5f;

    // Use this for initialization
    void Start() {
        //Respawn();
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void FirstTimeInitialize() {        
        if(rigidbodyRef == null) {
            
            rigidbodyRef = this.gameObject.AddComponent<Rigidbody2D>();
            mainCollider = this.gameObject.AddComponent<CapsuleCollider2D>();
            rigidbodyRef.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            springJoint = this.gameObject.AddComponent<SpringJoint2D>();
            springJoint.enabled = false;
            springJoint.autoConfigureDistance = false;
            springJoint.distance = 0.1f;
            springJoint.dampingRatio = 1f;
            springJoint.frequency = 1f;
            
            GameObject mouseClickColliderGO = new GameObject("MouseClickCollider");
            mouseClickColliderGO.transform.parent = this.gameObject.transform;
            mouseClickColliderGO.transform.localPosition = new Vector3(0f, 0f, 1f);
            mouseClickCollider = mouseClickColliderGO.AddComponent<CapsuleCollider>();
            mouseClickCollider.isTrigger = true;
            mouseClickColliderGO.SetActive(false);
        }
    }

    public void InitializeEggSackFromGenomePregnant(EggSackGenome genome, Agent parentAgent) {
        
        index = genome.index;
        this.fullSize = genome.fullSize;

        BeginLifeStageGrowingPregnant(parentAgent);
    }
    public void InitializeEggSackFromGenomeImmaculate(EggSackGenome genome, StartPositionGenome startPos) {
        curLifeStage = EggLifeStage.GrowingIndependent;

        index = genome.index;
        this.fullSize = genome.fullSize;
        foodAmount = this.fullSize.x * this.fullSize.y;
        
        lifeStageTransitionTimeStepCounter = 0;
        
        growthStatus = 0f;
        decayStatus = 0f;

        this.transform.localPosition = startPos.startPosition;        
                        
        rigidbodyRef.velocity = Vector2.zero;
        rigidbodyRef.angularVelocity = 0f;
        rigidbodyRef.drag = 7.5f;
        rigidbodyRef.angularDrag = 5f;
       
        isDepleted = false;
        healthStructural = 1f;
        prevPos = transform.position;        
    }

    private void UpdateEggSackSize(float percentage) {

        mainCollider.size = fullSize * Mathf.Max(0.01f, percentage);

        float mass = mainCollider.size.x * mainCollider.size.y; // Mathf.Lerp(minMass, maxMass, percentage);  // *** <<< REVISIT!!! ****
        mass = Mathf.Max(mass, 0.05f);  // constrain minimum
        rigidbodyRef.mass = mass;

        curSize = mainCollider.size;
        
        //if(lifeStageTransitionTimeStepCounter % numSkipFramesResize == 0) {
            //float sidesRatio = fullSize.x / fullSize.y;
            //float sideY = Mathf.Sqrt(foodAmount / sidesRatio);
            //float sideX = sideY * sidesRatio;
            //curSize = new Vector2(sideX, sideY);

            //mainCollider.size = fullSize * percentage;
            //transform.localScale = new Vector3(curSize.x, curSize.y, 1f); // ** CHANGE THIS!!!
        
        //}
    }

    private void BeginLifeStageGrowingPregnant(Agent parentAgent) {
        curLifeStage = EggLifeStage.GrowingInsideParent;        
        lifeStageTransitionTimeStepCounter = 0;
        parentAgentRef = parentAgent;

        growthStatus = 0f;
        decayStatus = 0f;

        rigidbodyRef.MovePosition(parentAgent.bodyRigidbody.position);

        springJoint.connectedBody = parentAgent.bodyRigidbody;
        springJoint.autoConfigureConnectedAnchor = false;
        springJoint.anchor = Vector2.zero;
        springJoint.connectedAnchor = Vector2.zero;
        springJoint.dampingRatio = 1f;
        springJoint.distance = 0f;
        springJoint.enableCollision = false;
        springJoint.enabled = true;
        springJoint.frequency = 1f;

        mainCollider.enabled = true;  // ?? maybe??

        UpdateEggSackSize(0f);
    }
    private void CommenceBeingBorn() {
        curLifeStage = EggLifeStage.BeingBorn;
        Debug.Log("Begin Birth!");
        lifeStageTransitionTimeStepCounter = 0;
    }
    private void BeginLifeStageGrowingIndependent() {
        curLifeStage = EggLifeStage.GrowingIndependent;
        Debug.Log("EGGS BEING LAID (LAIN?)!");
        lifeStageTransitionTimeStepCounter = 0;

        springJoint.enabled = false;
        springJoint.enableCollision = true;
        springJoint.connectedBody = null;

        mainCollider.enabled = true;
        // size collider properly here:
    }
    private void BeginLifeStageMature() {
        curLifeStage = EggLifeStage.Mature;
                    
        lifeStageTransitionTimeStepCounter = 0;

        growthStatus = 1f;

        UpdateEggSackSize(1f);
    }
   
    private void CheckForLifeStageTransition() {
        switch(curLifeStage) {
            case EggLifeStage.GrowingInsideParent:
                // 
                // Preggers
                if(lifeStageTransitionTimeStepCounter >= pregnantDurationTimeSteps) {                   
                    // transition from being attached to parent Agent rigidbody, to free-floating:
                    CommenceBeingBorn();
                }
                break;
            case EggLifeStage.BeingBorn:
                //
                if(lifeStageTransitionTimeStepCounter >= birthDurationTimeSteps) {                                        
                    // Disconnect from parent:
                    BeginLifeStageGrowingIndependent();                    
                }
                break;
            case EggLifeStage.GrowingIndependent:
                //
                if(lifeStageTransitionTimeStepCounter >= growDurationTimeSteps) {
                    BeginLifeStageMature();
                }
                /*float maxFoodAvg = foodAmount; // Mathf.Max(Mathf.Max(amountR, amountG), amountB);
                if (maxFoodAvg <= 0f) {
                    curLifeStage = EggLifeStage.Decaying;
                    //isNull = true;
                }
                if (healthStructural <= 0f) {
                    curLifeStage = EggLifeStage.Decaying;
                }*/
                break;
            case EggLifeStage.Mature:
                //
                // Check for Death:
                //float minFood = Mathf.Min(Mathf.Min(testModule.foodAmountR[0], testModule.foodAmountG[0]), testModule.foodAmountB[0]);
                /*float maxFood = foodAmount; // Mathf.Max(Mathf.Max(amountR, amountG), amountB);
                if (maxFood <= 0f) {
                    curLifeStage = EggLifeStage.Decaying;
                    //isNull = true;
                }
                if (healthStructural <= 0f) {
                    curLifeStage = EggLifeStage.Decaying;
                }*/
                if(lifeStageTransitionTimeStepCounter >= matureDurationTimeSteps) {
                    curLifeStage = EggLifeStage.Decaying;                    
                }
                if(transform.position.x > SimulationManager._MapSize || transform.position.x < 0f || transform.position.y > SimulationManager._MapSize || transform.position.y < 0f) {
                    curLifeStage = EggLifeStage.Decaying;   
                }
                break;
            case EggLifeStage.Decaying:
                //
                if(lifeStageTransitionTimeStepCounter >= decayDurationTimeSteps) {
                    curLifeStage = EggLifeStage.Null;
                    //Debug.Log("FOOD NO LONGER EXISTS!");
                    lifeStageTransitionTimeStepCounter = 0;
                    isDepleted = true;  // flagged for respawn
                }
                break;
            case EggLifeStage.Null:
                //
                //Debug.Log("FoodLifeStage is null - probably shouldn't have gotten to this point...;");
                break;
            default:
                Debug.LogError("NO SUCH ENUM ENTRY IMPLEMENTED, YOU FOOL!!! (" + curLifeStage.ToString() + ")");
                break;
        }
    }
    public void Tick() {
        facingDirection = new Vector2(Mathf.Cos(Mathf.Deg2Rad * transform.localEulerAngles.z + Mathf.PI * 0.5f), Mathf.Sin(Mathf.Deg2Rad * transform.localEulerAngles.z + Mathf.PI * 0.5f));
        // Check for StateChange:
        CheckForLifeStageTransition();
        
        switch(curLifeStage) {
            case EggLifeStage.GrowingInsideParent:
                //
                TickGrowingInsideParent();
                break;
            case EggLifeStage.BeingBorn:
                //
                TickBeingBorn();
                break;
            case EggLifeStage.GrowingIndependent:
                //
                TickGrowingIndependent();
                break;
            case EggLifeStage.Mature:
                //
                TickMature();
                break;
            case EggLifeStage.Decaying:
                //
                TickDecaying();
                break;
            case EggLifeStage.Null:
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

    private void TickGrowingInsideParent() {    ///  ***** v v v *** REFACTOR!!!!!
        lifeStageTransitionTimeStepCounter++;

        float growthPercentage = (float)lifeStageTransitionTimeStepCounter / (float)(pregnantDurationTimeSteps + birthDurationTimeSteps + growDurationTimeSteps);
        growthStatus = growthPercentage;

        if(lifeStageTransitionTimeStepCounter % numSkipFramesResize == 0) {
            UpdateEggSackSize(growthStatus);
        }        
    }
    private void TickBeingBorn() {
        lifeStageTransitionTimeStepCounter++;

        float birthPercentage = (float)lifeStageTransitionTimeStepCounter / (float)birthDurationTimeSteps;
        float springStrength = Mathf.Lerp(springJointMaxStrength, 0.001f, birthPercentage);
        float springDistance = Mathf.Lerp(0f, parentAgentRef.colliderBody.size.magnitude, birthPercentage);

        springJoint.frequency = springStrength;
        springJoint.distance = springDistance;
    }
    private void TickGrowingIndependent() {
        lifeStageTransitionTimeStepCounter++;

        float growthPercentage = (float)(lifeStageTransitionTimeStepCounter + pregnantDurationTimeSteps + birthDurationTimeSteps) / (float)(pregnantDurationTimeSteps + birthDurationTimeSteps + growDurationTimeSteps);
        growthStatus = growthPercentage;

        if(lifeStageTransitionTimeStepCounter % numSkipFramesResize == 0) {
            UpdateEggSackSize(growthStatus);
        } 
    }
    private void TickMature() {
        growthStatus = 1f;
                
        isDepleted = CheckIfDepleted();

        //ageCounterMature++;        
    }
    private void TickDecaying() {
        float decayPercentage = (float)lifeStageTransitionTimeStepCounter / (float)decayDurationTimeSteps;
        decayStatus = decayPercentage;
        //curSize = Vector2.Lerp(fullSize * 0.4f, new Vector3(0.0f, 0.0f), Mathf.Clamp01(decayStatus * 6f));
        //transform.localScale = new Vector3(curSize.x, curSize.y, 1f);
        lifeStageTransitionTimeStepCounter++;
    }
    
    /*private void ComputeCollisionDamage(Collider2D coll) {
        
        //healthStructural -= 0.002f;
        Agent collidingAgent = coll.gameObject.GetComponentInParent<Agent>();
        //collidingAgent.GetComponent<Rigidbody2D>().drag = 100f;
        Vector2 agentHeading = collidingAgent.facingDirection;
        Vector2 agentToFoodDir = new Vector2(transform.position.x - collidingAgent.transform.position.x, transform.position.y - collidingAgent.transform.position.y).normalized;

        float dotProd = Vector2.Dot(agentHeading, agentToFoodDir);

        if(dotProd > 0f) {
            //isBeingDamaged = 1.0f;
            collidingAgent.isInsideFood = true;
        }        
    }*/
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
