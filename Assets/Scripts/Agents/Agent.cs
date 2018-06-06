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
    public float swimAnimationCycleSpeed = 0.01f;
    public float smoothedThrottleLerp = 0.25f;
    public float restingJointTorque = 50f;
    public float bendRatioHead = 0f;
    public float bendRatioTailTip = 1f;

    public float animationCycle = 0f;

    public int index;
    
    public AgentLifeStage curLifeStage;
    public enum AgentLifeStage {
        Egg,
        Young,
        Mature,
        Decaying,
        Null
    }
    private int gestationDurationTimeSteps = 60;
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
    private int youngDurationTimeSteps = 360;
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
    private int decayDurationTimeSteps = 2;
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
   
    public Brain brain;
    public CritterSegment[] critterSegmentsArray;
    public GameObject[] segmentsArray;
    public GameObject[] tempRenderObjectsArray;
    public Rigidbody2D[] rigidbodiesArray;
    public HingeJoint2D[] hingeJointsArray;
    public Vector2[] segmentFullSizeArray;
    public float[] jointAnglesArray;

    //public TestModule testModule;
    public CritterModuleCore coreModule;
    public CritterModuleMovement movementModule;

    //private Rigidbody2D rigidBody2D; // ** segments???
        
    public Vector2 fullSize;

    //public float fullBodyLength;
    //public float maxBodyWidth;
    public int numSegments;

    public bool isInsideFood = false;
    private float eatingLoopAnimFrame = 0f;
        
    public bool humanControlled = false;
    public float humanControlLerp = 0f;
    public bool isNull = false;

    public bool wasImpaled = false;
        
    public Texture2D texture;

    public int ageCounterMature = 0; // only counts when agent is an adult
    public int lifeStageTransitionTimeStepCounter = 0; // keeps track of how long agent has been in its current lifeStage
    public int scoreCounter = 0;

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

    public Vector2 throttle;
    public Vector2 smoothedThrottle;
    public Vector2 facingDirection;  // based on throttle history

    public float avgVel;
    public float avgFluidVel;
    
    // Use this for initialization
    private void Awake() {
        //size = new Vector2(1f, 1f); // Better way to handle this! ****
    }
    void Start() { // *** MOVE THIS TO BETTER SPOT! ***
                    
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

    //public void ReplaceBrain(AgentGenome genome) {
    //    brain = new Brain(genome.brainGenome, this);
    //}
    /*public void ResetAgent() {  // for when an agent dies, this resets attributes, moves to new spawnPos, switches Brain etc.aw

    }*/
    public void ResetBrainState() {
        brain.ResetBrainState();
    }
    
    public void TickBrain() {
        brain.BrainMasterFunction();
    }
    public void TickModules() { // Updates internal state of body - i.e health, energy etc. -- updates input Neuron values!!!
                                // Update Stocks & Flows ::: new health, energy, stamina
                                // This should have happened during last frame's Internal PhysX Update

        // HOWEVER, some updates only happen once per frame and can be handled here, like converting food into energy automatically

        // Turns out that most of these updates are updating input neurons which tend to be sensors
        // These values are sometimes raw attributes & sometimes processed data
        // Should I break them up into individual sensor types -- like Ears, Collider Rangefind, etc.?
        // Separate sensors for each target type or add multiple data types to rangefinder raycasts?
        
        //UpdateInternalResources();  // update energy, stamina, food -- or do this during TickActions?

        Vector2 ownPos = new Vector2(rigidbodiesArray[0].transform.localPosition.x, rigidbodiesArray[0].transform.localPosition.y);
        Vector2 ownVel = new Vector2(rigidbodiesArray[0].velocity.x, rigidbodiesArray[0].velocity.y);

        coreModule.Tick(humanControlled, ownPos, ownVel);
        movementModule.Tick(humanControlled, ownVel);
        // Add more sensor Modules later:

        //testModule.Tick(humanControlled); // old
        
        //rangefinderModule.Tick();
        //enemyModule.Tick();
    }

    private void UpdateInternalResources() {
        // Convert Food to Energy if there is some in stomach
        // convert energy to stamina

        // Each module can send back information about energy usage? Or store it as a value from last frame
    }

    private void CheckForDeathStarvation() {
        // STARVATION::
        if (coreModule.energy <= 0f) {
            curLifeStage = AgentLifeStage.Decaying;
        }
    }
    private void CheckForDeathHealth() {
        // HEALTH FAILURE:
        if (coreModule.healthHead <= 0f) {

            curLifeStage = Agent.AgentLifeStage.Decaying;
            lifeStageTransitionTimeStepCounter = 0;
            wasImpaled = true;
        }
        if (coreModule.healthBody <= 0f) {

            curLifeStage = Agent.AgentLifeStage.Decaying;
            lifeStageTransitionTimeStepCounter = 0;
            wasImpaled = true;
        }
    }
    private void CheckForLifeStageTransition() {
        switch(curLifeStage) {
            case AgentLifeStage.Egg:
                //
                if(lifeStageTransitionTimeStepCounter >= gestationDurationTimeSteps) {
                    curLifeStage = AgentLifeStage.Young;
                    //Debug.Log("EGG HATCHED!");
                    lifeStageTransitionTimeStepCounter = 0;
                }
                break;
            case AgentLifeStage.Young:
                //
                if(lifeStageTransitionTimeStepCounter >= youngDurationTimeSteps) {
                    curLifeStage = AgentLifeStage.Mature;
                    //Debug.Log("EGG HATCHED!");
                    lifeStageTransitionTimeStepCounter = 0;

                    ScaleBody(1f);
                }

                CheckForDeathStarvation();
                CheckForDeathHealth();
                break;
            case AgentLifeStage.Mature:
                
                // Check for Death:
                CheckForDeathStarvation();
                CheckForDeathHealth();
                
                break;
            case AgentLifeStage.Decaying:
                //
                if(lifeStageTransitionTimeStepCounter >= decayDurationTimeSteps) {
                    curLifeStage = AgentLifeStage.Null;
                    //Debug.Log("AGENT NO LONGER EXISTS!");
                    lifeStageTransitionTimeStepCounter = 0;
                    isNull = true;  // flagged for respawn
                }
                break;
            case AgentLifeStage.Null:
                //
                if(humanControlled) {
                    // player agent stays null for extended period of time
                }
                else {
                    Debug.Log("agent is null - probably shouldn't have gotten to this point...;");
                }                
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

    public void EatFood(float amount) {
        if(humanControlled) {
            coreModule.foodAmountR[0] += amount * 0.720f;  // 0.33 is too little
            coreModule.foodAmountG[0] += amount * 0.720f;
            coreModule.foodAmountB[0] += amount * 0.720f;
        }
        else {
            coreModule.foodAmountR[0] += amount;
            coreModule.foodAmountG[0] += amount;
            coreModule.foodAmountB[0] += amount;
        }

        if(coreModule.foodAmountR[0] > 1f) {
            coreModule.foodAmountR[0] = 1f;
        }
        if(coreModule.foodAmountG[0] > 1f) {
            coreModule.foodAmountG[0] = 1f;
        }
        if(coreModule.foodAmountB[0] > 1f) {
            coreModule.foodAmountB[0] = 1f;
        }
    }

    public void Tick() {
        // Any external inputs updated by simManager just before this

        // Check for StateChange:
        CheckForLifeStageTransition();
        
        switch(curLifeStage) {
            case AgentLifeStage.Egg:
                //
                TickEgg();
                break;
            case AgentLifeStage.Young:
                TickYoung();
                break;
            case AgentLifeStage.Mature:
                //
                TickMature();
                break;
            case AgentLifeStage.Decaying:
                //
                TickDecaying();
                break;
            case AgentLifeStage.Null:
                //
                //Debug.Log("agent is null - probably shouldn't have gotten to this point...;");
                break;
            default:
                Debug.LogError("NO SUCH ENUM ENTRY IMPLEMENTED, YOU FOOL!!! (" + curLifeStage.ToString() + ")");
                break;
        }
        
        /*if(isInsideFood) {
            rigidBody2D.drag = 20f;
            //Debug.Log("isInsideFood!");
        }
        else {
            rigidBody2D.drag = 10f;
        }*/
        isInsideFood = false;
        
        Vector3 curPos = transform.localPosition;
        avgVel = Mathf.Lerp(avgVel, (curPos - prevPos).magnitude, 0.25f);
        prevPos = curPos;

        //transform.localScale = new Vector3(fullSize.x, fullSize.y, 1f);

        //transform.localRotation = Quaternion.FromToRotation(new Vector3(1f, 0f, 0f), new Vector3(facingDirection.x, facingDirection.y, 0f)); // ***** BREAKS BUILD **** w/ sim fluidRendrs!! ****
        //rigidBody2D.MoveRotation(Quaternion.FromToRotation(new Vector3(1f, 0f, 0f), new Vector3(facingDirection.x, facingDirection.y, 0f)));

        // DebugDisplay
        if(texture != null) {  // **** Will have to move this into general Tick() method and account for death types & lifeCycle transitions!
            if(humanControlled) {
                //Debug.Log(texture.ToString());
                float displayFoodAmount = coreModule.energy; // coreModule.foodAmountR[0];
                //displayFoodAmount = coreModule.foodAmountR[0];
                if(curLifeStage == AgentLifeStage.Egg) {
                    displayFoodAmount = (float)lifeStageTransitionTimeStepCounter / (float)gestationDurationTimeSteps;
                }
                if(curLifeStage == AgentLifeStage.Decaying || curLifeStage == AgentLifeStage.Null) {
                    displayFoodAmount = 0f;
                }
                texture.SetPixel(0, 0, new Color(coreModule.hitPoints[0], coreModule.hitPoints[0], coreModule.hitPoints[0]));
                texture.SetPixel(1, 0, new Color(displayFoodAmount, displayFoodAmount, displayFoodAmount));
                texture.SetPixel(2, 0, new Color(displayFoodAmount, displayFoodAmount, displayFoodAmount));
                texture.SetPixel(3, 0, new Color(displayFoodAmount, displayFoodAmount, displayFoodAmount));

                float comm0 = coreModule.outComm0[0] * 0.5f + 0.5f;
                float comm1 = coreModule.outComm1[0] * 0.5f + 0.5f;
                float comm2 = coreModule.outComm2[0] * 0.5f + 0.5f;
                float comm3 = coreModule.outComm3[0] * 0.5f + 0.5f;
                texture.SetPixel(0, 1, new Color(comm0, comm0, comm0));
                texture.SetPixel(1, 1, new Color(comm1, comm1, comm1));
                texture.SetPixel(2, 1, new Color(comm2, comm2, comm2));
                texture.SetPixel(3, 1, new Color(comm3, comm3, comm3));

                texture.Apply();
            }            
        }

    }

    // *** Condense these into ONE?
    private void TickEgg() {        
        lifeStageTransitionTimeStepCounter++;
    }
    private void TickYoung() {

        // Scaling Test:
        int frameNum = lifeStageTransitionTimeStepCounter % growthScalingSkipFrames;
        if(frameNum == 0) {
            float growthLerp = (float)lifeStageTransitionTimeStepCounter / (float)youngDurationTimeSteps;
            ScaleBody(growthLerp);  
        }

        TickModules(); // update inputs for Brain        
        TickBrain(); // Tick Brain
        TickActions(); // Execute Actions  -- Also Updates Resources!!! ***
        lifeStageTransitionTimeStepCounter++;
        scoreCounter++;
    }
    private void TickMature() {
        // Check for death & stuff? Or is this handled inside OnCollisionEnter() events?

        TickModules(); // update inputs for Brain        
        TickBrain(); // Tick Brain
        TickActions(); // Execute Actions  -- Also Updates Resources!!! ***
        
        ageCounterMature++;
        scoreCounter++;
    }
    private void TickDecaying() {
        lifeStageTransitionTimeStepCounter++;
    }

    private void ScaleBody(float growthPercentage) {
        //segmentFullSizeArray
        float scale = Mathf.Lerp(0.1f, 1f, growthPercentage); // Minimum size = 0.1 ???

        for(int i = 0; i < numSegments; i++) {
            
            segmentsArray[i].GetComponent<CapsuleCollider2D>().size = segmentFullSizeArray[i] * scale;

            if(i != 0) {
                segmentsArray[i].GetComponent<HingeJoint2D>().autoConfigureConnectedAnchor = false;
                segmentsArray[i].GetComponent<HingeJoint2D>().anchor = new Vector2(0f, 0.5f) * segmentFullSizeArray[i].y * scale;
                segmentsArray[i].GetComponent<HingeJoint2D>().connectedAnchor = new Vector2(0f, -0.5f) * segmentFullSizeArray[i].y * scale;
            }
            else {
                segmentsArray[i].GetComponent<CircleCollider2D>().radius = segmentFullSizeArray[i].x * scale * 0.5f;
                segmentsArray[i].GetComponent<CircleCollider2D>().offset = new Vector2(0f, segmentFullSizeArray[i].y * scale * 0.5f);
            }

            tempRenderObjectsArray[i].transform.localScale = new Vector3(segmentFullSizeArray[i].x * scale, segmentFullSizeArray[i].y * scale, 1f);


            // MASS ???
            rigidbodiesArray[i].mass = scale;
        }
    }

    public void TickActions() {
        float horizontalMovementInput = 0f;
        float verticalMovementInput = 0f;
        
        float horHuman = 0f;
        float verHuman = 0f;
        if (Input.GetKey("left") || Input.GetKey("a")) {
            horHuman -= 1f;
        }
        if (Input.GetKey("right") || Input.GetKey("d")) {
            horHuman += 1f;
        }
        verticalMovementInput = 0f;
        if (Input.GetKey("up") || Input.GetKey("w")) {
            verHuman += 1f;
        }
        if (Input.GetKey("down") || Input.GetKey("s")) {
            verHuman -= 1f;
        }

        float horAI = movementModule.throttleX[0]; // Mathf.Round(movementModule.throttleX[0] * 3f / 2f);
        float verAI = movementModule.throttleY[0]; // Mathf.Round(movementModule.throttleY[0] * 3f / 2f);

        horizontalMovementInput = Mathf.Lerp(horAI, horHuman, humanControlLerp);
        verticalMovementInput = Mathf.Lerp(verAI, verHuman, humanControlLerp);

        // MOVEMENT HERE:
        //this.rigidbodiesArray[0].AddForce(new Vector2(horizontalMovementInput, verticalMovementInput).normalized * speed * Time.deltaTime, ForceMode2D.Impulse);
        //this.GetComponent<Rigidbody2D>().AddForce(new Vector2(speed * horizontalMovementInput * Time.deltaTime, speed * verticalMovementInput * Time.deltaTime), ForceMode2D.Impulse);
        
        // Facing Direction:
        throttle = new Vector2(horizontalMovementInput, verticalMovementInput);
        smoothedThrottle = Vector2.Lerp(smoothedThrottle, throttle, smoothedThrottleLerp);
        Vector2 throttleForwardDir = throttle.normalized;

        if(curLifeStage == AgentLifeStage.Decaying || curLifeStage == AgentLifeStage.Egg) {
            throttle = Vector2.zero;
            smoothedThrottle = Vector2.zero;
        }

        // ENERGY!!!!
        // Digestion:
        float amountDigested = 0.02f;
        float digestionAmount = Mathf.Min(coreModule.foodAmountR[0], amountDigested);
        float foodToEnergyConversion = 1.0f;
        float createdEnergy = digestionAmount * foodToEnergyConversion;
        coreModule.foodAmountR[0] -= digestionAmount;
        if(coreModule.foodAmountR[0] < 0f) {
            coreModule.foodAmountR[0] = 0f;
        }
        coreModule.energy += createdEnergy;
        if(coreModule.energy > 5f) {
            coreModule.energy = 5f;
        }

        float throttleMag = smoothedThrottle.magnitude;
        if(throttleMag > 0.01f) {
            // ***** UPDATE THIS!!! Right now you can get "free" energy/stamina if current amount is less than the cost
            // Will want to check energy levels before performing Actions!!
            float staminaCost = throttleMag * 0.01f;
            coreModule.stamina[0] -= staminaCost;
            if(coreModule.stamina[0] < 0f) {
                coreModule.stamina[0] = 0f;
            }

            float energyCost = 0.002f; // + 0.001f * throttleMag;  // idle + movement calorie burn
            coreModule.energy -= energyCost;
            if(coreModule.energy < 0f) {
                coreModule.energy = 0f;
            }
        }
        else {
            coreModule.stamina[0] += 0.01f;  // recovery
            if(coreModule.stamina[0] > 1f) {
                coreModule.stamina[0] = 1f;
            }

            float energyCost = 0.002f; // idle calorie burn
            coreModule.energy -= energyCost;
            if(coreModule.energy < 0f) {
                coreModule.energy = 0f;
            }
        }

        ApplyPhysicsForces(smoothedThrottle);

        float rotationInRadians = (rigidbodiesArray[0].transform.localRotation.eulerAngles.z + 90f) * Mathf.Deg2Rad;
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
    private void MovementScalingTest(Vector2 throttle) {
        // Save current joint angles:
        for(int j = 0; j < numSegments - 1; j++) {
            jointAnglesArray[j] = hingeJointsArray[j].jointAngle;            
        }

        if (throttle.sqrMagnitude > 0.0001f) {  // Throttle is NOT == ZERO
            
            Vector2 headForwardDir = new Vector2(this.rigidbodiesArray[0].transform.up.x, this.rigidbodiesArray[0].transform.up.y).normalized;
            Vector2 headRightDir =  new Vector2(this.rigidbodiesArray[0].transform.right.x, this.rigidbodiesArray[0].transform.right.y).normalized;
            Vector2 throttleDir = throttle.normalized;

            float headTurn = Vector2.Dot(throttleDir, headRightDir) * -1f * (-Vector2.Dot(throttleDir, headForwardDir) * 0.5f + 0.5f);
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

                //hingeJointsArray[i - 1].useMotor = true;

                JointMotor2D motor = hingeJointsArray[i-1].motor;           
                motor.motorSpeed = 0f;
                motor.maxMotorTorque = restingJointTorque;

                hingeJointsArray[i-1].motor = motor;
            }

            
            // Forward Slide
            for(int k = 0; k < numSegments; k++) {
                Vector2 segmentForwardDir = new Vector2(this.rigidbodiesArray[k].transform.up.x, this.rigidbodiesArray[k].transform.up.y).normalized;
                this.rigidbodiesArray[k].AddForce(segmentForwardDir * movementModule.horsepower * this.rigidbodiesArray[k].mass * Time.deltaTime, ForceMode2D.Impulse);
            }

            // Head turn:
            this.rigidbodiesArray[0].AddTorque(headTurn * movementModule.turnRate * this.rigidbodiesArray[0].mass * this.rigidbodiesArray[0].mass * Time.deltaTime, ForceMode2D.Impulse);
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

    public void InitializeModules(AgentGenome genome, Agent agent, StartPositionGenome startPos) {
        //testModule = new TestModule();
        //testModule.Initialize(genome.bodyGenome.testModuleGenome, agent, startPos);    

        coreModule = new CritterModuleCore();
        coreModule.Initialize(genome.bodyGenome.coreGenome, agent, startPos);

        movementModule = new CritterModuleMovement();
        movementModule.Initialize(genome.bodyGenome.movementGenome);
    }

    public void ReconstructAgentGameObjects(AgentGenome genome, StartPositionGenome startPos) {

        // Delete existing children GameObjects:
        foreach (Transform child in this.transform) {
             GameObject.Destroy(child.gameObject);
         }

        //fullBodyLength = genome.bodyGenome.coreGenome.coreLength;
        //maxBodyWidth = genome.bodyGenome.coreGenome.coreWidth;

        float widthHead = genome.bodyGenome.coreGenome.relativeWidthHead;
        float widthBody = genome.bodyGenome.coreGenome.relativeWidthBody;
        float widthTail = genome.bodyGenome.coreGenome.relativeWidthTail;
        float headStart = genome.bodyGenome.coreGenome.headStart;
        float tailStart = genome.bodyGenome.coreGenome.tailStart;

        //coreModule.numSegments = genome.bodyGenome.coreGenome.numSegments;

        float segmentLength = genome.bodyGenome.coreGenome.coreLength / (float)numSegments;

        critterSegmentsArray = new CritterSegment[numSegments];
        segmentsArray = new GameObject[numSegments];
        tempRenderObjectsArray = new GameObject[numSegments];
        rigidbodiesArray = new Rigidbody2D[numSegments];
        hingeJointsArray = new HingeJoint2D[Mathf.RoundToInt(Mathf.Max(1, numSegments - 1))];
        segmentFullSizeArray = new Vector2[numSegments];
        jointAnglesArray = new float[hingeJointsArray.Length];

        for (int i = 0; i < numSegments; i++) {

            float segmentStartCoord = (float)i / (float)numSegments;
            float segmentEndCoord = (float)(i + 1) / (float)numSegments;


            // Calculate Width:
            int numWidthSamples = 12; // resolution to sample at
            float totalWidth = 0f;
            for(int j = 0; j < numWidthSamples; j++) {
                float xCoord = Mathf.Lerp(segmentStartCoord, segmentEndCoord, j / (float)(numWidthSamples - 1));

                float sampledWidth = widthHead;
                if(xCoord > headStart) {
                    sampledWidth = widthBody;
                }
                if(xCoord > tailStart) {
                    sampledWidth = widthTail;
                }
                // get absolute from relative value:
                sampledWidth = sampledWidth * genome.bodyGenome.coreGenome.coreWidth;

                totalWidth += sampledWidth;
            }
            float avgSegmentWidth = totalWidth / (float)numWidthSamples;

            // CACHE FULLSIZE DIMENSIONS:
            segmentFullSizeArray[i] = new Vector2(avgSegmentWidth, segmentLength);
            
            // Create Physics GameObject:
            GameObject segmentGO = new GameObject("Segment" + i.ToString());
            segmentGO.transform.parent = this.gameObject.transform;
            segmentGO.transform.localPosition = new Vector3(0f, -i * segmentLength - segmentLength / 2f, 0f) * 0.1f + startPos.startPosition;
            segmentGO.tag = "LiveAnimal";
            segmentsArray[i] = segmentGO;
            critterSegmentsArray[i] = segmentGO.AddComponent<CritterSegment>();
            critterSegmentsArray[i].agentIndex = this.index;
            critterSegmentsArray[i].agentRef = this;
            critterSegmentsArray[i].segmentIndex = i;
            rigidbodiesArray[i] = segmentGO.AddComponent<Rigidbody2D>();
            rigidbodiesArray[i].drag = 12.5f; // bodyDrag;
            rigidbodiesArray[i].angularDrag = 12.5f;
            // Collision!
            // Switch this to smart-aligned Capsules!!
            CapsuleCollider2D collider = segmentGO.AddComponent<CapsuleCollider2D>(); // change this to Capsule Later -- upgrade!!!!
            collider.size = new Vector2(avgSegmentWidth, segmentLength) * 0.1f;
            if(avgSegmentWidth > segmentLength) {
                collider.direction = CapsuleDirection2D.Horizontal;
            }
            else {
                collider.direction = CapsuleDirection2D.Vertical;
                
            }
            
            
            // RENDER OBJECTS:::
            //GameObject segmentGO = new GameObject("Segment" + i.ToString());
            string assetURL = "Prefabs/DebugCritterSegmentRender";
            GameObject renderGO = Instantiate(Resources.Load(assetURL)) as GameObject;
            renderGO.name = "Render" + i.ToString();
            renderGO.transform.parent = segmentGO.transform; // Parent under this Agent GO
            renderGO.transform.localPosition = Vector3.zero;
            renderGO.transform.localScale = new Vector3(avgSegmentWidth, segmentLength, 1f) * 0.1f;
            tempRenderObjectsArray[i] = renderGO;
            //renderGO.SetActive(false);
          
            if(i == 0) {  // ROOT SEGMENT!
                prevPos = segmentGO.transform.localPosition;
                facingDirection = new Vector2(0f, 1f);

                rigidbodiesArray[i].drag = 12.5f; // headDrag; // only on root? // will need to suss out proper balance for this
                rigidbodiesArray[i].mass = 1f; // headMass;

                // Mouth Trigger:
                CircleCollider2D mouthTrigger = segmentsArray[0].AddComponent<CircleCollider2D>();
                mouthTrigger.isTrigger = true;
                mouthTrigger.radius = avgSegmentWidth / 2f * 0.1f;
                mouthTrigger.offset = new Vector2(0f, segmentLength / 2f * 0.1f);

                //segmentsArray[0].AddComponent<CritterMouthComponent>().agentIndex = index;

                coreModule.mouthRef = segmentsArray[0].AddComponent<CritterMouthComponent>();
                coreModule.mouthRef.agentIndex = this.index;
                coreModule.mouthRef.agentRef = this;
            }
            else {
                // Hinge Joint!
                HingeJoint2D hingeJoint = segmentGO.AddComponent<HingeJoint2D>();
                hingeJointsArray[i - 1] = hingeJoint;
                
                hingeJoint.connectedBody = rigidbodiesArray[i - 1];
                hingeJoint.autoConfigureConnectedAnchor = false;
                hingeJoint.anchor = new Vector2(0f, 0.5f) * segmentLength * 0.1f;
                hingeJoint.connectedAnchor = new Vector2(0f, -0.5f) * segmentLength * 0.1f;
                hingeJoint.useMotor = false;

                JointMotor2D motor = hingeJoint.motor;
                motor.maxMotorTorque = 0f;
                hingeJoint.motor = motor;

                hingeJoint.useLimits = true;
                JointAngleLimits2D jointLimits = hingeJoint.limits;
                jointLimits.min = -35f;
                jointLimits.max = 35f;
                hingeJoint.limits = jointLimits;
                
                rigidbodiesArray[i].mass = bodyMass;
            }   
            
            //float segmentArea = avgSegmentWidth * segmentLength;
            //float massProportion = 1f / (float)numSegments * avgSegmentWidth;
            //rigidbodiesArray[i].mass = massProportion * 10f;
        }
        

        //ScaleBody(0f);
    }

    public void InitializeAgentFromGenome(int agentIndex, AgentGenome genome, StartPositionGenome startPos) {
        //sourceGenomeIndex = genomeIndex;

        index = agentIndex;

        numSegments = genome.bodyGenome.coreGenome.numSegments;
        
        curLifeStage = AgentLifeStage.Egg;
        this.fullSize = new Vector2(genome.bodyGenome.coreGenome.coreWidth, genome.bodyGenome.coreGenome.coreLength);
        isNull = false;
        wasImpaled = false;
        lifeStageTransitionTimeStepCounter = 0;
        ageCounterMature = 0;
        scoreCounter = 0;
        //this.transform.localPosition = startPos.startPosition;
        //float semiMajorSize = fullSize.magnitude;
        //this.transform.localScale = new Vector3(semiMajorSize, semiMajorSize, 1f);

        InitializeModules(genome, this, startPos);      // Modules need to be created first so that Brain can map its neurons to existing modules  

        // Create blank Modules here so they can be populated during body construction??

        // Upgrade this to proper Pooling!!!!
        ReconstructAgentGameObjects(genome, startPos);

        brain = new Brain(genome.brainGenome, this);

        facingDirection = new Vector2(0f, 1f);
        throttle = Vector2.zero;
        smoothedThrottle = new Vector2(0f, 0.01f);
        //prevPos = transform.localPosition;
    }
        
}
