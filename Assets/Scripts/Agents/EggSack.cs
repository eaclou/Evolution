﻿using UnityEngine;

public class EggSack : MonoBehaviour {
        
    public int index;
    public int speciesIndex; // temp - based on static species    

    public EggLifeStage curLifeStage;
    public enum EggLifeStage {
        Growing,
        Mature,  // Developed enough for some eggs to start hatching
        Decaying,  // all eggs either hatched or failed
        Null
    }
    /*private int pregnantDurationTimeSteps = 360;
    public int _PregnantDurationTimeSteps
    {
        get
        {
            return pregnantDurationTimeSteps;
        }
        set
        {

        }
    }*/
    private float pregnancyPercentageOfTotalGrowTime = 0.5f;
    private int birthDurationTimeSteps = 30;
    private int growDurationTimeSteps = 540;
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
    private int matureDurationTimeSteps = 210;  // buffer time for late bloomers to spawn
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
    private int decayDurationTimeSteps = 330;  // how long it takes to rot/decay
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

    private float minMass = 0.33f;
    private float maxMass = 3.33f;

    public float isBeingEaten = 0f;
    public float healthStructural = 1f;
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
        
    public FixedJoint2D fixedJoint;
    public CapsuleCollider mouseClickCollider;
    public CapsuleCollider2D mainCollider;
    public Rigidbody2D rigidbodyRef;
    public Agent parentAgentRef;
    public int parentAgentIndex;

    public Agent predatorAgentRef;
    public SpringJoint2D springJoint;

    private float springJointMaxStrength = 15f;

    public bool isProtectedByParent = false;
    public bool isAttachedBySpring = false;  
    public bool isBeingBorn = false;
    public int birthTimeStepsCounter = 0;

    //public float debugFluidVel = 0f;
    //public float debugDepth = 0f;

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
            mainCollider.enabled = false;


            fixedJoint = this.gameObject.AddComponent<FixedJoint2D>();
            fixedJoint.enabled = false;
            fixedJoint.enableCollision = false;
            //fixedJoint.autoConfigureConnectedAnchor = false;
            //fixedJoint.anchor = new Vector2(0f, 0.05f);
            //fixedJoint.autoConfigureDistance = false;
            //fixedJoint.distance = 0.1f;
            fixedJoint.dampingRatio = 0.25f;
            fixedJoint.frequency = springJointMaxStrength;

            springJoint = this.gameObject.AddComponent<SpringJoint2D>();
            springJoint.enabled = false;
            springJoint.autoConfigureDistance = false;
            springJoint.autoConfigureConnectedAnchor = false;
            springJoint.anchor = Vector2.zero;
            springJoint.connectedAnchor = Vector2.zero;
            springJoint.distance = 0.005f;
            springJoint.dampingRatio = 0.1f;
            springJoint.frequency = 5f;
            
            GameObject mouseClickColliderGO = new GameObject("MouseClickCollider");
            mouseClickColliderGO.transform.parent = this.gameObject.transform;
            mouseClickColliderGO.transform.localPosition = new Vector3(0f, 0f, 1f);
            mouseClickCollider = mouseClickColliderGO.AddComponent<CapsuleCollider>();
            mouseClickCollider.isTrigger = true;
            mouseClickColliderGO.SetActive(false);
        }
    }

    public void InitializeEggSackFromGenome(int index, AgentGenome agentGenome, Agent parentAgent, Vector3 startPos) {
        parentAgentIndex = parentAgent.index;
        this.index = index;        
        this.fullSize = new Vector2(agentGenome.bodyGenome.coreGenome.fullBodyWidth, agentGenome.bodyGenome.coreGenome.fullBodyLength) * 1f;
        foodAmount = this.fullSize.x * this.fullSize.y;
       
        BeginLifeStageGrowing(parentAgent, agentGenome, startPos);
    }
    /*public void InitializeEggSackFromGenomeImmaculate(int eggSackIndex, AgentGenome genome, StartPositionGenome startPos) {
        curLifeStage = EggLifeStage.GrowingIndependent;

        parentAgentIndex = 0;

        index = eggSackIndex;
        float parentScale = genome.bodyGenome.coreGenome.fullBodyWidth * 0.5f + genome.bodyGenome.coreGenome.fullBodyLength * 0.5f;
        this.fullSize.x = parentScale * 0.9f;  // golden ratio? // easter egg for math nerds?
        this.fullSize.y = this.fullSize.x * 1.16f;    
        
        //this.fullSize = new Vector2(genome.bodyGenome.coreGenome.fullBodyWidth, genome.bodyGenome.coreGenome.fullBodyLength) * 0.64f;

        foodAmount = this.fullSize.x * this.fullSize.y;
        this.fullSize *= 1.2f;
        
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
    }*/

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

    private void BeginLifeStageGrowing(Agent parentAgentRef, AgentGenome parentGenomeRef, Vector3 startPos) {
        curLifeStage = EggLifeStage.Growing;        
        lifeStageTransitionTimeStepCounter = 0;
        this.parentAgentRef = parentAgentRef;
        birthTimeStepsCounter = 0;
        
        growthStatus = 0f;
        decayStatus = 0f;

        //rigidbodyRef.MovePosition(parentAgent.bodyRigidbody.position);
        rigidbodyRef.mass = 0.05f;
        rigidbodyRef.velocity = Vector2.zero;
        rigidbodyRef.angularVelocity = 0f;
        rigidbodyRef.drag = 7.5f;
        rigidbodyRef.angularDrag = 5f;

        if(parentAgentRef == null) {
            //curLifeStage = EggLifeStage.GrowingIndependent;
            isProtectedByParent = false;
            isAttachedBySpring = false;

            parentAgentIndex = -1;

            this.transform.localPosition = startPos;  
            
            //index = eggSackIndex;
            // REVISIT THIS:
            float parentScale = parentGenomeRef.bodyGenome.coreGenome.fullBodyWidth * 0.5f + parentGenomeRef.bodyGenome.coreGenome.fullBodyLength * 0.5f;
            this.fullSize.x = parentScale * 0.9f;  // golden ratio? // easter egg for math nerds?
            this.fullSize.y = parentScale * 1.16f;            
            foodAmount = this.fullSize.x * this.fullSize.y;
            this.fullSize *= 1.2f;
            //*******        
              
            mainCollider.enabled = true;
        }
        else {
            isProtectedByParent = true;
            isAttachedBySpring = true;

            rigidbodyRef.transform.position = parentAgentRef.bodyRigidbody.position;
        
            fixedJoint.connectedBody = parentAgentRef.bodyRigidbody;
            fixedJoint.autoConfigureConnectedAnchor = false;
            fixedJoint.anchor = new Vector2(0f, this.parentAgentRef.coreModule.coreLength * 0.05f);
            fixedJoint.connectedAnchor = new Vector2(0f, -this.parentAgentRef.coreModule.coreLength * 0.05f);            
            fixedJoint.enableCollision = false;
            fixedJoint.enabled = true;
            fixedJoint.frequency = springJointMaxStrength;

            mainCollider.enabled = true;  // ?? maybe??   ****** Refactor this to distance-threshold method

        }
        
        isDepleted = false;
        isBeingBorn = false;        
        healthStructural = 1f;
        prevPos = transform.position;        

        UpdateEggSackSize(0.1f);
    }
    private void CommenceBeingBorn() {
        //curLifeStage = EggLifeStage.BeingBorn;
        //Debug.Log("Begin Birth!");
        birthTimeStepsCounter = 0;
        isBeingBorn = true;

        //isAttachedBySpring = true; 
        isProtectedByParent = false;

    }
    /*private void BeginLifeStageGrowingIndependent() {
        curLifeStage = EggLifeStage.GrowingIndependent;
        //Debug.Log("EGGS FULLY LAID (LAIN?)!");
        lifeStageTransitionTimeStepCounter = 0;

        if(parentAgentRef != null) {
            parentAgentRef.AbortPregnancy(); // ???
            parentAgentRef = null;   
        }             

        fixedJoint.enabled = false;
        fixedJoint.enableCollision = false;
        //fixedJoint.connectedBody = null;
        
        fixedJoint.dampingRatio = 0.001f;
        fixedJoint.frequency = 0f;

        mainCollider.enabled = true;
        // size collider properly here:
    }*/
    private void BeginLifeStageMature() {  // Buffer time for late bloomers to hatch
        curLifeStage = EggLifeStage.Mature;
                    
        lifeStageTransitionTimeStepCounter = 0;

        growthStatus = 1f;

        UpdateEggSackSize(1f);
    }
    public void ParentDiedWhilePregnant() {

        parentAgentRef = null;
        fixedJoint.enabled = false;
        fixedJoint.enableCollision = false;
        fixedJoint.connectedBody = null;

        curLifeStage = EggLifeStage.Decaying;

        lifeStageTransitionTimeStepCounter = 0;
    }

    private void SeverJointAttachment() {
        //Debug.Log("SeverJoint!");
        parentAgentRef = null;
        isAttachedBySpring = false;
        isProtectedByParent = false; // might not be necessary, as this should be flipped at frame 1 of birth?
        fixedJoint.enabled = false;
        fixedJoint.enableCollision = false;
        fixedJoint.connectedBody = null;
    }
   
    private void CheckForLifeStageTransition() {
        switch(curLifeStage) {
            case EggLifeStage.Growing:
                // 
                // Preggers
                if(lifeStageTransitionTimeStepCounter >= Mathf.RoundToInt((float)growDurationTimeSteps * (float)pregnancyPercentageOfTotalGrowTime)) {                   
                    // transition from being attached to parent Agent rigidbody, to free-floating:
                    if(!isBeingBorn) {
                        CommenceBeingBorn();  // only start it once
                    }                    
                }

                if(lifeStageTransitionTimeStepCounter >= growDurationTimeSteps) {
                    BeginLifeStageMature();
                }
                                
                if(birthTimeStepsCounter >= birthDurationTimeSteps) {
                    // Birth time is up
                    isBeingBorn = false;
                }

                if(!isProtectedByParent && isAttachedBySpring) {
                    float distToParent = (parentAgentRef.bodyRigidbody.transform.position - rigidbodyRef.transform.position).magnitude;
                    //if(distToParent > (fullSize.magnitude + parentAgentRef.fullSizeBoundingBox.magnitude) * 1.5f) {
                        // Can also use an OnTriggerExit?
                        SeverJointAttachment();
                    //}
                }
                
                break;
            /*case EggLifeStage.BeingBorn:
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
                break;
                */
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
                    lifeStageTransitionTimeStepCounter = 0;
                }
                if(transform.position.x > SimulationManager._MapSize || transform.position.x < 0f || transform.position.y > SimulationManager._MapSize || transform.position.y < 0f) {
                    curLifeStage = EggLifeStage.Decaying;
                    lifeStageTransitionTimeStepCounter = 0;
                }
                break;
            case EggLifeStage.Decaying:
                //
                if(lifeStageTransitionTimeStepCounter >= decayDurationTimeSteps) {
                    curLifeStage = EggLifeStage.Null;
                    //Debug.Log("FOOD NO LONGER EXISTS!");
                    lifeStageTransitionTimeStepCounter = 0;
                    isDepleted = true;  // flagged for respawn
                    mainCollider.enabled = false;

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
        //facingDirection = new Vector2(Mathf.Cos(Mathf.Deg2Rad * transform.localEulerAngles.z + Mathf.PI * 0.5f), Mathf.Sin(Mathf.Deg2Rad * transform.localEulerAngles.z + Mathf.PI * 0.5f));

        float rotationInRadians = (rigidbodyRef.transform.localRotation.eulerAngles.z + 90f) * Mathf.Deg2Rad;
        facingDirection = new Vector2(Mathf.Cos(rotationInRadians), Mathf.Sin(rotationInRadians));

        // Check for StateChange:
        CheckForLifeStageTransition();
        
        switch(curLifeStage) {
            case EggLifeStage.Growing:
                //
                TickGrowing();
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

    private void TickGrowing() {
        lifeStageTransitionTimeStepCounter++;

        float growthPercentage = (float)lifeStageTransitionTimeStepCounter / (float)(growDurationTimeSteps);
        growthStatus = growthPercentage;

        if(lifeStageTransitionTimeStepCounter % numSkipFramesResize == 0) {
            UpdateEggSackSize(growthStatus);
        }  

        if(isBeingBorn) {
            birthTimeStepsCounter++;

            
        }
        if(isAttachedBySpring) {
            float birthPercentage = Mathf.Clamp01((float)birthTimeStepsCounter / (float)birthDurationTimeSteps);
            //float lerpVal = birthPercentage;

            //float jointStrength = Mathf.Lerp(springJointMaxStrength, 0f, lerpVal);
            //fixedJoint.frequency = jointStrength;
            //fixedJoint.dampingRatio = Mathf.Lerp(0.25f, 0f, birthPercentage);

            //Debug.Log("_" + fixedJoint.ToString());
            //Debug.Log("_" + parentAgentRef.ToString());
            fixedJoint.anchor = Vector2.Lerp(new Vector2(0f, parentAgentRef.coreModule.coreLength * 0.05f), new Vector2(0f, parentAgentRef.coreModule.coreLength * 2f), birthPercentage);
            fixedJoint.connectedAnchor = Vector2.Lerp(new Vector2(0f, -parentAgentRef.coreModule.coreLength * 0.05f), new Vector2(0f, -parentAgentRef.coreModule.coreLength * 2f), birthPercentage);

        }
    }
    /*private void TickGrowingInsideParent() {    ///  ***** v v v *** REFACTOR!!!!!
        lifeStageTransitionTimeStepCounter++;

        float growthPercentage = (float)lifeStageTransitionTimeStepCounter / (float)(pregnantDurationTimeSteps + birthDurationTimeSteps);
        growthStatus = growthPercentage;

        if(lifeStageTransitionTimeStepCounter % numSkipFramesResize == 0) {
            UpdateEggSackSize(growthStatus);
        }        
    }
    private void TickBeingBorn() {
        lifeStageTransitionTimeStepCounter++;

        float growthPercentage = (float)(lifeStageTransitionTimeStepCounter + pregnantDurationTimeSteps) / (float)(pregnantDurationTimeSteps + birthDurationTimeSteps);
        growthStatus = growthPercentage;
        
        float birthPercentage = Mathf.Clamp01((float)lifeStageTransitionTimeStepCounter / (float)birthDurationTimeSteps);
        float lerpVal = birthPercentage * birthPercentage;

        float springStrength = Mathf.Lerp(springJointMaxStrength, 0f, lerpVal);
        //float springDistance = Mathf.Lerp(0f, parentAgentRef.colliderBody.size.magnitude, birthPercentage);

        fixedJoint.frequency = springStrength;
        //fixedJoint.dampingRatio = Mathf.Lerp(0.25f, 0f, birthPercentage);

        Debug.Log("_" + fixedJoint.ToString());
        Debug.Log("_" + parentAgentRef.ToString());
        fixedJoint.anchor = Vector2.Lerp(new Vector2(0f, parentAgentRef.coreModule.coreLength * 0.05f), new Vector2(0f, parentAgentRef.coreModule.coreLength * 1.4f), birthPercentage);
        fixedJoint.connectedAnchor = Vector2.Lerp(new Vector2(0f, -parentAgentRef.coreModule.coreLength * 0.05f), new Vector2(0f, -parentAgentRef.coreModule.coreLength * 1.4f), birthPercentage);

    }
    private void TickGrowingIndependent() {
        lifeStageTransitionTimeStepCounter++;

        float growthPercentage = 1f; // Mathf.Clamp01((float)(lifeStageTransitionTimeStepCounter) / (float)(36));
        growthStatus = growthPercentage;

        if(lifeStageTransitionTimeStepCounter % numSkipFramesResize == 0) {
            UpdateEggSackSize(growthStatus); // * (1f - timeOfBirthGrowthPercentage) + timeOfBirthGrowthPercentage);
        }
    }*/
    private void TickMature() {
        lifeStageTransitionTimeStepCounter++;
        growthStatus = 1f;                
        isDepleted = CheckIfDepleted();      
    }
    private void TickDecaying() {
        float decayPercentage = (float)lifeStageTransitionTimeStepCounter / (float)decayDurationTimeSteps;
        decayStatus = decayPercentage;
        //curSize = Vector2.Lerp(fullSize * 0.4f, new Vector3(0.0f, 0.0f), Mathf.Clamp01(decayStatus * 6f));
        //transform.localScale = new Vector3(curSize.x, curSize.y, 1f);
        lifeStageTransitionTimeStepCounter++;
    }
    
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
}
