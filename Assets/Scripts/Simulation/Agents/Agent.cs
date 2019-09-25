using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {

    SettingsManager settingsRef;
    //public float totalFoodEatenDecay = 0f;
    public float totalFoodEatenPlant = 0f;
    public float totalFoodEatenMeat = 0f;
    public float totalDamageDealt = 0f;
    public float totalDamageTaken = 0f;

    public float speed = 500f;
    public float smoothedThrottleLerp = 0.2f;
    public float animationCycle = 0f;
    public float turningAmount = 0f;
    public float swimAnimationCycleSpeed = 0.1f;

    public float spawnStartingScale = 0.1f; // *** REFACTOR!!! SYNC WITH EGGS!!!

    public bool isInert = true;  // when inert, colliders disabled
    // Refactor??
    public bool isActing = false;  // biting, defending, dashing, etc -- exclusive actions
    public bool isResting = false;

    public bool isMarkedForDeathByUser = false;

    public int index;    
    public int speciesIndex = -1;  // ********************** NEED to set these at birth!
    public CandidateAgentData candidateRef;

    public string stringCauseOfDeath = "";
    public string lastEvent = "";
    public int lastEventTime = 0;
    
    public AgentLifeStage curLifeStage;
    public enum AgentLifeStage {
        AwaitingRespawn,  // can i merge this in with null?
        Egg,
        Mature,
        Dead,
        Null
    }
    private int gestationDurationTimeSteps = 21;
    public int _GestationDurationTimeSteps
    {
        get
        {
            return gestationDurationTimeSteps;
        }
        set
        {

        }
    }
    
    public int maxAgeTimeSteps = 100000;
    private int decayDurationTimeSteps = 420;
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
    private int maxGrowthPeriod = 2560;

    private int growthScalingSkipFrames = 8;

    public float sizePercentage = 0f;
    
    public Brain brain;
    public GameObject bodyGO;
    public Rigidbody2D bodyRigidbody;

    // MODULES:::
    public CritterModuleCommunication communicationModule;
    public CritterModuleCore coreModule;
    public CritterModuleEnvironment environmentModule;
    public CritterModuleFood foodModule;
    public CritterModuleFriends friendModule;
    public CritterModuleMovement movementModule;
    public CritterModuleThreats threatsModule;

    public CritterMouthComponent mouthRef;

    public CapsuleCollider2D colliderBody;
    public SpringJoint2D springJoint;   // Used to attach to EggSack Object while still in Egg stage
    public CapsuleCollider mouseClickCollider;
    
    public Vector3 fullSizeBoundingBox;  // ASSUMES Z=LENGTH, Y=HEIGHT, X=WIDTH
    public Vector3 currentBoundingBoxSize;
    public float fullSizeBodyVolume = 1f;
    //public float centerOfMass = 0f;
    
        // *********  Combine thse stats into a serializable class for cleanliness?  *****
    public int lifeStageTransitionTimeStepCounter = 0; // keeps track of how long agent has been in its current lifeStage
    public int ageCounter = 0;
    public float consumptionScore = 0f;
    public float damageDealtScore = 0f;
    public float damageReceivedScore = 0f;
    public float energyEfficiencyScore = 0f;
    public float reproductionScore = 0f;
    public float supportScore = 0f;
    public float masterFitnessScore = 0f;
    public float totalExperience = 0f;
    public float experienceForNextLevel = 2f; // 2, 4, 8, 16, 32, 64, 128, 256?
    public int curLevel = 0;
    
    public int pregnancyRefactoryTimeStepCounter = 0;

    // *** Need resource Overhaul
    public float currentBiomass = 0f;
    public float wasteProducedLastFrame = 0f;
    public float oxygenUsedLastFrame = 0f;
    //public float currentReproductiveStockpile = 0f;
    private float fullsizeBiomass = 1f;
    private float biomassAtDeath = 1f;
    
    private Vector3 prevPos;  // use these instead of sampling rigidbody?
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
    public float prevVel;
    public float curVel;
    public float curAccel;

    public Vector2 ownPos;
    public Vector2 ownVel;

    public Vector2 throttle;
    public Vector2 smoothedThrottle;
    public Vector2 facingDirection;  // based on throttle history

    public float avgVel;
    public Vector2 avgFluidVel;
    public float depth;
    
    public bool isSwallowingPrey = false;
    public bool isBeingSwallowed = false;
    public bool isSexuallyMature = false;
    public int beingSwallowedFrameCounter = 0;
    public int swallowingPreyFrameCounter = 0;
    public int swallowDuration = 60; // **** how does this mix with mouseComponent???? ***
    public Agent predatorAgentRef;
    public Agent preyAgentRef;

    public EggSack parentEggSackRef;  // instead of using own fixed embry development duration - look up parentEggSack and use its counter?
    public bool isAttachedToParentEggSack = false;

    public EggSack childEggSackRef;
    public bool isPregnantAndCarryingEggs = false;
    public int pregnancyRefactoryDuration = 1600;

    //public float overflowFoodAmount = 0f;
        
    // Use this for initialization
    private void Awake() {        
        // temp fix for delayed spawning of Agents (leading to nullReferenceExceptions)
        //agentWidthsArray = new float[widthsTexResolution];
        isInert = true;
    }
    
    public void SetToAwaitingRespawn() {
        curLifeStage = AgentLifeStage.AwaitingRespawn;

        isInert = true;
        isMarkedForDeathByUser = false;
    }

    public void InitiateBeingSwallowed(Agent predatorAgent)
    {
        curLifeStage = Agent.AgentLifeStage.Dead; // ....

        isInert = true;

        colliderBody.enabled = false;
        isBeingSwallowed = true;
        isSexuallyMature = false;
        beingSwallowedFrameCounter = 0;
        predatorAgentRef = predatorAgent;

        springJoint.connectedBody = predatorAgentRef.bodyRigidbody;
        springJoint.autoConfigureConnectedAnchor = false;
        springJoint.anchor = Vector2.zero;
        springJoint.connectedAnchor = Vector2.zero;
        springJoint.autoConfigureDistance = false;
        springJoint.distance = 0.005f;
        springJoint.enableCollision = false;
        springJoint.enabled = true;
        springJoint.frequency = 15f;
    }
    public void InitiateSwallowingPrey(Agent preyAgent)
    {
        isSwallowingPrey = true;
        swallowingPreyFrameCounter = 0;
        preyAgentRef = preyAgent;
    }

    //private int GetNumInputs() {
        //return 44;  // make more robust later!
    //} // *** UPGRADE!!!!
    //private int GetNumOutputs() {
    //    return 7;  // make more robust later!
    //} // *** UPGRADE!!!!
    /*public DataSample RecordData() {
        DataSample sample = new DataSample(GetNumInputs(), GetNumOutputs());
        
        bias = new float[1];   //0
        foodPosX = new float[1];  //1
        foodPosY = new float[1]; // 2
        foodDirX = new float[1];  // 3
        foodDirY = new float[1];  // 4
        foodTypeR = new float[1]; // 5
        foodTypeG = new float[1]; // 6
        foodTypeB = new float[1]; // 7

        friendPosX = new float[1]; // 8
        friendPosY = new float[1]; // 9
        friendVelX = new float[1]; // 10
        friendVelY = new float[1]; // 11
        friendDirX = new float[1]; // 12
        friendDirY = new float[1]; // 13

        enemyPosX = new float[1]; // 14
        enemyPosY = new float[1]; // 15
        enemyVelX = new float[1]; // 16
        enemyVelY = new float[1]; // 17
        enemyDirX = new float[1]; // 18
        enemyDirY = new float[1]; // 19

        ownVelX = new float[1]; // 20
        ownVelY = new float[1]; // 21
        temperature = new float[1]; // 22
        pressure = new float[1]; // 23
        isContact = new float[1]; // 24
        contactForceX = new float[1]; // 25
        contactForceY = new float[1]; // 26
        hitPoints = new float[1]; // 27
        stamina = new float[1]; // 28
        foodAmountR = new float[1]; // 29
        foodAmountG = new float[1]; // 30
        foodAmountB = new float[1]; // 31

        distUp = new float[1]; // 32 // start up and go clockwise!
        distTopRight = new float[1]; // 33
        distRight = new float[1]; // 34
        distBottomRight = new float[1]; // 35
        distDown = new float[1]; // 36
        distBottomLeft = new float[1]; // 37
        distLeft = new float[1]; // 38
        distTopLeft = new float[1]; // 39

        inComm0 = new float[1]; // 40
        inComm1 = new float[1]; // 41
        inComm2 = new float[1]; // 42
        inComm3 = new float[1]; // 43 
        // 44 Total Inputs

        throttleX = new float[1]; // 0
        throttleY = new float[1]; // 1
        dash = new float[1]; // 2
        outComm0 = new float[1]; // 3
        outComm1 = new float[1]; // 4
        outComm2 = new float[1]; // 5
        outComm3 = new float[1]; // 6 
        */
        /*
        sample.inputDataArray[0] = 1f; // bias
        sample.inputDataArray[1] = testModule.foodPosX[0];
        sample.inputDataArray[2] = testModule.foodPosY[0];
        sample.inputDataArray[3] = testModule.foodDirX[0];
        sample.inputDataArray[4] = testModule.foodDirY[0];
        sample.inputDataArray[5] = testModule.foodTypeR[0];
        sample.inputDataArray[6] = testModule.foodTypeG[0];
        sample.inputDataArray[7] = testModule.foodTypeB[0];
        sample.inputDataArray[8] = testModule.friendPosX[0];        
        sample.inputDataArray[9] = testModule.friendPosY[0];
        sample.inputDataArray[10] = testModule.friendVelX[0];
        sample.inputDataArray[11] = testModule.friendVelY[0];
        sample.inputDataArray[12] = testModule.friendDirX[0];
        sample.inputDataArray[13] = testModule.friendDirY[0];
        sample.inputDataArray[14] = testModule.enemyPosX[0];
        sample.inputDataArray[15] = testModule.enemyPosY[0];
        sample.inputDataArray[16] = testModule.enemyVelX[0];
        sample.inputDataArray[17] = testModule.enemyVelY[0];
        sample.inputDataArray[18] = testModule.enemyDirX[0];
        sample.inputDataArray[19] = testModule.enemyDirY[0];
        sample.inputDataArray[20] = testModule.ownVelX[0];
        sample.inputDataArray[21] = testModule.ownVelY[0];
        sample.inputDataArray[22] = testModule.temperature[0];
        sample.inputDataArray[23] = testModule.pressure[0];
        sample.inputDataArray[24] = testModule.isContact[0];
        sample.inputDataArray[25] = testModule.contactForceX[0];
        sample.inputDataArray[26] = testModule.contactForceY[0];
        sample.inputDataArray[27] = testModule.hitPoints[0];
        sample.inputDataArray[28] = testModule.stamina[0];
        sample.inputDataArray[29] = testModule.foodAmountR[0];
        sample.inputDataArray[30] = testModule.foodAmountG[0];
        sample.inputDataArray[31] = testModule.foodAmountB[0];

        sample.inputDataArray[32] = testModule.distUp[0];
        sample.inputDataArray[33] = testModule.distTopRight[0];
        sample.inputDataArray[34] = testModule.distRight[0];
        sample.inputDataArray[35] = testModule.distBottomRight[0];
        sample.inputDataArray[36] = testModule.distDown[0];
        sample.inputDataArray[37] = testModule.distBottomLeft[0];
        sample.inputDataArray[38] = testModule.distLeft[0];
        sample.inputDataArray[39] = testModule.distTopLeft[0];
        sample.inputDataArray[40] = testModule.inComm0[0];
        sample.inputDataArray[41] = testModule.inComm1[0];
        sample.inputDataArray[42] = testModule.inComm2[0];
        sample.inputDataArray[43] = testModule.inComm3[0];

        // @$!@$#!#% REVISIT THIS!! REDUNDANT CODE!!!!!!!!!!!!!!!!!!!!!!!!!!  movement script on Agent also does this....
        float outputHorizontal = 0f;
        if (Input.GetKey("left") || Input.GetKey("a")) {
            outputHorizontal += -1f;
        }
        if (Input.GetKey("right") || Input.GetKey("d")) {
            outputHorizontal += 1f;
        }
        float outputVertical = 0f;
        if (Input.GetKey("down") || Input.GetKey("s")) {
            outputVertical += -1f;
        }
        if (Input.GetKey("up") || Input.GetKey("w")) {
            outputVertical += 1f;
        }
        sample.outputDataArray[0] = outputHorizontal;
        sample.outputDataArray[1] = outputVertical;
        sample.outputDataArray[2] = 0f; //dash;
        sample.outputDataArray[3] = 0f; //outComm0;
        sample.outputDataArray[4] = 0f; //outComm1;
        sample.outputDataArray[5] = 0f; //outComm2;
        sample.outputDataArray[6] = 0f; //outComm3;

        

        return sample;
    }*/

    public void MapNeuronToModule(NID nid, Neuron neuron) {
        
        // Hidden nodes!
        if (nid.moduleID == -1) {
            neuron.currentValue = new float[1];
            neuron.neuronType = NeuronGenome.NeuronType.Hid;
            neuron.previousValue = 0f;
        }
        else {  // In/Out nodes:::
            communicationModule.MapNeuron(nid, neuron);
            coreModule.MapNeuron(nid, neuron);
            environmentModule.MapNeuron(nid, neuron);
            foodModule.MapNeuron(nid, neuron);
            friendModule.MapNeuron(nid, neuron);
            movementModule.MapNeuron(nid, neuron);
            threatsModule.MapNeuron(nid, neuron);
        }
    }
        
    public void ResetBrainState() {
        brain.ResetBrainState();
    }
    
    public void TickBrain() {
        brain.BrainMasterFunction();
    }
    public void TickModules(SimulationManager simManager) { // Updates internal state of body - i.e health, energy etc. -- updates input Neuron values!!!
                                // Update Stocks & Flows ::: new health, energy, stamina
                                // This should have happened during last frame's Internal PhysX Update

        // HOWEVER, some updates only happen once per frame and can be handled here, like converting food into energy automatically

        // Turns out that most of these updates are updating input neurons which tend to be sensors
        // These values are sometimes raw attributes & sometimes processed data
        // Should I break them up into individual sensor types -- like Ears, Collider Rangefind, etc.?
        // Separate sensors for each target type or add multiple data types to rangefinder raycasts?
        
        //UpdateInternalResources();  // update energy, stamina, food -- or do this during TickActions?

        ownPos = new Vector2(bodyRigidbody.transform.localPosition.x, bodyRigidbody.transform.localPosition.y);
        ownVel = new Vector2(bodyRigidbody.velocity.x, bodyRigidbody.velocity.y); // change this to ownPos - prevPos *****************

        coreModule.Tick();
        communicationModule.Tick(this);
        environmentModule.Tick(this);
        foodModule.Tick(simManager, this);
        friendModule.Tick(this);
        movementModule.Tick(this, ownVel);
        threatsModule.Tick(this);
        // Add more sensor Modules later:

        // Update Mouth::::         
        mouthRef.Tick();
    }
    
    private void CheckForDeathStarvation() {
        // STARVATION::
        if (coreModule.energy <= 0f) {
            if(coreModule.stomachContentsNorm > 0.01f) {
                stringCauseOfDeath = "Suffocated";
            }
            else {
                stringCauseOfDeath = "Starved";
            }
            curLifeStage = AgentLifeStage.Dead;
            lifeStageTransitionTimeStepCounter = 0;
            
            InitializeDeath();
        }
    }
    public void CheckForDeathHealth() {
        // HEALTH FAILURE:
        if (coreModule.healthHead <= 0f) {

            curLifeStage = Agent.AgentLifeStage.Dead;
            lifeStageTransitionTimeStepCounter = 0;
            
            coreModule.hitPoints[0] = 0f;
            coreModule.healthHead = 0f;
            coreModule.healthBody = 0f;
            coreModule.healthExternal = 0f;

            stringCauseOfDeath = "Fatal Injuries";
            InitializeDeath();
        }
        /*if (coreModule.healthBody <= 0f) {

            curLifeStage = Agent.AgentLifeStage.Dead;
            lifeStageTransitionTimeStepCounter = 0;
            
            coreModule.healthHead = 0f;
            coreModule.healthBody = 0f;
            coreModule.healthExternal = 0f;

            InitializeDeath();
        }*/
    }
    private void CheckForDeathOldAge() {
        if(ageCounter > maxAgeTimeSteps) {
            curLifeStage = Agent.AgentLifeStage.Dead;
            lifeStageTransitionTimeStepCounter = 0;

            //Debug.Log("Died of old age!");
            stringCauseOfDeath = "Old Age";
            InitializeDeath();
        }
    }
    private void CheckForDeathDivineJudgment() {
        if(isMarkedForDeathByUser) {
            curLifeStage = Agent.AgentLifeStage.Dead;
            lifeStageTransitionTimeStepCounter = 0;
            stringCauseOfDeath = "Divine Judgment";
            InitializeDeath();
        }
    }
    private void InitializeDeath()   // THIS CAN BE A LOT CLEANER!!!!! *****
    {    
        if(isPregnantAndCarryingEggs) {
            AbortPregnancy();
        }

        isSexuallyMature = false;
        isSwallowingPrey = false;
        swallowingPreyFrameCounter = 0;

        masterFitnessScore = totalExperience; // update this???
        
        mouthRef.Disable();
    }
    private void CheckForLifeStageTransition() {
        switch(curLifeStage) {
            case AgentLifeStage.AwaitingRespawn:
                //
                break;
            case AgentLifeStage.Egg:
                //
                if(lifeStageTransitionTimeStepCounter >= gestationDurationTimeSteps) {
                    BeginHatching();
                }
                break;
            case AgentLifeStage.Mature:
                
                // Check for Death:
                CheckForDeathStarvation();
                CheckForDeathHealth();
                CheckForDeathOldAge();
                CheckForDeathDivineJudgment();
                break;
            case AgentLifeStage.Dead:
                if(currentBiomass <= 0f && lifeStageTransitionTimeStepCounter >= decayDurationTimeSteps) {  // Fully decayed
                    curLifeStage = AgentLifeStage.Null;                    
                    lifeStageTransitionTimeStepCounter = 0;
                    isInert = true;
                }

                // REFACTOR THIS!!!! ******  SYNC actual biomass decay with visual percentage?!??

                // OLD::::
                /*
                if(lifeStageTransitionTimeStepCounter >= decayDurationTimeSteps) { //  Corpse naturally decayed without being fully consumed:
                    
                }*/
                break;
            case AgentLifeStage.Null:
                isInert = true;

                beingSwallowedFrameCounter = 0;
                isBeingSwallowed = false;
                isSwallowingPrey = false;

                parentEggSackRef = null;
                childEggSackRef = null;

                colliderBody.enabled = false;
                springJoint.enabled = false;
                springJoint.connectedBody = null;

                bodyRigidbody.velocity = Vector2.zero;
                bodyRigidbody.angularVelocity = 0f;
                              
                break;
            default:
                Debug.LogError("NO SUCH ENUM ENTRY IMPLEMENTED, YOU FOOL!!! (" + curLifeStage.ToString() + ")");
                break;
        }
    }
    
    public void EatFoodPlant(float amount) {
        totalFoodEatenPlant += amount;  
        float stomachSpace = coreModule.stomachCapacity - coreModule.stomachContentsPlant - coreModule.stomachContentsMeat;
        if(amount > stomachSpace) {
            amount = stomachSpace; // ??
        }
        //amount *= coreModule.foodEfficiencyPlant;
        coreModule.stomachContentsNorm += (amount / coreModule.stomachCapacity);
        
        if(coreModule.stomachContentsNorm > 1f) {
            float overStuffAmount = coreModule.stomachContentsNorm - 1f;
            //ProcessDamageReceived(overStuffAmount);
            coreModule.stomachContentsNorm = 1f;            
        }
        else {
            coreModule.stomachContentsPlant += amount;
        }
        GainExperience((amount / coreModule.stomachCapacity) * coreModule.foodEfficiencyPlant * 1f); // Exp for appropriate food    

        lastEvent = "Ate Plant! (" + amount.ToString() + ")";
        lastEventTime = UnityEngine.Time.frameCount;
    }
    public void EatFoodMeat(float amount) {
        totalFoodEatenMeat += amount; 
        float stomachSpace = coreModule.stomachCapacity - coreModule.stomachContentsPlant - coreModule.stomachContentsMeat;
        if(amount > stomachSpace) {
            amount = stomachSpace; // ??
        }
        //amount *= coreModule.foodEfficiencyMeat;
        coreModule.stomachContentsNorm += (amount / coreModule.stomachCapacity);
        
        if(coreModule.stomachContentsNorm > 1f) {
            float overStuffAmount = coreModule.stomachContentsNorm - 1f;
            //ProcessDamageReceived(overStuffAmount);
            coreModule.stomachContentsNorm = 1f;
        }
        else {
            coreModule.stomachContentsMeat += amount;
        }
        
        GainExperience((amount / coreModule.stomachCapacity) * coreModule.foodEfficiencyMeat * 1f); // Exp for appropriate food

        lastEvent = "Ate Zooplankton! (" + amount.ToString() + ")";
        lastEventTime = UnityEngine.Time.frameCount;
    }
    public void ProcessDamageReceived(float damage, Agent predatorAgentRef) {

        damage = damage / coreModule.healthBonus;

        float defendBonus = 1f;
        if(coreModule.isDefending) {
            if(coreModule.defendFrameCounter < coreModule.defendDuration) {
                defendBonus = 0.2f;
            }
            else {
                defendBonus = 1.25f; // cooldown penalty
            }
        }

        damage *= defendBonus;

        coreModule.hitPoints[0] -= damage;
        // currently no distinctionbetween regions:
        coreModule.healthHead -= damage;
        coreModule.healthBody -= damage;
        coreModule.healthExternal -= damage;

        totalDamageTaken += damage;
        predatorAgentRef.totalDamageDealt += damage;

        CheckForDeathHealth();

        lastEvent = "Bitten! (" + damage.ToString() + ") by #" + predatorAgentRef.index.ToString();
        lastEventTime = UnityEngine.Time.frameCount;
    }
    public void ProcessBeingEaten(float amount) {
        // if this agent is dead, it acts as food.
        // it was just bitten by another creature and removed material -- 

        currentBiomass -= amount;

        if (currentBiomass < 0f)
        {
            currentBiomass = 0f;

            coreModule.healthBody = 0f;
            coreModule.healthHead = 0f;
            coreModule.healthExternal = 0f;

            curLifeStage = AgentLifeStage.Null;
            lifeStageTransitionTimeStepCounter = 0;
                
            beingSwallowedFrameCounter = 0;
            isBeingSwallowed = false;

            colliderBody.enabled = false;
            springJoint.enabled = false;
            springJoint.connectedBody = null;

            // fully consumed?? Should this case be checked for earlier in the pipe ???
            // Need to 
        }
        else
        {
            ScaleBody(sizePercentage, true);

            // ******** CHANGE THIS LATER IT"S FUCKING AWFUL!!!! **************************************************  *** ***** ***** ***** **
            /*float sidesRatio = coreModule.coreWidth / coreModule.coreLength;
            float sideY = Mathf.Sqrt(currentCorpseFoodAmount / sidesRatio);
            float sideX = sideY * sidesRatio;

            // v v v move this into ScaleBody function?  Or re-organize into sub functions?
            // Do I even use currentBodySize as a trusted value?
            coreModule.currentBodySize = new Vector2(sideX, sideY);
            colliderBody.size = coreModule.currentBodySize;
            */
        }

        lastEvent = "Devoured!";
        lastEventTime = UnityEngine.Time.frameCount;
    }

    public void Tick(SimulationManager simManager, SettingsManager settings) {
        
        // Resources:
        wasteProducedLastFrame = 0f;
        oxygenUsedLastFrame = 0f;


        if(isBeingSwallowed)
        {
            beingSwallowedFrameCounter++;

            if(beingSwallowedFrameCounter >= swallowDuration + 6)
            {
                //Debug.Log("isBeingSwallowed + swallow Complete!");
                curLifeStage = AgentLifeStage.Null;
                lifeStageTransitionTimeStepCounter = 0;
                
                beingSwallowedFrameCounter = 0;
                isBeingSwallowed = false;

                colliderBody.enabled = false;
                springJoint.enabled = false;
                springJoint.connectedBody = null;
            }
            else
            {
                float scale = (float)beingSwallowedFrameCounter / (float)swallowDuration;

                ScaleBody((1.0f - scale) * 1f * sizePercentage, true);
            }
        }        

        // Any external inputs updated by simManager just before this

        // Check for StateChange:
        CheckForLifeStageTransition();
        
        switch(curLifeStage) {
            case AgentLifeStage.AwaitingRespawn:
                //
                break;
            case AgentLifeStage.Egg:
                //
                TickEgg();
                break;            
            case AgentLifeStage.Mature:
                //
                TickMature(simManager);
                break;
            case AgentLifeStage.Dead:
                //
                TickDead(settings);
                break;
            case AgentLifeStage.Null:
                //
                //Debug.Log("agent is null - probably shouldn't have gotten to this point...;");
                break;
            default:
                Debug.LogError("NO SUCH ENUM ENTRY IMPLEMENTED, YOU FOOL!!! (" + curLifeStage.ToString() + ")");
                break;
        }
        
        Vector3 curPos = bodyRigidbody.transform.position;
        
        curVel = (curPos - prevPos).magnitude;
        curAccel = Mathf.Lerp(curAccel, (curVel - prevVel), 0.2f);

        avgVel = Mathf.Lerp(avgVel, (curPos - prevPos).magnitude, 0.25f); // OLD

        prevPos = curPos;
        prevVel = curVel;
          
    }

    // *** Condense these into ONE?
    private void TickEgg() {        
        lifeStageTransitionTimeStepCounter++;

        sizePercentage = Mathf.Clamp01(((float)lifeStageTransitionTimeStepCounter / (float)gestationDurationTimeSteps) * spawnStartingScale);

        // Scaling Test:
        int frameNum = lifeStageTransitionTimeStepCounter % growthScalingSkipFrames;
        bool resizeCollider = false;
        if(frameNum == 1) {
            resizeCollider = true;
            this.bodyRigidbody.AddForce(UnityEngine.Random.insideUnitCircle * 250f * this.bodyRigidbody.mass * Time.deltaTime, ForceMode2D.Impulse);
        }
        ScaleBody(sizePercentage, resizeCollider);  

        if(isAttachedToParentEggSack) {
            if(parentEggSackRef == null) {
                //  *** eventually look into aborting agents who are attached to EggSacks which don't reach birth ***
            }
            else {
                float embryoPercentage = (float)lifeStageTransitionTimeStepCounter / (float)_GestationDurationTimeSteps;
                float targetDist = Mathf.Lerp(0.01f, parentEggSackRef.fullSize.magnitude * 0.15f, embryoPercentage);
                springJoint.distance = targetDist;

                if(parentEggSackRef.curLifeStage == EggSack.EggLifeStage.Null) {                    
                    isAttachedToParentEggSack = false;
                    springJoint.connectedBody = null;
                    springJoint.enabled = false;
                    colliderBody.enabled = true;
                    parentEggSackRef = null;  
                }
            }            
        }
        else {
            //springJoint.enableCollision = false;
        }
        
        
    }
    private void BeginHatching() {

        springJoint.connectedBody = null;
        springJoint.enabled = false;

        curLifeStage = AgentLifeStage.Mature;
        //Debug.Log("EGG HATCHED!");
        lifeStageTransitionTimeStepCounter = 0;
                
        // Detach from parent EggSack
        parentEggSackRef = null;
        isAttachedToParentEggSack = false;        
                
        colliderBody.enabled = true;

        currentBiomass = settingsRef.agentSettings._BaseInitMass;
        fullsizeBiomass = currentBiomass * 10f;
        coreModule.energy = currentBiomass * 20f;  // should be proportional to body size??

        mouthRef.Enable();

        lastEvent = "Was Born!";
        lastEventTime = UnityEngine.Time.frameCount;
        
    }
    
    private void TickMature(SimulationManager simManager) {

        //ProcessSwallowing();

        // Check for death & stuff? Or is this handled inside OnCollisionEnter() events?
        // Refactor this eventually!!!!
        //sizePercentage = Mathf.Clamp01(((float)ageCounter / (float)maxGrowthPeriod) * (1.0f - spawnStartingScale) + spawnStartingScale);  // *** Revisit this::: ***
        sizePercentage = Mathf.Clamp01(((currentBiomass - settingsRef.agentSettings._BaseInitMass) / fullsizeBiomass) * (1.0f - spawnStartingScale) + spawnStartingScale);

        // Scaling Test:
        int frameNum = ageCounter % growthScalingSkipFrames;
        bool resizeFrame = false;
        if(frameNum == 1) {
            resizeFrame = true;
            GainExperience(0.025f); // Auto Exp for staying alive
        }
        ScaleBody(sizePercentage, resizeFrame);  // change how growth works?

        TickModules(simManager); // update inputs for Brain        
        TickBrain(); // Tick Brain
        TickActions(simManager); // Execute Actions  -- Also Updates Resources!!! ***

        lifeStageTransitionTimeStepCounter++;
        ageCounter++;
                
        if(!isPregnantAndCarryingEggs) {
            pregnancyRefactoryTimeStepCounter++;
        }
        
    }
    public void BeginPregnancy(EggSack developingEggSackRef) {
        //Debug.Log("BeginPregnancy! [" + developingEggSackRef.index.ToString() + "]");
        isPregnantAndCarryingEggs = true;
        childEggSackRef = developingEggSackRef;

        // energy transfer within EggSack class
        float starterMass = settingsRef.agentSettings._BaseInitMass * settingsRef.agentSettings._MinPregnancyFactor;
        float curProportion = currentBiomass * settingsRef.agentSettings._MaxPregnancyProportion;
        // probably 0.05 * 2 = 0.1   for now
        if(curProportion > starterMass) { // Good to go!
            Debug.Log("Pregnancy! " + " curMass: " + currentBiomass.ToString() + ", reqMass: " + starterMass.ToString() + ", curProp: " + curProportion.ToString());
            currentBiomass -= starterMass;
            childEggSackRef.currentBiomass = starterMass;            
        }
        else {
            Debug.LogError("Something went wrong!! " + " curMass: " + currentBiomass.ToString() + ", reqMass: " + starterMass.ToString() + ", curProp: " + curProportion.ToString() );
        }
    }
    public void AbortPregnancy() {
        //childEggSackRef
        if(childEggSackRef != null) {
            childEggSackRef.ParentDiedWhilePregnant();
            childEggSackRef = null;
        }        

        isPregnantAndCarryingEggs = false;
        pregnancyRefactoryTimeStepCounter = 0;
    }
    public void CompletedPregnancy() {
        childEggSackRef = null;
        
        isPregnantAndCarryingEggs = false;
        pregnancyRefactoryTimeStepCounter = 0;
    }
    private void TickDead(SettingsManager settings) {
        lifeStageTransitionTimeStepCounter++;

        // DECAY HERE!
        // Should shrink as well as lose foodContent
        float decayAmount = settings.agentSettings._BaseDecompositionRate;

        // include reproductive stockpile energy???

        currentBiomass -= decayAmount;
        float wasteProducedMult = 1f;
        wasteProducedLastFrame += decayAmount * wasteProducedMult;

        if(currentBiomass <= 0f) {
            currentBiomass = 0f;
        }
    }

    private void ScaleBody(float sizePercentage, bool resizeColliders) {
        //segmentFullSizeArray
        //float minScale = 0.2f;
        float scale = Mathf.Lerp(spawnStartingScale, 1f, sizePercentage); // Minimum size = 0.1 ???  // SYNC WITH EGG SIZE!!!
        currentBoundingBoxSize = fullSizeBoundingBox * scale;
        float currentBodyVolume = currentBoundingBoxSize.y * (currentBoundingBoxSize.x + currentBoundingBoxSize.z) * 0.5f; // coreModule.currentBodySize.x * coreModule.currentBodySize.y;

        // REvisit this::::
        //currentBiomass = currentBodyVolume;  // ??? **** DOESN'T LOOK CORRECT -- FIX!!

        coreModule.stomachCapacity = 1f; // currentBodyVolume;
        
        if(resizeColliders) {
            colliderBody.size = new Vector2(currentBoundingBoxSize.x, currentBoundingBoxSize.y); // coreModule.currentBodySize;
            bodyRigidbody.mass = currentBodyVolume; 

            // MOUTH:
            mouthRef.triggerCollider.radius = currentBoundingBoxSize.x * 0.5f;  // ***** REVISIT THIS!!! USE GENOME FOR MOUTH!!! *****
            mouthRef.triggerCollider.offset = new Vector2(0f, currentBoundingBoxSize.y * 0.5f);
        
            // THIS IS HOT GARBAGE !!! RE-FACTOR!! *****
            mouseClickCollider.radius = currentBoundingBoxSize.x * 0.5f + 2f;        
            mouseClickCollider.height = currentBoundingBoxSize.y + 2f;
            //mouseClickCollider.radius += 2f; // ** TEMP -- should be based on camera distance also
            //mouseClickCollider.height += 2f;
        }               
    }

    public void TickActions(SimulationManager simManager) {
       
        
        float horizontalMovementInput = movementModule.throttleX[0]; // Mathf.Lerp(horAI, horHuman, humanControlLerp);
        float verticalMovementInput = movementModule.throttleY[0]; // Mathf.Lerp(verAI, verHuman, humanControlLerp);
        
        // Facing Direction:
        throttle = new Vector2(horizontalMovementInput, verticalMovementInput);        
        smoothedThrottle = Vector2.Lerp(smoothedThrottle, throttle, smoothedThrottleLerp);
        Vector2 throttleForwardDir = throttle.normalized;
        
        // ENERGY!!!!
        // Digestion:
        float maxDigestionRate = settingsRef.agentSettings._BaseDigestionRate * currentBiomass; // proportional to biomass?
        //float foodToEnergyBaseConversion = 1f; // what should this be?
        float totalStomachContents = (coreModule.stomachContentsPlant + coreModule.stomachContentsMeat);
        Vector2 foodProportionsVec = new Vector2(coreModule.stomachContentsPlant, coreModule.stomachContentsMeat) / (totalStomachContents + 0.000001f);

        float digestedAmountTotal = Mathf.Min(totalStomachContents, maxDigestionRate);

        float totalStomachContentsNorm = (coreModule.stomachContentsPlant + coreModule.stomachContentsMeat) / coreModule.stomachCapacity;
        coreModule.stomachContentsNorm = totalStomachContentsNorm; // ** we'll see.... ***
                                                                   //float digestedAmountTotal = Mathf.Min(totalStomachContentsNorm, maxDigestionRate);
                                                                   //float digestedProportionOfTotalContents = digestedAmountTotal / (totalStomachContentsNorm + 0.000001f);

        
        // *** Remember to Re-Implement dietary specialization!!! ****
        float digestedPlantMass = digestedAmountTotal * foodProportionsVec.x;
        float plantToEnergyAmount = digestedPlantMass; // * coreModule.foodEfficiencyPlant;
        float digestedMeatMass = digestedAmountTotal * foodProportionsVec.y;
        float meatToEnergyAmount = digestedMeatMass; // * coreModule.foodEfficiencyMeat;        
        
        float createdEnergyTotal = (plantToEnergyAmount + meatToEnergyAmount) * settingsRef.agentSettings._DigestionEnergyEfficiency;
        
        wasteProducedLastFrame += digestedAmountTotal * settingsRef.agentSettings._DigestionWasteEfficiency;
        oxygenUsedLastFrame = currentBiomass * settingsRef.agentSettings._BaseOxygenUsage;
        currentBiomass += digestedAmountTotal * settingsRef.agentSettings._GrowthEfficiency;  // **** <-- Reconsider
        if(currentBiomass > fullsizeBiomass) {
            wasteProducedLastFrame += (currentBiomass - fullsizeBiomass);
            currentBiomass = fullsizeBiomass;
        }
                
        coreModule.stomachContentsPlant -= digestedPlantMass;
        if(coreModule.stomachContentsPlant < 0f) {
            coreModule.stomachContentsPlant = 0f;
        }
        coreModule.stomachContentsMeat -= digestedMeatMass;
        if(coreModule.stomachContentsMeat < 0f) {
            coreModule.stomachContentsMeat = 0f;
        }

        float oxygenMask = Mathf.Clamp01(simManager.simResourceManager.curGlobalOxygen * settingsRef.agentSettings._OxygenEnergyMask);
        
        coreModule.energy += createdEnergyTotal * settingsRef.agentSettings._DigestionEnergyEfficiency * oxygenMask;

        //if(coreModule.energy > 1f) {
        //    coreModule.energy = 1f;
        //}

        // Heal:  // *** Re-Implement !!! ***************
        /*float healRate = 0.0005f;
        float energyToHealthConversionRate = 5f * coreModule.healthBonus;
        if(coreModule.healthBody < 1f) {
            coreModule.healthBody += healRate;
            coreModule.healthHead += healRate;
            coreModule.healthExternal += healRate;

            coreModule.energy -= healRate / energyToHealthConversionRate;
        }*/

        /* STAMINA:
        float staminaRefillRate = 0.0005f;
        float energyToStaminaConversionRate = 5f; // * coreModule.healthBonus;
        coreModule.stamina[0] += staminaRefillRate * energyToStaminaConversionRate;
        coreModule.energy -= staminaRefillRate; // / energyToStaminaConversionRate;
        */
        /*if(coreModule.stamina[0] < 0.25f) {
            staminaRefillRate *= 0.25f;
        }
        if(coreModule.stamina[0] > 0.75f) {
            staminaRefillRate *= 1.5f;
        }
        if(isResting) {
            staminaRefillRate *= 5f;
        }
        if(coreModule.stamina[0] < 1f) {
            coreModule.stamina[0] += staminaRefillRate;
            coreModule.energy -= staminaRefillRate / energyToStaminaConversionRate;
        }*/

        //ENERGY:
        float energyCost = currentBiomass * settingsRef.agentSettings._BaseEnergyCost; // * SimulationManager.energyDifficultyMultiplier; // / coreModule.energyBonus;
        
        float throttleMag = smoothedThrottle.magnitude;
        
        // ENERGY DRAIN::::
        coreModule.energy -= energyCost;
        if(coreModule.energy < 0f) {
            coreModule.energy = 0f;
        }

        //if(coreModule.stamina[0] <= 0f) {
//
        //}
        
        if(curLifeStage == AgentLifeStage.Dead || curLifeStage == AgentLifeStage.Egg) {
            throttle = Vector2.zero;
            smoothedThrottle = Vector2.zero;
        }
        else {
            bool startBite = false;
            // Food calc before energy/healing/etc? **************
            float sizeValue = BodyGenome.GetBodySizeScore01(candidateRef.candidateGenome.bodyGenome);
            // FOOD PARTICLES: Either mouth type for now:
            float foodParticleEatAmount = simManager.vegetationManager.plantParticlesEatAmountsArray[index];
            if(foodParticleEatAmount > 0f) {
                //mouthRef.InitiatePassiveBite();
                //float sizeEfficiencyPlant = Mathf.Lerp(settings.minSizeFeedingEfficiencyDecay, settings.maxSizeFeedingEfficiencyDecay, sizeValue);
                startBite = true;
                //Debug.Log("Agent[" + index.ToString() + "], Ate Plant: " + foodParticleEatAmount.ToString());
                EatFoodPlant(foodParticleEatAmount);                
            }

            float animalParticleEatAmount = simManager.zooplanktonManager.animalParticlesEatAmountsArray[index];
            if(animalParticleEatAmount > 0f) {
                //float sizeEfficiencyPlant = Mathf.Lerp(settings.minSizeFeedingEfficiencyDecay, settings.maxSizeFeedingEfficiencyDecay, sizeValue);
                animalParticleEatAmount *= 1f;
                //Debug.Log("Agent[" + index.ToString() + "], Ate Zooplankton: " + animalParticleEatAmount.ToString());
                EatFoodMeat(animalParticleEatAmount); // * sizeEfficiencyPlant);    
                startBite = true;
            }

            if(startBite) {
                mouthRef.AttemptInitiateActiveFeedBite(); 
            }
            

            // DECAY NUTRIENTS: // Grid:
            /*if(mouthRef.GetIsFeeding() > 0.5f) {
                //mouthRef.Enable();
                                        
                float ambientFoodDensity = nutrientCellInfo.x;
                float mouthArea = mouthRef.triggerCollider.radius * mouthRef.triggerCollider.radius * Mathf.PI;

                float maxEatRate = mouthArea * 4f * settings.eatRateMultiplier;
                // *** This is gross - CHANGE IT:::
                // Mathf.Clamp01(((fullSizeBoundingBox.x + fullSizeBoundingBox.z) * 0.5f - 0.035f) / 2f); // ** Hardcoded assuming size ranges from 0.1 --> 2.5 !!! ********
                float sizeEfficiencyDecay = Mathf.Lerp(settings.minSizeFeedingEfficiencyDecay, settings.maxSizeFeedingEfficiencyDecay, sizeValue) * ambientFoodDensity;
                    
                // *** Can double dip !!! BROKEN! **** Check reservoir first to avoid overdrafting!! ******
                float filteredFoodAmount = Mathf.Min(maxEatRate * sizeEfficiencyDecay, maxEatRate);
                   
                // Needs to use Compute shader here to sample the current nutrientMapRT:::: ****
                eatAmountsArray[index].x = filteredFoodAmount;

                EatFoodDecay(filteredFoodAmount);
            }  
            else {
                //mouthRef.Disable();
            }*/

            // *** REFACTOR THIS GARBAGE!!!!! ********
            float mostActiveEffectorVal = 0f;
            mostActiveEffectorVal = Mathf.Max(mostActiveEffectorVal, coreModule.mouthFeedEffector[0]);
            mostActiveEffectorVal = Mathf.Max(mostActiveEffectorVal, coreModule.mouthAttackEffector[0]);
            mostActiveEffectorVal = Mathf.Max(mostActiveEffectorVal, coreModule.defendEffector[0]);
            mostActiveEffectorVal = Mathf.Max(mostActiveEffectorVal, coreModule.dashEffector[0]);
            mostActiveEffectorVal = Mathf.Max(mostActiveEffectorVal, coreModule.healEffector[0]);

            /*if(coreModule.mouthFeedEffector[0] >= mostActiveEffectorVal) {                
                mouthRef.AttemptInitiateActiveFeedBite();                
            }*/
            /*if(coreModule.mouthAttackEffector[0] >= mostActiveEffectorVal) {                
                mouthRef.AttemptInitiateActiveAttackBite();                
            }*/
            /*if(coreModule.dashEffector[0] >= mostActiveEffectorVal) {
                if(!coreModule.isDashing && coreModule.stamina[0] > 0.1f) {
                    coreModule.isDashing = true;
                    //coreModule.stamina[0] -= 0.1f;
                }
            }
            if(coreModule.defendEffector[0] >= mostActiveEffectorVal) {
                if(!coreModule.isDefending && coreModule.stamina[0] > 0.1f) {
                    coreModule.isDefending = true;
                    //coreModule.stamina[0] -= 0.1f;
                }
            }
            
            isResting = false;
            if(coreModule.healEffector[0] >= mostActiveEffectorVal) {
                if(!coreModule.isDefending && !coreModule.isDashing && !mouthRef.isFeeding && !mouthRef.isAttacking) {
                    isResting = true;
                    //coreModule.stamina[0] -= 0.1f;
                }                
            }*/
                     
        }
        //coreModule.debugFoodValue = nutrientCellInfo.x;

        ApplyPhysicsForces(smoothedThrottle);

        float rotationInRadians = (bodyRigidbody.transform.localRotation.eulerAngles.z + 90f) * Mathf.Deg2Rad;
        facingDirection = new Vector2(Mathf.Cos(rotationInRadians), Mathf.Sin(rotationInRadians));
    }

    private void ApplyPhysicsForces(Vector2 throttle) {
        
        MovementScalingTest(throttle);
    }
    
    private void MovementScalingTest(Vector2 throttle) {
        // Save current joint angles:
        //for(int j = 0; j < numSegments - 1; j++) {
        //    jointAnglesArray[j] = hingeJointsArray[j].jointAngle;            
        //}

        float bitingPenalty = 1f;
        
        /*if(mouthRef.isBiting)
        {
            bitingPenalty = 1f;
        }*/
        if(coreModule.mouthFeedEffector[0] > 0f)
        {
            bitingPenalty = 0.5f;
        }
        
        float fatigueMultiplier = Mathf.Clamp01(coreModule.energy * 5f + 0.05f) * Mathf.Clamp01(coreModule.stamina[0] * 4f + 0.05f);
        float lowHealthPenalty = Mathf.Clamp01(coreModule.healthBody * 5f) * 0.5f + 0.5f;
        fatigueMultiplier *= lowHealthPenalty;
        //float growthStatus = 

        turningAmount = Mathf.Lerp(turningAmount, this.bodyRigidbody.angularVelocity * Mathf.Deg2Rad * 0.1f, 0.28f);

        animationCycle += smoothedThrottle.magnitude * swimAnimationCycleSpeed * (sizePercentage * 0.25f + 0.75f) * fatigueMultiplier; // (Mathf.Lerp(fullSizeBoundingBox.y, 1f, 1f)

        if (throttle.sqrMagnitude > 0.000001f) {  // Throttle is NOT == ZERO
            
            Vector2 headForwardDir = new Vector2(this.bodyRigidbody.transform.up.x, this.bodyRigidbody.transform.up.y).normalized;
            Vector2 headRightDir =  new Vector2(this.bodyRigidbody.transform.right.x, this.bodyRigidbody.transform.right.y).normalized;
            Vector2 throttleDir = throttle.normalized;

            float turnSharpness = (-Vector2.Dot(throttleDir, headForwardDir) * 0.5f + 0.5f);
            float headTurn = Vector2.Dot(throttleDir, headRightDir) * -1f * turnSharpness;
            float headTurnSign = Mathf.Clamp(Vector2.Dot(throttleDir, headRightDir) * -10000f, -1f, 1f);
                        
            float developmentMultiplier = Mathf.Lerp(0.25f, 1f, Mathf.Clamp01(sizePercentage * 2f));
            //turningAmount = Mathf.Lerp(turningAmount, this.bodyRigidbody.angularVelocity * Mathf.Deg2Rad * 0.1f, 0.15f);

            //this.rigidbodiesArray[0].AddForce(headForwardDir * speed * Time.deltaTime, ForceMode2D.Impulse);

            //animationCycle += 0.01f; // swimAnimationCycleSpeed; // * smoothedThrottle.magnitude / (Mathf.Lerp(fullSizeBoundingBox.y, 1f, 0.6f) * (growthPercentage * 0.4f + 0.6f));
            //animationCycle = animationCycle % 1.0f;

            // get size in 0-1 range from minSize to maxSize:
            float sizeValue = Mathf.Clamp01(coreModule.speedBonus * (candidateRef.candidateGenome.bodyGenome.coreGenome.creatureBaseLength - 0.2f) / 2f); ; // Mathf.Clamp01((fullSizeBoundingBox.x - 0.1f) / 2.5f); // ** Hardcoded assuming size ranges from 0.1 --> 2.5 !!! ********

            float aspectSpeedPenalty = Mathf.Lerp(1.2f, 0.4f, candidateRef.candidateGenome.bodyGenome.coreGenome.creatureAspectRatio);

            float swimSpeed = Mathf.Lerp(movementModule.smallestCreatureBaseSpeed, movementModule.largestCreatureBaseSpeed, sizeValue);
            float turnRate = Mathf.Lerp(movementModule.smallestCreatureBaseTurnRate, movementModule.largestCreatureBaseTurnRate, sizeValue);
            float dashBonus = 1f;
            /*if(coreModule.isDashing) {
                if(coreModule.dashFrameCounter < coreModule.dashDuration) {
                    dashBonus = 2f;
                }
                else {
                    dashBonus = 0.5f; // cooldown penalty
                }
            }*/
            speed = swimSpeed * movementModule.speedBonus * dashBonus * aspectSpeedPenalty;
            // Forward Slide
            //for(int k = 0; k < numSegments; k++) {
            Vector2 segmentForwardDir = new Vector2(this.bodyRigidbody.transform.up.x, this.bodyRigidbody.transform.up.y).normalized;

            Vector2 forwardThrustDir = Vector2.Lerp(segmentForwardDir, throttleDir, 0.1f).normalized;

            this.bodyRigidbody.AddForce(forwardThrustDir * (1f - turnSharpness * 0.25f) * swimSpeed * this.bodyRigidbody.mass * Time.deltaTime * developmentMultiplier * fatigueMultiplier * bitingPenalty, ForceMode2D.Impulse);
            //}

            // modify turning rate based on body proportions:
            float turnRatePenalty = Mathf.Lerp(0.25f, 1f, 1f - sizeValue);

            // Head turn:
            this.bodyRigidbody.AddTorque(Mathf.Lerp(headTurn, headTurnSign, 0.75f) * turnRatePenalty * turnRate * this.bodyRigidbody.mass * this.bodyRigidbody.mass * fatigueMultiplier * bitingPenalty * Time.deltaTime, ForceMode2D.Impulse);
            
            // OLD:::
            /*
            for(int i = 1; i < numSegments; i++) {

                float phaseOffset = (float)(i - 1) / (float)(numSegments - 1) * offsetDelay;
                
                float targetOscillateAngle = Mathf.Sin((-animationCycle + phaseOffset) * Mathf.PI * 2f * frequency);

                float oscillateTorque = targetOscillateAngle - (jointAnglesArray[i - 1] * Mathf.PI / 180f);
                
                float bendMultiplier = Mathf.Lerp(bendRatioHead, bendRatioTailTip, (float)(i - 1) / (float)(numSegments - 1));
                
                float targetTurnAngle = -10f * Vector2.Dot(throttleDir, headRightDir) * Mathf.PI / 4f / ((float)numSegments - 1f);
                float turningTorque = targetTurnAngle - (jointAnglesArray[i - 1] * Mathf.PI / 180f);

                float uTurnMultiplier = -Vector2.Dot(throttleDir, headRightDir) * 0.5f + 0.5f;
                float turningMultiplier = (Mathf.Abs(Vector2.Dot(throttleDir, headRightDir)) + 0.05f) * (1.0f + uTurnMultiplier);                
                float oscillateMultiplier = (Vector2.Dot(throttleDir, headForwardDir) * 0.5f + 0.4f);

                float finalTorque = oscillateTorque * oscillateMultiplier + turningTorque * turningMultiplier * bendMultiplier;

                //hingeJointsArray[i - 1].useMotor = true;

                JointMotor2D motor = hingeJointsArray[i-1].motor;           
                motor.motorSpeed = 0f;
                motor.maxMotorTorque = restingJointTorque;

                hingeJointsArray[i-1].motor = motor;
            }*/
        }
    }

    public void GainExperience(float exp) {
        totalExperience += exp;
        if(totalExperience >= experienceForNextLevel) {
            GainLevel();
        }
    }
    public void GainLevel() {
        curLevel++;
        experienceForNextLevel = experienceForNextLevel * 2f;
    }

    public void InitializeModules(AgentGenome genome) {

        communicationModule = new CritterModuleCommunication();
        communicationModule.Initialize(genome.bodyGenome.communicationGenome, this);

        coreModule = new CritterModuleCore();
        coreModule.Initialize(genome.bodyGenome.coreGenome, this);

        mouthRef.Initialize(genome.bodyGenome.coreGenome, this);

        environmentModule = new CritterModuleEnvironment();
        environmentModule.Initialize(genome.bodyGenome.environmentalGenome, this);

        foodModule = new CritterModuleFood();
        foodModule.Initialize(genome.bodyGenome.foodGenome, this);

        friendModule = new CritterModuleFriends();
        friendModule.Initialize(genome.bodyGenome.friendGenome, this);

        movementModule = new CritterModuleMovement();
        movementModule.Initialize(genome, genome.bodyGenome.movementGenome);

        threatsModule = new CritterModuleThreats();
        threatsModule.Initialize(genome.bodyGenome.threatGenome, this);
    }
    
    public void FirstTimeInitialize(SettingsManager settings) {  //AgentGenome genome) {  // ** See if I can get away with init sans Genome
        this.settingsRef = settings;
        curLifeStage = AgentLifeStage.AwaitingRespawn;
        //InitializeAgentWidths(genome);
        InitializeGameObjectsAndComponents();
        //InitializeModules(genome);  //  This breaks MapGridCell update, because coreModule doesn't exist?
    }
    private void InitializeGameObjectsAndComponents() {
        // Create Physics GameObject:
        if(bodyGO == null) {
            GameObject bodySegmentGO = new GameObject("RootSegment" + index.ToString());
            bodySegmentGO.transform.parent = this.gameObject.transform;            
            bodySegmentGO.tag = "LiveAnimal";
            bodyGO = bodySegmentGO;
            //bodyCritterSegment = bodySegmentGO.AddComponent<CritterSegment>();
            bodyRigidbody = bodySegmentGO.AddComponent<Rigidbody2D>();
            colliderBody = bodyGO.AddComponent<CapsuleCollider2D>();            
            //bodyCritterSegment.segmentCollider = colliderBody;
            bodyRigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            springJoint = bodyGO.AddComponent<SpringJoint2D>();
            springJoint.enabled = false;
            springJoint.autoConfigureDistance = false;
            springJoint.distance = 0f;
            springJoint.dampingRatio = 0.1f;
            springJoint.frequency = 15f;

            GameObject testMouthGO = new GameObject("Mouth");
            testMouthGO.transform.parent = bodyGO.transform;
            testMouthGO.transform.localPosition = Vector3.zero;
            mouthRef = testMouthGO.AddComponent<CritterMouthComponent>();
            CircleCollider2D mouthTrigger = testMouthGO.AddComponent<CircleCollider2D>();
            mouthRef.triggerCollider = mouthTrigger;

            GameObject mouseClickColliderGO = new GameObject("MouseClickCollider");
            mouseClickColliderGO.transform.parent = bodyGO.transform;
            mouseClickColliderGO.transform.localPosition = new Vector3(0f, 0f, 1f);
            mouseClickCollider = mouseClickColliderGO.AddComponent<CapsuleCollider>();
            mouseClickCollider.isTrigger = true;
        }
    }
    
    // Colliders Footprint???  *************************************************************************************************************

    /*public void InitializeAgentWidths(AgentGenome genome) {   // This is no longer needed -- going point-by-point instead???
        // Calculate Widths, total volume, center of mass, etc:
        // REFACTOR!!! ******
        this.fullSizeBoundingBox = new Vector3(genome.bodyGenome.coreGenome.fullBodyWidth, genome.bodyGenome.coreGenome.fullBodyLength, genome.bodyGenome.coreGenome.fullBodyWidth);

        // Calculate body regions lengthwise:
        float totalRelativeLength = genome.bodyGenome.coreGenome.relLengthSnout + genome.bodyGenome.coreGenome.relLengthHead + genome.bodyGenome.coreGenome.relLengthTorso + genome.bodyGenome.coreGenome.relLengthTail;
        float normalizedSnoutLength = genome.bodyGenome.coreGenome.relLengthSnout / totalRelativeLength;
        float normalizedHeadLength = genome.bodyGenome.coreGenome.relLengthHead / totalRelativeLength;
        float normalizedTorsoLength = genome.bodyGenome.coreGenome.relLengthTorso / totalRelativeLength;
        float normalizedTailLength = genome.bodyGenome.coreGenome.relLengthTail / totalRelativeLength;

        // Calculate body Widths:
        //float totalRelativeWidth = genome.bodyGenome.coreGenome.relWidthMouth + genome.bodyGenome.coreGenome.relWidthHead + genome.bodyGenome.coreGenome.relWidthTorso + genome.bodyGenome.coreGenome.relWidthTail;
        float maxRelWidth = Mathf.Max(genome.bodyGenome.coreGenome.relWidthHead, genome.bodyGenome.coreGenome.relWidthTorso);        
        float normalizedHeadWidth = genome.bodyGenome.coreGenome.relWidthHead / maxRelWidth;
        float normalizedTorsoWidth = genome.bodyGenome.coreGenome.relWidthTorso / maxRelWidth;

        float normalizedSnoutWidth = genome.bodyGenome.coreGenome.relWidthSnout * normalizedHeadWidth;
        float normalizedTailWidth = genome.bodyGenome.coreGenome.relWidthTail * normalizedTorsoWidth;
        
        // Calculate Width:
        //int numWidthSamples = 16; // resolution to sample at
        //float sampleIncrementSize = 1f / (float)widthsTexResolution;
        float totalWidth = 0f;
        for(int j = 0; j < widthsTexResolution; j++) {
            float yCoord = j / (float)widthsTexResolution;

            float sampledWidth = Mathf.Lerp(normalizedSnoutWidth, normalizedHeadWidth, genome.bodyGenome.coreGenome.snoutTaper * (yCoord / normalizedSnoutLength));
            if(yCoord > normalizedSnoutLength) {
                //sampledWidth = normalizedHeadWidth;
                sampledWidth = Mathf.Lerp(normalizedHeadWidth, normalizedTorsoWidth, (yCoord - normalizedSnoutLength) / normalizedHeadLength);
            }
            if(yCoord > normalizedSnoutLength + normalizedHeadLength) {
                sampledWidth = normalizedTorsoWidth;
            }
            if(yCoord > normalizedSnoutLength + normalizedHeadLength + normalizedTorsoLength) {
                //sampledWidth = normalizedTailWidth;
                sampledWidth = Mathf.Lerp(normalizedTailWidth, normalizedTorsoWidth, genome.bodyGenome.coreGenome.tailTaper * ((1f - yCoord) / normalizedTailLength));
            }
            // get absolute from relative value:
            float circleWidth = Mathf.Sqrt(1f - (yCoord * 2f - 1f) * (yCoord * 2f - 1f));
            sampledWidth = Mathf.Min(sampledWidth, circleWidth) * genome.bodyGenome.coreGenome.fullBodyWidth;
                       

            totalWidth += sampledWidth;

            agentWidthsArray[j] = Mathf.Lerp(1f, sampledWidth, 0.5f); // sampledWidth;  *** 1f temporarily to test ***
        }
        float avgSegmentWidth = totalWidth / (float)widthsTexResolution;

        fullSizeBodyVolume = genome.bodyGenome.coreGenome.fullBodyWidth * genome.bodyGenome.coreGenome.fullBodyLength; ////avgSegmentWidth * genome.bodyGenome.coreGenome.fullBodyLength;
        averageFullSizeWidth = avgSegmentWidth;       
    }*/
    
    public void ReconstructAgentGameObjects(SettingsManager settings, AgentGenome genome, EggSack parentEggSack, Vector3 startPos, bool isImmaculate) {
        float corpseLerp = (float)settings.curTierFoodCorpse / 10f;
        //decayDurationTimeSteps = 480; // Mathf.RoundToInt(Mathf.Lerp(360f, 3600f, corpseLerp));
        float eggLerp = (float)settings.curTierFoodEgg / 10f;
        //gestationDurationTimeSteps = Mathf.RoundToInt(Mathf.Lerp(360f, 1080f, eggLerp));
        //pregnancyRefactoryDuration = Mathf.RoundToInt(Mathf.Lerp(3600f, 800f, eggLerp));

        //InitializeAgentWidths(genome);
        InitializeGameObjectsAndComponents();  // Not needed??? ***

        genome.bodyGenome.CalculateFullsizeBoundingBox();
        //Debug.Log("fullSize = " + genome.bodyGenome.fullsizeBoundingBox.ToString() + ", head: " + genome.bodyGenome.coreGenome.headLength.ToString());
        fullSizeBoundingBox = genome.bodyGenome.fullsizeBoundingBox; // genome.bodyGenome.GetFullsizeBoundingBox();
        fullSizeBodyVolume = (fullSizeBoundingBox.x + fullSizeBoundingBox.z) * 0.5f * fullSizeBoundingBox.y; // * fullSizeBoundingBox.z;

        sizePercentage = 0.005f;

        // Positioning and Pinning to parentEggSack HERE:
        bodyGO.transform.position = startPos;

        springJoint.distance = 0.005f;
        springJoint.enableCollision = false;        
        springJoint.frequency = 15f;

        if(isImmaculate) {            
            springJoint.connectedBody = null; // parentEggSack.rigidbodyRef;
            springJoint.enabled = false;
            isAttachedToParentEggSack = false;

            colliderBody.enabled = true;
        }
        else {
            //bodyGO.transform.localPosition = parentEggSack.gameObject.transform.position; // startPos.startPosition;        
            springJoint.connectedBody = parentEggSack.rigidbodyRef;
            springJoint.enabled = true;
            isAttachedToParentEggSack = true;

            colliderBody.enabled = false;
        }                
            
        bodyRigidbody.mass = 0.01f; // min mass
        bodyRigidbody.drag = 13.75f; // bodyDrag;
        bodyRigidbody.angularDrag = 15.5f;
        
        // Collision!
        colliderBody.direction = CapsuleDirection2D.Vertical;
        colliderBody.size = new Vector2(fullSizeBoundingBox.x, fullSizeBoundingBox.y) * sizePercentage;  // spawn size percentage 1/10th  

        // Mouth Trigger:
        //mouthRef.isPassive = genome.bodyGenome.coreGenome.isPassive;
        mouthRef.triggerCollider.isTrigger = true;
        mouthRef.triggerCollider.radius = fullSizeBoundingBox.x / 2f * sizePercentage;
        mouthRef.triggerCollider.offset = new Vector2(0f, fullSizeBoundingBox.y / 2f * sizePercentage);
        mouthRef.isFeeding = false;
        mouthRef.feedingFrameCounter = 0;
        mouthRef.isAttacking = false;
        mouthRef.attackingFrameCounter = 0;
        mouthRef.isCooldown = false;
        mouthRef.agentIndex = this.index;
        mouthRef.agentRef = this;

        //mouthRef.Disable();

        //mouseclickcollider MCC
        mouseClickCollider.direction = 1; // Y-Axis ???
        mouseClickCollider.center = Vector3.zero; // new Vector3(0f, -0.65f, 0f); //Vector3.zero; // new Vector3(0f, 0f, 1f);
        mouseClickCollider.radius = fullSizeBoundingBox.x / 2f * sizePercentage;
        mouseClickCollider.radius *= 1.85f; // ** TEMP
        mouseClickCollider.height = fullSizeBoundingBox.y / 2f * sizePercentage;
    }

    private void ResetStartingValues() {
        animationCycle = 0f;
        lifeStageTransitionTimeStepCounter = 0;
        pregnancyRefactoryTimeStepCounter = 0;
        ageCounter = 0;
        sizePercentage = 0f;
        consumptionScore = 0f;
        damageDealtScore = 0f;
        damageReceivedScore = 0f;
        energyEfficiencyScore = 0f;
        reproductionScore = 0f;
        supportScore = 0f;
        masterFitnessScore = 0f;
        totalExperience = 0f;
        experienceForNextLevel = 2f; // 2, 4, 8, 16, 32, 64, 128, 256?
        curLevel = 0;
        //scoreCounter = 0;
        //totalFoodEatenDecay = 0f;
        totalFoodEatenPlant = 0f;
        totalFoodEatenMeat = 0f;
        totalDamageDealt = 0f;
        totalDamageTaken = 0f;
        turningAmount = 5f; // temporary for zygote animation
        facingDirection = new Vector2(0f, 1f);
        throttle = Vector2.zero;
        smoothedThrottle = new Vector2(0f, 0.01f); 
    }

    //  When should biomass be transferred from EggSack?
    // If spawnImmaculate, where does the biomass come from? -- should it be free?
    //  

    public void InitializeSpawnAgentImmaculate(SettingsManager settings, int agentIndex, CandidateAgentData candidateData, Vector3 spawnWorldPos) {        
        index = agentIndex;
        speciesIndex = candidateData.speciesID;
        candidateRef = candidateData;
        AgentGenome genome = candidateRef.candidateGenome;        
        
        curLifeStage = AgentLifeStage.Egg;
        parentEggSackRef = null;

        // **** Separate out this code into shared function to avoid duplicate code::::
        ResetStartingValues();
        
        InitializeModules(genome);      // Modules need to be created first so that Brain can map its neurons to existing modules  
        
        // Upgrade this to proper Pooling!!!!
        ReconstructAgentGameObjects(settings, genome, null, spawnWorldPos, true);

        brain = new Brain(genome.brainGenome, this); 
        
        isInert = false;
    }
    public void InitializeSpawnAgentFromEggSack(SettingsManager settings, int agentIndex, CandidateAgentData candidateData, EggSack parentEggSack) {        
        index = agentIndex;
        speciesIndex = candidateData.speciesID;
        candidateRef = candidateData;
        AgentGenome genome = candidateRef.candidateGenome;
                
        curLifeStage = AgentLifeStage.Egg;
        parentEggSackRef = parentEggSack;
        
        ResetStartingValues();
                
        InitializeModules(genome);      // Modules need to be created first so that Brain can map its neurons to existing modules  

        // Upgrade this to proper Pooling!!!!
        Vector3 spawnOffset = UnityEngine.Random.insideUnitSphere * parentEggSack.curSize.magnitude * 0.167f;
        spawnOffset.z = 0f;
        ReconstructAgentGameObjects(settings, genome, parentEggSack, parentEggSack.gameObject.transform.position + spawnOffset, false);

        brain = new Brain(genome.brainGenome, this);   
        
        isInert = false;
    }

}
