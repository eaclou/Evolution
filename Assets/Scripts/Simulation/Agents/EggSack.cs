using UnityEngine;

public class EggSack : MonoBehaviour {
    SettingsManager settingsRef => SettingsManager.instance;

    public int index;    
    public int speciesIndex; // temp - based on static species    

    public EggLifeStage curLifeStage;
    public enum EggLifeStage {
        Growing,
        Mature,    // Developed enough for some eggs to start hatching
        Decaying,  // all eggs either hatched or failed
        Null
    }
    
    public bool isNull => curLifeStage == EggLifeStage.Null;
    public bool isMature => curLifeStage == EggLifeStage.Mature;

    private float pregnancyPercentageOfTotalGrowTime = 0.25f;
    private int birthDurationTimeSteps = 30;
    private int growDurationTimeSteps = 240;

    private int matureDurationTimeSteps = 1000;  // buffer time for late bloomers to spawn
    public int _MatureDurationTimeSteps => matureDurationTimeSteps;

    private int decayDurationTimeSteps = 1440;  // how long it takes to rot/decay

    public float individualEggMaxSize = 0.1f;
    public int maxNumEggs = 64;
    public int curNumEggs = 64;
    public float foodAmount;  // if eaten, how much nutrients is the entire EggSack worth?
    
    public float eggSize => individualEggMaxSize * growthScaleNormalized;

    public float developmentProgress = 0f; // 0-1 born --> mature
    public float growthScaleNormalized = 0f; // the normalized size compared to max fullgrown size
    public float decayStatus = 0f;

    public bool isDepleted = false;

    public Vector2 fullSize;
    public Vector2 curSize;
    public float area => curSize.x * curSize.y;

    //private float minMass = 0.33f;
    //private float maxMass = 3.33f;

    public float isBeingEaten = 0f;
    public float healthStructural = 1f;
    public int lifeStageTransitionTimeStepCounter = 0; // keeps track of how long food has been in its current lifeStage

    private int numSkipFramesResize = 7;

    public Vector2 prevPos { get; private set; }

    public Vector2 facingDirection;
        
    public FixedJoint2D fixedJoint;
    public CapsuleCollider2D mainCollider;
    public Rigidbody2D rigidbodyRef;
    public Agent parentAgent;
    public int parentAgentIndex;

    // * Not used
    public Agent predatorAgent;
    public SpringJoint2D springJoint;
    public CapsuleCollider mouseClickCollider;

    private float springJointMaxStrength = 15f;

    public bool isProtectedByParent = false;
    public bool isAttachedBySpring = false;  
    public bool isBeingBorn = false;    
    public int birthTimeStepsCounter = 0;

    public float currentBiomass = 0f;
    public float currentStoredEnergy = 0f;
    public float wasteProducedLastFrame = 0f;
    public float oxygenUsedLastFrame = 0f;
    
    
    public void FirstTimeInitialize() {
        fixedJoint.frequency = springJointMaxStrength;
    }

    public void InitializeEggSackFromGenome(int index, AgentGenome agentGenome, Agent parentAgent, Vector3 startPos) {
        //currentBiomass = 0.01f; // immaculate eggsacks given free mass?
        string debugTxT = "NULL";
        if (parentAgent) {  
            parentAgentIndex = parentAgent.index;
            debugTxT = "" + parentAgent.speciesIndex;
        }
        else {

        }
        //Debug.Log(debugTxT + "InitializeEggSackFromGenome(int index" + index + ", AgentGenome agentGenome" + ", parentAgentIndex: " + parentAgentIndex + ", Vector3 startPos " + startPos);
        this.index = index;        
        fullSize = Vector2.one * (agentGenome.bodyGenome.GetFullsizeBoundingBox().x + agentGenome.bodyGenome.GetFullsizeBoundingBox().y) * 0.5f; // new Vector2(agentGenome.bodyGenome.fullsizeBoundingBox.x, agentGenome.bodyGenome.fullsizeBoundingBox.y) * 1f;
               
        BeginLifeStageGrowing(parentAgent, agentGenome, startPos);
    }

    public void Nullify()
    {
        isDepleted = true;
        curLifeStage = EggLifeStage.Null;
    }

    private void UpdateEggSackSize(float percentage, bool resizeCollider) {
        Vector2 newSize = fullSize * Mathf.Max(0.01f, percentage);
        
        if (resizeCollider) {
            mainCollider.size = fullSize * Mathf.Clamp01(Mathf.Max(0.01f, percentage + 0.2f));
            float mass = mainCollider.size.x * mainCollider.size.y; // Mathf.Lerp(minMass, maxMass, percentage);  // *** <<< REVISIT!!! ****
            mass = Mathf.Max(mass, 0.25f);  // constrain minimum
            rigidbodyRef.mass = mass;
        }  

        curSize = newSize;
        healthStructural = (float)curNumEggs / (float)maxNumEggs;
        foodAmount = healthStructural * curSize.x * curSize.y;
    }

    private void BeginLifeStageGrowing(Agent parent, AgentGenome parentGenome, Vector3 startPos) {
        curLifeStage = EggLifeStage.Growing;        
        lifeStageTransitionTimeStepCounter = 0;
        parentAgent = parent;
        birthTimeStepsCounter = 0;
        
        curNumEggs = maxNumEggs;
        developmentProgress = 0f;
        growthScaleNormalized = 0.01f;
        decayStatus = 0f;
                
        rigidbodyRef.mass = 5f;
        rigidbodyRef.velocity = Vector2.zero;
        rigidbodyRef.angularVelocity = 0f;
        rigidbodyRef.drag = 7.5f;
        rigidbodyRef.angularDrag = 1.5f;
        
        isProtectedByParent = parent;
        isAttachedBySpring = parent;

        if (!parent) 
        {
            transform.localPosition = startPos;  
            
            // REVISIT THIS:
            var parentBoundingBox = parentGenome.bodyGenome.GetFullsizeBoundingBox();
            float parentScale = (parentBoundingBox.x + parentBoundingBox.y) * 0.5f;
            fullSize.x = parentScale;  // golden ratio? // easter egg for math nerds?
            fullSize.y = parentScale;
        }
        else 
        {
            //currentBiomass = 0.05f;
            //parentAgentRef.currentBiomass -= 0.05f; // *** Handled within agent initPregnancy
            rigidbodyRef.transform.position = parent.bodyRigidbody.position;
        
            fixedJoint.connectedBody = parent.bodyRigidbody;
            fixedJoint.autoConfigureConnectedAnchor = false;
            fixedJoint.anchor = new Vector2(0f, parent.fullSizeBoundingBox.y * 0.25f);
            fixedJoint.connectedAnchor = new Vector2(0f, parent.fullSizeBoundingBox.y * -0.25f);            
            fixedJoint.enableCollision = false;
            fixedJoint.enabled = true;
            fixedJoint.frequency = springJointMaxStrength;
        }
        
        mainCollider.enabled = true;
        
        isDepleted = false;
        isBeingBorn = false;
        prevPos = transform.position;        

        UpdateEggSackSize(0.05f, true);
    }
    
    private void CommenceBeingBorn() {
        //Debug.Log("Begin Birth!");
        birthTimeStepsCounter = 0;
        isBeingBorn = true;
        isProtectedByParent = false;
    }
    
    // Buffer time for late bloomers to hatch
    private void BeginLifeStageMature() {  
        curLifeStage = EggLifeStage.Mature;
        lifeStageTransitionTimeStepCounter = 0;
        developmentProgress = 1f;
        UpdateEggSackSize(1f, true);
    }
    
    public void ParentDiedWhilePregnant() {
        parentAgent = null;
        fixedJoint.enabled = false;
        fixedJoint.enableCollision = false;
        fixedJoint.connectedBody = null;

        curLifeStage = EggLifeStage.Decaying;
        lifeStageTransitionTimeStepCounter = 0;
    }

    private void SeverJointAttachment() {
        //Debug.Log("SeverJoint!");
        parentAgent.CompletedPregnancy();
        parentAgent = null;
        isAttachedBySpring = false;
        isProtectedByParent = false; // might not be necessary, as this should be flipped at frame 1 of birth?
        fixedJoint.enabled = false;
        fixedJoint.enableCollision = false;
        fixedJoint.connectedBody = null;
        mainCollider.enabled = true;  // *** ???? maybe?
    }
    
    public void BittenByAgent(int numEggsEaten, float massConsumed)
    {
        curNumEggs -= numEggsEaten;
        
        if (curNumEggs <= 0) 
        {
            curNumEggs = 0;            
            ConsumedByPredatorAgent();
        }

        currentBiomass -= massConsumed;
        
        // Deprecate this
        foodAmount = (float)curNumEggs / (float)maxNumEggs * curSize.x * curSize.y;
    }

    public void ConsumedByPredatorAgent() {
        mainCollider.enabled = false;
        curLifeStage = EggLifeStage.Null;
        isDepleted = true;        
        lifeStageTransitionTimeStepCounter = 0;
        growthScaleNormalized = 0.01f;
        UpdateEggSackSize(growthScaleNormalized, false);

        currentBiomass = 0f;

        fixedJoint.enabled = false;
        isAttachedBySpring = false;
        isProtectedByParent = false;
        decayStatus = 1f;
    }
    
    bool isPregnant => lifeStageTransitionTimeStepCounter >= Mathf.RoundToInt((float)growDurationTimeSteps * (float)pregnancyPercentageOfTotalGrowTime);
    bool isOutsideMap => transform.position.x > SimulationManager._MapSize || transform.position.x < 0f || transform.position.y > SimulationManager._MapSize || transform.position.y < 0f;
    bool birthTimeComplete => birthTimeStepsCounter >= birthDurationTimeSteps;

    private void CheckForLifeStageTransition() 
    {
        switch (curLifeStage) 
        {
            case EggLifeStage.Growing:
                // Transition from being attached to parent Agent rigidbody, to free-floating:
                // Only happens if this eggSack belongs to a pregnant parent Agent and only starts once
                if (isPregnant && !isBeingBorn && parentAgent) {
                    CommenceBeingBorn();
                }
                if (lifeStageTransitionTimeStepCounter >= growDurationTimeSteps) {
                    BeginLifeStageMature();
                }
                if (birthTimeComplete) {
                    isBeingBorn = false;
                }
                if (!isProtectedByParent && isAttachedBySpring) {
                    //float distToParent = (parentAgentRef.bodyRigidbody.transform.position - rigidbodyRef.transform.position).magnitude;
                    SeverJointAttachment();                    
                }
                break;
            case EggLifeStage.Mature:
                if (lifeStageTransitionTimeStepCounter >= matureDurationTimeSteps || isOutsideMap) {
                    curLifeStage = EggLifeStage.Decaying;
                    lifeStageTransitionTimeStepCounter = 0;
                }
                break;
            case EggLifeStage.Decaying:
                if (currentBiomass <= 0f) {
                    //Decay fully!
                    currentBiomass = 0f;
                    curLifeStage = EggLifeStage.Null;
                    lifeStageTransitionTimeStepCounter = 0;
                    isDepleted = true;  // flagged for respawn
                    mainCollider.enabled = false;
                }
                break;
            case EggLifeStage.Null:
                healthStructural = 0f; // temp hack
                break;
            default:
                Debug.LogError($"NO SUCH ENUM ENTRY IMPLEMENTED, YOU FOOL!!! ({curLifeStage})");
                break;
        }
    }
    
    public void Tick() {
        wasteProducedLastFrame = 0f;
        oxygenUsedLastFrame = 0f;
        //facingDirection = new Vector2(Mathf.Cos(Mathf.Deg2Rad * transform.localEulerAngles.z + Mathf.PI * 0.5f), Mathf.Sin(Mathf.Deg2Rad * transform.localEulerAngles.z + Mathf.PI * 0.5f));

        float rotationInRadians = (rigidbodyRef.transform.localRotation.eulerAngles.z + 90f) * Mathf.Deg2Rad;
        facingDirection = new Vector2(Mathf.Cos(rotationInRadians), Mathf.Sin(rotationInRadians));

        // Check for StateChange:
        CheckForLifeStageTransition();
        
        switch(curLifeStage) {
            case EggLifeStage.Growing:
                TickGrowing();
                break;
            case EggLifeStage.Mature:
                TickMature();
                break;
            case EggLifeStage.Decaying:
                TickDecaying();
                break;
            case EggLifeStage.Null:
                decayStatus = 1f;
                //Debug.Log("agent is null - probably shouldn't have gotten to this point...;");
                break;
            default:
                Debug.LogError($"NO SUCH ENUM ENTRY IMPLEMENTED, YOU FOOL!!! ({curLifeStage})");
                break;
        }
                
        growthScaleNormalized = developmentProgress * (healthStructural * (1f - decayStatus) * 0.75f + 0.25f);
        
        bool resizeCollider = lifeStageTransitionTimeStepCounter % numSkipFramesResize == 2;
        UpdateEggSackSize(growthScaleNormalized, resizeCollider);
        
        Vector3 curPos = transform.localPosition;        
        prevPos = curPos;

        isBeingEaten = 0.0f;
    }

    private void TickGrowing() {
        lifeStageTransitionTimeStepCounter++;

        developmentProgress = Mathf.Clamp01((float)lifeStageTransitionTimeStepCounter / (float)(growDurationTimeSteps));
                        
        if (isBeingBorn) {
            birthTimeStepsCounter++;
        }
        
        if (isAttachedBySpring) {
            float birthPercentage = Mathf.Clamp01((float)birthTimeStepsCounter / (float)birthDurationTimeSteps);
            //float lerpVal = birthPercentage;
            //float jointStrength = Mathf.Lerp(springJointMaxStrength, 0f, lerpVal);
            //fixedJoint.frequency = jointStrength;
            //fixedJoint.dampingRatio = Mathf.Lerp(0.25f, 0f, birthPercentage);
            fixedJoint.anchor = Vector2.Lerp(Vector2.zero, new Vector2(0f, parentAgent.fullSizeBoundingBox.y * 0.25f), birthPercentage);
            fixedJoint.connectedAnchor = Vector2.Lerp(Vector2.zero, new Vector2(0f, parentAgent.fullSizeBoundingBox.y * -0.25f), birthPercentage);
        }
    } 
       
    private void TickMature() {
        lifeStageTransitionTimeStepCounter++;
        //growthScaleNormalized = 1f;                
        isDepleted = CheckIfDepleted();      
    }
    
    private void TickDecaying() {
        float decayAmount = settingsRef.agentSettings._BaseDecompositionRate;
        currentBiomass -= decayAmount;
        wasteProducedLastFrame += decayAmount;

        if(currentBiomass <= 0f) {
            currentBiomass = 0f;
        }

        // OLD:::
        float decayPercentage = (float)lifeStageTransitionTimeStepCounter / (float)decayDurationTimeSteps;
        decayStatus = decayPercentage;
        lifeStageTransitionTimeStepCounter++;
    }
    
    private bool CheckIfDepleted() {
        return currentBiomass <= 0f;
    }
}

#region Dead Code (please delete)

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

#endregion
