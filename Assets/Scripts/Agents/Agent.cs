﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {

    public float frequency = 4.5f;
    public float amplitude = 0f;
    public float offsetDelay = 1.3f;
    public float headDrag = 8f;
    public float bodyDrag = 8f;
    public float headMass = 1f;
    public float bodyMass = 1f;
    public float speed = 500f;
    public float jointSpeed = 100f;
    public float jointMaxTorque = 250f;
    public float swimAnimationCycleSpeed = 0.025f;
    public float smoothedThrottleLerp = 0.65f;
    public float restingJointTorque = 10f;
    public float bendRatioHead = 0f;
    public float bendRatioTailTip = 1f;

    public float totalFoodEaten = 0f;

    public float animationCycle = 0f;
    public float turningAmount = 0f;

    public float spawnStartingScale = 0.25f;

    public int index;
    
    public AgentLifeStage curLifeStage;
    public enum AgentLifeStage {
        AwaitingRespawn,
        Egg,
        Young,
        Mature,
        Dead,
        Null
    }
    private int gestationDurationTimeSteps = 180;
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
    private int youngDurationTimeSteps = 600;
    public int _YoungDurationTimeSteps
    {
        get
        {
            return youngDurationTimeSteps;
        }
        set
        {

        }
    }
    public int maxAgeTimeSteps = 6400;
    private int decayDurationTimeSteps = 400;
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

    private int growthScalingSkipFrames = 12;

    public float growthPercentage = 0f;
   
    public Brain brain;
    public GameObject bodyGO;
    public Rigidbody2D bodyRigidbody;
    //public CritterSegment bodyCritterSegment;

        // MODULES:::
    public CritterModuleCore coreModule;
    public CritterModuleMovement movementModule;
    public CritterMouthComponent mouthRef;

    public CapsuleCollider2D colliderBody;
    public SpringJoint2D springJoint;
    public CapsuleCollider mouseClickCollider;
    
    public Vector3 fullSizeBoundingBox;
    public float averageFullSizeWidth = 1f;  // used to determine size of collider
    public float fullSizeBodyVolume = 1f;
    public float centerOfMass = 0f;
    
    //public bool isInsideFood = false;
        
    //public bool humanControlled = false;
    //public float humanControlLerp = 0f;
    //public bool isNull = false;

    //public bool wasImpaled = false;
        
    public Texture2D textureHealth;
    private int widthsTexResolution = 16;
    public float[] agentWidthsArray;

    public int ageCounterMature = 0; // only counts when agent is an adult
    public int lifeStageTransitionTimeStepCounter = 0; // keeps track of how long agent has been in its current lifeStage
    public int scoreCounter = 0;

    public float corpseFoodAmount = 1f;

    private Vector3 prevPos;
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

    public Vector2 throttle;
    public Vector2 smoothedThrottle;
    public Vector2 facingDirection;  // based on throttle history

    public float avgVel;
    public float avgFluidVel;

    public int speciesIndex = -1;

    public bool isSwallowingPrey = false;
    public bool isBeingSwallowed = false;
    public int beingSwallowedFrameCounter = 0;
    public int swallowingPreyFrameCounter = 0;
    public int swallowDuration = 60;
    public Agent predatorAgentRef;
    public Agent preyAgentRef;

    public EggSack parentEggSackRef;  // instead of using own fixed embry development duration - look up parentEggSack and use its counter?
    
    
    // Use this for initialization
    private void Awake() {
        //size = new Vector2(1f, 1f); // Better way to handle this! ****

        // temp fix for delayed spawning of Agents (leading to nullReferenceExceptions)
        agentWidthsArray = new float[widthsTexResolution];
    }
    void Start() { // *** MOVE THIS TO BETTER SPOT! ***
                    
    }

    public void SetToAwaitingRespawn() {
        curLifeStage = AgentLifeStage.AwaitingRespawn;
    }

    public void InitiateBeingSwallowed(Agent predatorAgent)
    {

        isBeingSwallowed = true;
        beingSwallowedFrameCounter = 0;
        predatorAgentRef = predatorAgent;
    }
    public void InitiateSwallowingPrey(Agent preyAgent)
    {
        isSwallowingPrey = true;
        swallowingPreyFrameCounter = 0;
        preyAgentRef = preyAgent;
    }

    private int GetNumInputs() {
        return 44;  // make more robust later!
    } // *** UPGRADE!!!!
    private int GetNumOutputs() {
        return 7;  // make more robust later!
    } // *** UPGRADE!!!!
    public DataSample RecordData() {
        DataSample sample = new DataSample(GetNumInputs(), GetNumOutputs());
        /*
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

        */

        return sample;
    }

    public void MapNeuronToModule(NID nid, Neuron neuron) {
        //testModule.MapNeuron(nid, neuron); // OLD

        coreModule.MapNeuron(nid, neuron);
        movementModule.MapNeuron(nid, neuron);

        // Hidden nodes!
        if (nid.moduleID == -1) {
            neuron.currentValue = new float[1];
            neuron.neuronType = NeuronGenome.NeuronType.Hid;
            neuron.previousValue = 0f;
        }
    }
        
    public void ResetBrainState() {
        brain.ResetBrainState();
    }
    
    public void TickBrain() {
        brain.BrainMasterFunction();
    }
    public void TickModules(SimulationManager simManager, Vector4 nutrientCellInfo) { // Updates internal state of body - i.e health, energy etc. -- updates input Neuron values!!!
                                // Update Stocks & Flows ::: new health, energy, stamina
                                // This should have happened during last frame's Internal PhysX Update

        // HOWEVER, some updates only happen once per frame and can be handled here, like converting food into energy automatically

        // Turns out that most of these updates are updating input neurons which tend to be sensors
        // These values are sometimes raw attributes & sometimes processed data
        // Should I break them up into individual sensor types -- like Ears, Collider Rangefind, etc.?
        // Separate sensors for each target type or add multiple data types to rangefinder raycasts?
        
        //UpdateInternalResources();  // update energy, stamina, food -- or do this during TickActions?

        Vector2 ownPos = new Vector2(bodyRigidbody.transform.localPosition.x, bodyRigidbody.transform.localPosition.y);
        Vector2 ownVel = new Vector2(bodyRigidbody.velocity.x, bodyRigidbody.velocity.y); // change this to ownPos - prevPos *****************

        coreModule.Tick(simManager, nutrientCellInfo, mouthRef.isPassive, ownPos, ownVel, index);
        movementModule.Tick(this, ownVel);
        // Add more sensor Modules later:

        // Update Mouth::::         
        if(mouthRef.isBiting) {
            
            // Already biting
            mouthRef.bitingFrameCounter++;
            
            if(mouthRef.bitingFrameCounter >= mouthRef.biteHalfCycleDuration * 2 + mouthRef.biteCooldownDuration) {
                mouthRef.bitingFrameCounter = 0;
                mouthRef.isBiting = false;
            }
        }
        else
        {
            mouthRef.bitingFrameCounter = 0;
        }
    }

    private void UpdateInternalResources() {
        // Convert Food to Energy if there is some in stomach
        // convert energy to stamina

        // Each module can send back information about energy usage? Or store it as a value from last frame
    }

    private void CheckForDeathStarvation() {
        // STARVATION::
        if (coreModule.energyRaw <= 0f) {
            curLifeStage = AgentLifeStage.Dead;
            lifeStageTransitionTimeStepCounter = 0;

            InitializeCorpseAsFood();
        }
    }
    private void CheckForDeathHealth() {
        // HEALTH FAILURE:
        if (coreModule.healthHead <= 0f) {

            curLifeStage = Agent.AgentLifeStage.Dead;
            lifeStageTransitionTimeStepCounter = 0;
            
            coreModule.healthHead = 0f;
            coreModule.healthBody = 0f;
            coreModule.healthExternal = 0f;

            InitializeCorpseAsFood();
        }
        if (coreModule.healthBody <= 0f) {

            curLifeStage = Agent.AgentLifeStage.Dead;
            lifeStageTransitionTimeStepCounter = 0;
            
            coreModule.healthHead = 0f;
            coreModule.healthBody = 0f;
            coreModule.healthExternal = 0f;

            InitializeCorpseAsFood();
        }
    }
    private void CheckForDeathOldAge() {
        if(ageCounterMature > maxAgeTimeSteps) {
            curLifeStage = Agent.AgentLifeStage.Dead;
            lifeStageTransitionTimeStepCounter = 0;

            //Debug.Log("Died of old age!");
            InitializeCorpseAsFood();
        }
    }
    private void InitializeCorpseAsFood()
    {
        corpseFoodAmount = coreModule.currentBodySize.x * coreModule.currentBodySize.y;
        //Debug.Log("new corpse food! " + corpseFoodAmount.ToString());

        if(isBeingSwallowed)
        {
            predatorAgentRef.springJoint.enabled = false;
            predatorAgentRef.springJoint.connectedBody = null;

            colliderBody.enabled = true;
        }
    }
    private void CheckForLifeStageTransition() {
        switch(curLifeStage) {
            case AgentLifeStage.AwaitingRespawn:
                //
                break;
            case AgentLifeStage.Egg:
                //
                if(lifeStageTransitionTimeStepCounter >= gestationDurationTimeSteps) {
                    curLifeStage = AgentLifeStage.Young;
                    //Debug.Log("EGG HATCHED!");
                    lifeStageTransitionTimeStepCounter = 0;
                    growthPercentage = 0f;
                }
                break;
            case AgentLifeStage.Young:
                //
                if(lifeStageTransitionTimeStepCounter >= youngDurationTimeSteps) {
                    curLifeStage = AgentLifeStage.Mature;
                    //Debug.Log("EGG HATCHED!");
                    lifeStageTransitionTimeStepCounter = 0;

                    growthPercentage = 1f; // FULLY GROWN!!!
                    ScaleBody(growthPercentage);
                }

                CheckForDeathStarvation();
                CheckForDeathHealth();
                CheckForDeathOldAge();
                break;
            case AgentLifeStage.Mature:
                
                // Check for Death:
                CheckForDeathStarvation();
                CheckForDeathHealth();
                CheckForDeathOldAge();
                
                break;
            case AgentLifeStage.Dead:
                //
                if(lifeStageTransitionTimeStepCounter >= decayDurationTimeSteps) {
                    curLifeStage = AgentLifeStage.Null;
                    //Debug.Log("AGENT NO LONGER EXISTS!");
                    lifeStageTransitionTimeStepCounter = 0;
                }
                break;
            case AgentLifeStage.Null:
                
                beingSwallowedFrameCounter = 0;
                isBeingSwallowed = false;

                colliderBody.enabled = true;

                springJoint.enabled = false;
                springJoint.connectedBody = null;
                              
                break;
            default:
                Debug.LogError("NO SUCH ENUM ENTRY IMPLEMENTED, YOU FOOL!!! (" + curLifeStage.ToString() + ")");
                break;
        }
    }

    public void EatFood(float amount) {
        coreModule.stomachContents += amount;
        if(coreModule.stomachContents > coreModule.stomachCapacity) {
            coreModule.stomachContents = coreModule.stomachCapacity;
        }

        totalFoodEaten += amount;        
    }
    public void ProcessBeingEaten(float amount) {
        // if this agent is dead, it acts as food.
        // it was just bitten by another creature and removed material -- 

        corpseFoodAmount -= amount;
        if (corpseFoodAmount < 0f)
        {
            corpseFoodAmount = 0f;

            // fully consumed?? Should this case be checked for earlier in the pipe ???
            // Need to 
        }
        else
        {
            
            float sidesRatio = coreModule.coreWidth / coreModule.coreLength;
            float sideY = Mathf.Sqrt(corpseFoodAmount / sidesRatio);
            float sideX = sideY * sidesRatio;

            // v v v move this into ScaleBody function?  Or re-organize into sub functions?
            // Do I even use currentBodySize as a trusted value?
            coreModule.currentBodySize = new Vector2(sideX, sideY);
            colliderBody.size = coreModule.currentBodySize;

            // MOUTH:  // probably look to remove this in the future? mouth should be disabled anyway....
            mouthRef.triggerCollider.radius = coreModule.currentBodySize.x * 0.5f;
            mouthRef.triggerCollider.offset = new Vector2(0f, coreModule.currentBodySize.y * 0.5f);
            
        }
    }

    public void Tick(SimulationManager simManager, Vector4 nutrientCellInfo, ref Vector4[] eatAmountsArray, SettingsManager settings) {
        
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

                colliderBody.enabled = true; // **** IS THIS RIGHT???????? ***

                predatorAgentRef.springJoint.enabled = false;
                predatorAgentRef.springJoint.connectedBody = null;
            }
            else
            {
                float scale = (float)beingSwallowedFrameCounter / (float)swallowDuration;

                ScaleBody((1.0f - scale) * 0.8f);
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
            case AgentLifeStage.Young:
                TickYoung(simManager, nutrientCellInfo, ref eatAmountsArray, settings);
                break;
            case AgentLifeStage.Mature:
                //
                TickMature(simManager, nutrientCellInfo, ref eatAmountsArray, settings);
                break;
            case AgentLifeStage.Dead:
                //
                TickDead();
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
                

        // DebugDisplay
        /*if(textureHealth != null) {  // **** Will have to move this into general Tick() method and account for death types & lifeCycle transitions!
            if(humanControlled) {
                //Debug.Log(texture.ToString());
                float displayFoodAmount = coreModule.energyRaw; // coreModule.foodAmountR[0];
                //displayFoodAmount = coreModule.foodAmountR[0];
                if(curLifeStage == AgentLifeStage.Egg) {
                    displayFoodAmount = (float)lifeStageTransitionTimeStepCounter / (float)gestationDurationTimeSteps;
                }
                if(curLifeStage == AgentLifeStage.Dead || curLifeStage == AgentLifeStage.Null) {
                    displayFoodAmount = 0f;
                }
                textureHealth.SetPixel(0, 0, new Color(coreModule.hitPoints[0], coreModule.hitPoints[0], coreModule.hitPoints[0]));
                textureHealth.SetPixel(1, 0, new Color(displayFoodAmount, displayFoodAmount, displayFoodAmount));
                textureHealth.SetPixel(2, 0, new Color(displayFoodAmount, displayFoodAmount, displayFoodAmount));
                textureHealth.SetPixel(3, 0, new Color(displayFoodAmount, displayFoodAmount, displayFoodAmount));

                float comm0 = coreModule.outComm0[0] * 0.5f + 0.5f;
                float comm1 = coreModule.outComm1[0] * 0.5f + 0.5f;
                float comm2 = coreModule.outComm2[0] * 0.5f + 0.5f;
                float comm3 = coreModule.outComm3[0] * 0.5f + 0.5f;
                textureHealth.SetPixel(0, 1, new Color(comm0, comm0, comm0));
                textureHealth.SetPixel(1, 1, new Color(comm1, comm1, comm1));
                textureHealth.SetPixel(2, 1, new Color(comm2, comm2, comm2));
                textureHealth.SetPixel(3, 1, new Color(comm3, comm3, comm3));

                textureHealth.Apply();
            }            
        }*/

    }

    // *** Condense these into ONE?
    private void TickEgg() {        
        lifeStageTransitionTimeStepCounter++;
        
        // *** This seems wrong -- Should be handled in a different spot
        if(isBeingSwallowed)
        {
            predatorAgentRef.isSwallowingPrey = false;
            predatorAgentRef.swallowingPreyFrameCounter = 0;
            predatorAgentRef.springJoint.enabled = false;
            predatorAgentRef.springJoint.connectedBody = null;

            colliderBody.enabled = true;

            isBeingSwallowed = false;
            beingSwallowedFrameCounter = 0;

            Debug.Log("HUH????");
        }
        else {
            coreModule.energyStored[0] = 1f;
        }

    }
    private void TickYoung(SimulationManager simManager, Vector4 nutrientCellInfo, ref Vector4[] eatAmountsArray, SettingsManager settings) {
        ProcessSwallowing();

        growthPercentage = (float)lifeStageTransitionTimeStepCounter / (float)youngDurationTimeSteps;

        // Scaling Test:
        int frameNum = lifeStageTransitionTimeStepCounter % growthScalingSkipFrames;
        if(frameNum == 0) {
            ScaleBody(growthPercentage);  
        }

        TickModules(simManager, nutrientCellInfo); // update inputs for Brain        
        TickBrain(); // Tick Brain
        TickActions(simManager, nutrientCellInfo, ref eatAmountsArray, settings); // Execute Actions  -- Also Updates Resources!!! ***
        lifeStageTransitionTimeStepCounter++;
        scoreCounter++;
    }
    private void ProcessSwallowing() {
        if(isSwallowingPrey)
        {
            swallowingPreyFrameCounter++;

            if (swallowingPreyFrameCounter >= swallowDuration)
            {
                //Debug.Log("isSwallowingPrey + swallow Complete!");

                swallowingPreyFrameCounter = 0;
                isSwallowingPrey = false;

                colliderBody.enabled = true;

                springJoint.enabled = false;
                springJoint.connectedBody = null;
            }
            else
            {

                
            }
        }
        else
        {
            swallowingPreyFrameCounter = 0;
            isSwallowingPrey = false;

            colliderBody.enabled = true;

            springJoint.enabled = false;
            springJoint.connectedBody = null;
        }
    }
    private void TickMature(SimulationManager simManager, Vector4 nutrientCellInfo, ref Vector4[] eatAmountsArray, SettingsManager settings) {

        ProcessSwallowing();

        // Check for death & stuff? Or is this handled inside OnCollisionEnter() events?

        // Scaling Test:
        int frameNum = ageCounterMature % growthScalingSkipFrames;
        if(frameNum == 0) {
            ScaleBody(growthPercentage);  
        }

        TickModules(simManager, nutrientCellInfo); // update inputs for Brain        
        TickBrain(); // Tick Brain
        TickActions(simManager, nutrientCellInfo, ref eatAmountsArray, settings); // Execute Actions  -- Also Updates Resources!!! ***
        
        ageCounterMature++;
        scoreCounter++;
    }
    private void TickDead() {
        lifeStageTransitionTimeStepCounter++;

        isSwallowingPrey = false;
        swallowingPreyFrameCounter = 0;
        springJoint.enabled = false;
        springJoint.connectedBody = null;
        
        mouthRef.isBiting = false;        
    }

    private void ScaleBody(float growthPercentage) {
        //segmentFullSizeArray
        float scale = Mathf.Lerp(spawnStartingScale, 1f, growthPercentage); // Minimum size = 0.1 ???
        float currentBodyVolume = fullSizeBodyVolume * scale; // *** REFACTOR!!!! ****

        coreModule.currentBodySize = new Vector2(coreModule.coreWidth, coreModule.coreLength) * scale;

        coreModule.stomachCapacity = currentBodyVolume * 1f;

        coreModule.maxEnergyStorage = fullSizeBodyVolume * scale;

        colliderBody.size = coreModule.currentBodySize;

        bodyRigidbody.mass = currentBodyVolume;

        // MOUTH:
        mouthRef.triggerCollider.radius = coreModule.coreWidth * scale * 0.5f;
        mouthRef.triggerCollider.offset = new Vector2(0f, coreModule.coreLength * scale * 0.5f);
        
        mouseClickCollider.radius = coreModule.coreWidth * scale * 0.5f;        
        mouseClickCollider.height = coreModule.coreLength * scale;
        mouseClickCollider.radius *= 3.6f; // ** TEMP
        mouseClickCollider.height *= 1.4f;        
    }

    public void TickActions(SimulationManager simManager, Vector4 nutrientCellInfo, ref Vector4[] eatAmountsArray, SettingsManager settings) {
       
        float horizontalMovementInput = movementModule.throttleX[0];; // Mathf.Lerp(horAI, horHuman, humanControlLerp);
        float verticalMovementInput = movementModule.throttleY[0]; // Mathf.Lerp(verAI, verHuman, humanControlLerp);
        
        // Facing Direction:
        throttle = new Vector2(horizontalMovementInput, verticalMovementInput);        
        smoothedThrottle = Vector2.Lerp(smoothedThrottle, throttle, smoothedThrottleLerp);
        Vector2 throttleForwardDir = throttle.normalized;
                
        // ENERGY!!!!
        // Digestion:
        float amountDigested = 0.006f * fullSizeBodyVolume;
        float digestionAmount = Mathf.Min(coreModule.stomachContents, amountDigested);
        float foodToEnergyConversion = 1.0f;
        float createdEnergy = digestionAmount * foodToEnergyConversion;
        coreModule.stomachContents -= digestionAmount;
        if(coreModule.stomachContents < 0f) {
            coreModule.stomachContents = 0f;
        }
        coreModule.energyRaw += createdEnergy;
        float maxEnergy = fullSizeBodyVolume;
        if(coreModule.energyRaw > maxEnergy) {
            coreModule.energyRaw = maxEnergy;
        }

        // Heal:
        float healRate = 0.0005f * fullSizeBodyVolume;
        float energyToHealthConversionRate = 10f;
        if(coreModule.healthBody < 1f) {
            coreModule.healthBody += healRate;
            coreModule.healthHead += healRate;
            coreModule.healthExternal += healRate;

            coreModule.energyRaw -= healRate / energyToHealthConversionRate;
        }

        //ENERGY:
        float energyCost = 0.0025f * fullSizeBodyVolume * settings.energyDrainMultiplier;
        
        float throttleMag = smoothedThrottle.magnitude;
        
        // ENERGY DRAIN::::
        coreModule.energyRaw -= energyCost;
        if(coreModule.energyRaw < 0f) {
            coreModule.energyRaw = 0f;
        }

        eatAmountsArray[index].x = 0f;

        if(curLifeStage == AgentLifeStage.Dead || curLifeStage == AgentLifeStage.Egg) {
            throttle = Vector2.zero;
            smoothedThrottle = Vector2.zero;
        }
        else {

            // FOOD PARTICLES: Either mouth type for now:
            float foodParticleEatAmount = simManager.foodParticlesEatAmountsArray[index];
            coreModule.stomachContents += foodParticleEatAmount;
            if(coreModule.stomachContents > coreModule.stomachCapacity) {
                coreModule.stomachContents = coreModule.stomachCapacity;
            }

            totalFoodEaten += foodParticleEatAmount;
            
            // BITE!!!
            if(mouthRef.isPassive) {

                // PAssive filter feeding:
                if(coreModule.mouthEffector[0] > 0f) {
                    
                    float ambientFoodDensity = nutrientCellInfo.x;
                    float mouthArea = mouthRef.triggerCollider.radius * mouthRef.triggerCollider.radius * Mathf.PI;

                    float maxEatRate = mouthArea * 1f * settings.eatRateMultiplier;
                    float sizeValue = (coreModule.coreWidth - 0.1f) / 2.5f; // ** Hardcoded assuming size ranges from 0.1 --> 2.5 !!! ********
                    float efficiency = Mathf.Lerp(settings.minSizeFeedingEfficiency, settings.maxSizeFeedingEfficiency, sizeValue) * ambientFoodDensity;
                    
                    // *** Can double dip !!! BROKEN! **** Check reservoir first to avoid overdrafting!! ******
                    float filteredFoodAmount = Mathf.Min(maxEatRate * efficiency, maxEatRate);
                   
                    // Needs to use Compute shader here to sample the current nutrientMapRT:::: ****
                    eatAmountsArray[index].x = filteredFoodAmount;

                    coreModule.stomachContents += filteredFoodAmount;
                    if(coreModule.stomachContents > coreModule.stomachCapacity) {
                        coreModule.stomachContents = coreModule.stomachCapacity;
                    }
                    
                    totalFoodEaten += filteredFoodAmount;
                }                
            }
            else {
                if(coreModule.mouthEffector[0] > 0f) {                    
                    mouthRef.InitiateActiveBite();                    
                }
            }            
        }
        coreModule.debugFoodValue = nutrientCellInfo.x;

        ApplyPhysicsForces(smoothedThrottle);

        float rotationInRadians = (bodyRigidbody.transform.localRotation.eulerAngles.z + 90f) * Mathf.Deg2Rad;
        facingDirection = new Vector2(Mathf.Cos(rotationInRadians), Mathf.Sin(rotationInRadians));
    }

    private void ApplyPhysicsForces(Vector2 throttle) {
        //MovementNaiveSin(throttle);
        //MovementBasicSteering(throttle);
        //MovementSteeringSwim(throttle);
        //MovementForwardSlide(throttle);
        MovementScalingTest(throttle);
        
        //Debug.Log(jointAnglesArray[0].ToString());

        //this.rigidbodiesArray[0].AddForce(Vector2.one * movementModule.horsepower * Time.deltaTime, ForceMode2D.Impulse);
    }
    
    private void MovementScalingTest(Vector2 throttle) {
        // Save current joint angles:
        //for(int j = 0; j < numSegments - 1; j++) {
        //    jointAnglesArray[j] = hingeJointsArray[j].jointAngle;            
        //}

        float bitingPenalty = 1f;
        /*
        if(mouthRef.isBiting)
        {
            bitingPenalty = 1f;
        }
        if(coreModule.mouthEffector[0] > 0f)
        {
            bitingPenalty = 1f;
        }
        */
        float fatigueMultiplier = Mathf.Clamp01(coreModule.energyRaw * 5f / coreModule.maxEnergyStorage);
        float lowHealthPenalty = Mathf.Clamp01(coreModule.healthBody * 5f) * 0.5f + 0.5f;
        fatigueMultiplier *= lowHealthPenalty;
        //float growthStatus = 

        turningAmount = Mathf.Lerp(turningAmount, this.bodyRigidbody.angularVelocity * Mathf.Deg2Rad * 0.1f, 0.28f);

        animationCycle += smoothedThrottle.magnitude * swimAnimationCycleSpeed / (Mathf.Lerp(fullSizeBoundingBox.y, 1f, 0.75f) * (growthPercentage * 0.5f + 0.5f)) * fatigueMultiplier;

        if (throttle.sqrMagnitude > 0.000001f) {  // Throttle is NOT == ZERO
            
            Vector2 headForwardDir = new Vector2(this.bodyRigidbody.transform.up.x, this.bodyRigidbody.transform.up.y).normalized;
            Vector2 headRightDir =  new Vector2(this.bodyRigidbody.transform.right.x, this.bodyRigidbody.transform.right.y).normalized;
            Vector2 throttleDir = throttle.normalized;

            float turnSharpness = (-Vector2.Dot(throttleDir, headForwardDir) * 0.5f + 0.5f);
            float headTurn = Vector2.Dot(throttleDir, headRightDir) * -1f * turnSharpness;
            float headTurnSign = Mathf.Clamp(Vector2.Dot(throttleDir, headRightDir) * -10000f, -1f, 1f);

            
            float developmentMultiplier = Mathf.Lerp(0.25f, 1f, Mathf.Clamp01(growthPercentage * 2f));
            //turningAmount = Mathf.Lerp(turningAmount, this.bodyRigidbody.angularVelocity * Mathf.Deg2Rad * 0.1f, 0.15f);

            //this.rigidbodiesArray[0].AddForce(headForwardDir * speed * Time.deltaTime, ForceMode2D.Impulse);

            //animationCycle += 0.01f; // swimAnimationCycleSpeed; // * smoothedThrottle.magnitude / (Mathf.Lerp(fullSizeBoundingBox.y, 1f, 0.6f) * (growthPercentage * 0.4f + 0.6f));
            //animationCycle = animationCycle % 1.0f;

            // get size in 0-1 range from minSize to maxSize:
            float sizeValue = Mathf.Clamp01((coreModule.coreWidth - 0.1f) / 2.5f); // ** Hardcoded assuming size ranges from 0.1 --> 2.5 !!! ********
            float swimSpeed = Mathf.Lerp(movementModule.smallestCreatureBaseSpeed, movementModule.largestCreatureBaseSpeed, sizeValue);
            speed = swimSpeed;
            // Forward Slide
            //for(int k = 0; k < numSegments; k++) {
            Vector2 segmentForwardDir = new Vector2(this.bodyRigidbody.transform.up.x, this.bodyRigidbody.transform.up.y).normalized;

            Vector2 forwardThrustDir = Vector2.Lerp(segmentForwardDir, throttleDir, 0.1f).normalized;

            this.bodyRigidbody.AddForce(forwardThrustDir * (1f - turnSharpness * 0.25f) * swimSpeed * this.bodyRigidbody.mass * Time.deltaTime * developmentMultiplier * fatigueMultiplier * bitingPenalty, ForceMode2D.Impulse);
            //}

            // modify turning rate based on body proportions:
            float turnRatePenalty = Mathf.Lerp(0.25f, 1f, 1f - sizeValue);

            // Head turn:
            this.bodyRigidbody.AddTorque(Mathf.Lerp(headTurn, headTurnSign, 0.75f) * turnRatePenalty * movementModule.turnRate * this.bodyRigidbody.mass * this.bodyRigidbody.mass * fatigueMultiplier * bitingPenalty * Time.deltaTime, ForceMode2D.Impulse);
            
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

    public void InitializeModules(AgentGenome genome) {
        
        coreModule = new CritterModuleCore();
        coreModule.Initialize(genome.bodyGenome.coreGenome, this);

        movementModule = new CritterModuleMovement();
        movementModule.Initialize(genome.bodyGenome.movementGenome);
            
    }

    public void FirstTimeInitialize(AgentGenome genome) {
        InitializeAgentWidths(genome);
        InitializeGameObjectsAndComponents();
        InitializeModules(genome);
    }
    private void InitializeGameObjectsAndComponents() {
        // Create Physics GameObject:
        if(bodyGO == null) {
            GameObject bodySegmentGO = new GameObject("RootSegment");
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
            springJoint.frequency = 1f;

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
    public void InitializeAgentWidths(AgentGenome genome) {
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

        fullSizeBodyVolume = avgSegmentWidth * genome.bodyGenome.coreGenome.fullBodyLength;
        averageFullSizeWidth = avgSegmentWidth;       
    }
    
    public void ReconstructAgentGameObjects(AgentGenome genome, EggSack parentEggSack) {

        InitializeAgentWidths(genome);
        InitializeGameObjectsAndComponents();        

        // *** Positioning and Pinning to parentEggSack HERE:
        bodyGO.transform.localPosition = parentEggSack.gameObject.transform.position; // startPos.startPosition;        
        springJoint.connectedBody = parentEggSack.rigidbodyRef;
        springJoint.distance = 0.005f;
        springJoint.enableCollision = false;
        springJoint.enabled = true;
        springJoint.frequency = 3.9f;
              
        bodyRigidbody.drag = 13f; // bodyDrag;
        bodyRigidbody.angularDrag = 15f;
        
        // Collision!
        colliderBody.size = new Vector2(coreModule.coreWidth, coreModule.coreLength) * spawnStartingScale;  // spawn size percentage 1/10th      
        colliderBody.direction = CapsuleDirection2D.Vertical;

        // Mouth Trigger:
        mouthRef.isPassive = genome.bodyGenome.coreGenome.isPassive;
        mouthRef.triggerCollider.isTrigger = true;
        mouthRef.triggerCollider.radius = coreModule.coreWidth / 2f * spawnStartingScale;
        mouthRef.triggerCollider.offset = new Vector2(0f, coreModule.coreLength / 2f * spawnStartingScale);
        mouthRef.isBiting = false;
        mouthRef.bitingFrameCounter = 0;
        mouthRef.agentIndex = this.index;
        mouthRef.agentRef = this;

        //mouseclickcollider MCC
        mouseClickCollider.direction = 1; // Y-Axis ???
        mouseClickCollider.center = Vector3.zero;
        mouseClickCollider.radius = coreModule.coreWidth / 2f * spawnStartingScale;
        mouseClickCollider.radius *= 1.25f; // ** TEMP
        mouseClickCollider.height = coreModule.coreLength / 2f * spawnStartingScale;
    }

    public void InitializeSpawnAgentFromGenome(int agentIndex, AgentGenome genome, EggSack parentEggSack) {        
        index = agentIndex;        
                
        curLifeStage = AgentLifeStage.Egg;
        parentEggSackRef = parentEggSack;
        this.fullSizeBoundingBox = new Vector3(genome.bodyGenome.coreGenome.fullBodyWidth, genome.bodyGenome.coreGenome.fullBodyLength, genome.bodyGenome.coreGenome.fullBodyWidth); // ** REFACTOR ***
        
        animationCycle = 0f;
        lifeStageTransitionTimeStepCounter = 0;
        ageCounterMature = 0;
        growthPercentage = 0f;
        scoreCounter = 0;
        totalFoodEaten = 0f;
        turningAmount = 5f; // temporary for zygote animation
        facingDirection = new Vector2(0f, 1f);
        throttle = Vector2.zero;
        smoothedThrottle = new Vector2(0f, 0.01f); 
        
        InitializeModules(genome);      // Modules need to be created first so that Brain can map its neurons to existing modules  
        
        // Upgrade this to proper Pooling!!!!
        ReconstructAgentGameObjects(genome, parentEggSack);

        brain = new Brain(genome.brainGenome, this);
               
    }


    // OLD!!!!
    /*
    private void MovementNaiveSin(Vector2 throttle) {  // Applies a Sin force to each segment based on its position in the spinal column while moving
        // MOVEMENT HERE:
        //this.rigidbodiesArray[0].AddForce(throttle.normalized * speed * Time.deltaTime, ForceMode2D.Impulse);
        

        if (throttle.sqrMagnitude > 0.01f) {
            //facingDirection = Vector2.Lerp(facingDirection + Vector2.one * 0.025f, throttleForwardDir, 0.25f).normalized;
            animationCycle += 0.01f;

            animationCycle = animationCycle % 1.0f;

            // JOINTS!!!
            //float time = Time.realtimeSinceStartup;        

            for(int i = 1; i < numSegments; i++) {

                float delayValue = (float)(i - 1) / (float)(numSegments - 1) * offsetDelay;
                float targetSpeed = Mathf.Sin((animationCycle + delayValue) * Mathf.PI * 2f * frequency) * amplitude;

                hingeJointsArray[i - 1].useMotor = true;

                JointMotor2D motor = hingeJointsArray[i-1].motor;            
                motor.motorSpeed = targetSpeed;
                motor.maxMotorTorque = 200f;

                hingeJointsArray[i-1].motor = motor;
            }
        }
        else {
            for(int i = 1; i < numSegments; i++) {
                
                JointMotor2D motor = hingeJointsArray[i-1].motor;            
                motor.motorSpeed = 0f;
                motor.maxMotorTorque = 0f;
                hingeJointsArray[i-1].motor = motor;
            }
        }
    }
    private void MovementBasicSteering(Vector2 throttle) {
        
        for(int j = 0; j < numSegments - 1; j++) {
            jointAnglesArray[j] = hingeJointsArray[j].jointAngle;            
        }
        

        if (throttle.sqrMagnitude > 0.01f) {
            

            Vector2 headForwardDir = new Vector2(this.rigidbodiesArray[0].transform.up.x, this.rigidbodiesArray[0].transform.up.y).normalized;
            Vector2 headRightDir =  new Vector2(this.rigidbodiesArray[0].transform.right.x, this.rigidbodiesArray[0].transform.right.y).normalized;
            Vector2 throttleDir = throttle.normalized;

            float turnSign = Mathf.Clamp(Vector2.Dot(throttleDir, headRightDir) * -10000f, -1f, 1f);

            //facingDirection = Vector2.Lerp(facingDirection + Vector2.one * 0.025f, headDir.normalized, 0.25f).normalized;
             
            this.rigidbodiesArray[0].AddForce(headForwardDir * speed * Time.deltaTime, ForceMode2D.Impulse);

            animationCycle += 0.01f;
            animationCycle = animationCycle % 1.0f;

            // JOINTS!!!
            //float time = Time.realtimeSinceStartup;        
            
            for(int i = 1; i < numSegments; i++) {

                float delayValue = (float)(i - 1) / (float)(numSegments - 1) * offsetDelay;
                float targetSpeed = Mathf.Sin((animationCycle + delayValue) * Mathf.PI * 2f * frequency) * amplitude;

                hingeJointsArray[i - 1].useMotor = true;

                JointMotor2D motor = hingeJointsArray[i-1].motor;            
                motor.motorSpeed = 0f;
                motor.maxMotorTorque = 1f;

                hingeJointsArray[i-1].motor = motor;
            }

            JointMotor2D motor0 = hingeJointsArray[0].motor;            
            motor0.motorSpeed = turnSign * 500f;
            motor0.maxMotorTorque = 500f;
            hingeJointsArray[0].motor = motor0;
            
        }
        else {
            for(int i = 1; i < numSegments; i++) {
                
                JointMotor2D motor = hingeJointsArray[i-1].motor;            
                motor.motorSpeed = 0f;
                motor.maxMotorTorque = 1f;
                hingeJointsArray[i-1].motor = motor;
            }
        }
    }
    private void MovementSteeringSwim(Vector2 throttle) {
        // Save current joint angles:
        for(int j = 0; j < numSegments - 1; j++) {
            jointAnglesArray[j] = hingeJointsArray[j].jointAngle;            
        }

        if (throttle.sqrMagnitude > 0.01f) {  // Throttle is NOT == ZERO
            
            Vector2 headForwardDir = new Vector2(this.rigidbodiesArray[0].transform.up.x, this.rigidbodiesArray[0].transform.up.y).normalized;
            Vector2 headRightDir =  new Vector2(this.rigidbodiesArray[0].transform.right.x, this.rigidbodiesArray[0].transform.right.y).normalized;
            Vector2 throttleDir = throttle.normalized;

            float headTurnSign = Mathf.Clamp(Vector2.Dot(throttleDir, headRightDir) * -10000f, -1f, 1f);

            this.rigidbodiesArray[0].AddForce(headForwardDir * speed * Time.deltaTime, ForceMode2D.Impulse);

            animationCycle += swimAnimationCycleSpeed;
            //animationCycle = animationCycle % 1.0f;



            for(int i = 1; i < numSegments; i++) {

                float phaseOffset = (float)(i - 1) / (float)(numSegments - 1) * offsetDelay;
                //float targetSpeed = Mathf.Sin((animationCycle + phaseOffset) * Mathf.PI * 2f * frequency) * amplitude;

                float targetAngle = Mathf.Sin((-animationCycle + phaseOffset) * Mathf.PI * 2f * frequency);

                float targetWaveSpeed = targetAngle - (jointAnglesArray[i - 1] * Mathf.PI / 180f);
                //Debug.Log("Joint[" + i.ToString() + "] DeltaAngle: " + targetWaveSpeed.ToString());

                //float bendMultiplier = Mathf.Lerp(bendRatioHead, bendRatioTailTip, (float)(i - 1) / (float)(numSegments - 1));
                float bendMultiplier = Mathf.Lerp(bendRatioHead, bendRatioTailTip, (float)(i - 1) / (float)(numSegments - 1));

                float targetTurnSpeed = headTurnSign / (float)numSegments * 2f;

                hingeJointsArray[i - 1].useMotor = true;

                JointMotor2D motor = hingeJointsArray[i-1].motor;            
                motor.motorSpeed = (targetWaveSpeed * bendMultiplier + targetTurnSpeed) * jointSpeed;
                motor.maxMotorTorque = jointMaxTorque;

                hingeJointsArray[i-1].motor = motor;
            }

            // Head Joint:
            //JointMotor2D motor0 = hingeJointsArray[0].motor;            
            //motor0.motorSpeed = headTurnSign * jointSpeed;
            //motor0.maxMotorTorque = jointMaxTorque;
            //hingeJointsArray[0].motor = motor0;

            
        }
        else {
            for(int i = 1; i < numSegments; i++) {
                
                JointMotor2D motor = hingeJointsArray[i-1].motor;            
                motor.motorSpeed = 0f;
                motor.maxMotorTorque = restingJointTorque;
                hingeJointsArray[i-1].motor = motor;
            }
        }
    }
    private void MovementForwardSlide(Vector2 throttle) {
        // Save current joint angles:
        for(int j = 0; j < numSegments - 1; j++) {
            jointAnglesArray[j] = hingeJointsArray[j].jointAngle;            
        }

        if (throttle.sqrMagnitude > 0.0001f) {  // Throttle is NOT == ZERO
            
            Vector2 headForwardDir = new Vector2(this.rigidbodiesArray[0].transform.up.x, this.rigidbodiesArray[0].transform.up.y).normalized;
            Vector2 headRightDir =  new Vector2(this.rigidbodiesArray[0].transform.right.x, this.rigidbodiesArray[0].transform.right.y).normalized;
            Vector2 throttleDir = throttle.normalized;

            float headTurnSign = Mathf.Clamp(Vector2.Dot(throttleDir, headRightDir) * -10000f, -1f, 1f);

            //this.rigidbodiesArray[0].AddForce(headForwardDir * speed * Time.deltaTime, ForceMode2D.Impulse);

            animationCycle += swimAnimationCycleSpeed;
            //animationCycle = animationCycle % 1.0f;
            
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

                hingeJointsArray[i - 1].useMotor = true;

                JointMotor2D motor = hingeJointsArray[i-1].motor;           
                motor.motorSpeed = finalTorque * jointSpeed;
                motor.maxMotorTorque = jointMaxTorque * throttle.magnitude;

                hingeJointsArray[i-1].motor = motor;
            }

            // Forward Slide
            for(int k = 0; k < numSegments; k++) {
                Vector2 segmentForwardDir = new Vector2(this.rigidbodiesArray[k].transform.up.x, this.rigidbodiesArray[k].transform.up.y).normalized;
                this.rigidbodiesArray[k].AddForce(segmentForwardDir * speed * Time.deltaTime, ForceMode2D.Impulse);
            }
        }
        else {
            for(int i = 1; i < numSegments; i++) {
                
                JointMotor2D motor = hingeJointsArray[i-1].motor;            
                motor.motorSpeed = 0f;
                motor.maxMotorTorque = restingJointTorque;
                hingeJointsArray[i-1].motor = motor;
            }
        }
    }
    */
        
}
