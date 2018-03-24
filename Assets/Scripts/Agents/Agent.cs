﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {

    public AgentLifeStage curLifeStage;
    public enum AgentLifeStage {
        Egg,
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
   
    public Brain brain;

    //public MeshRenderer meshRendererBeauty;
    //public MeshRenderer meshRendererFluidCollider;

    public TestModule testModule;
    private Rigidbody2D rigidBody2D;
        
    //public TheRenderKing.PointStrokeData bodyPointStroke;
    //public TheRenderKing.PointStrokeData[] decorationPointStrokesArray;
    public Vector2 fullSize;

    public bool isInsideFood = false;
    private float eatingLoopAnimFrame = 0f;

    //public bool 

    //public int sourceGenomeIndex;
    
    private float speed = 120f;
    public bool humanControlled = false;
    public float humanControlLerp = 0f;
    public bool isNull = false;

    public bool wasImpaled = false;
        
    public Texture2D texture;

    public int ageCounterMature = 0; // only counts when agent is an adult
    public int lifeStageTransitionTimeStepCounter = 0; // keeps track of how long agent has been in its current lifeStage

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

    // Use this for initialization
    private void Awake() {
        //size = new Vector2(1f, 1f); // Better way to handle this! ****
    }
    void Start() { // *** MOVE THIS TO BETTER SPOT! ***
        rigidBody2D = GetComponent<Rigidbody2D>();
        prevPos = transform.localPosition;
        facingDirection = new Vector2(0f, 1f);
             
    }

    private int GetNumInputs() {
        return 44;  // make more robust later!
    }
    private int GetNumOutputs() {
        return 7;  // make more robust later!
    }
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
    }

    public void MapNeuronToModule(NID nid, Neuron neuron) {
        testModule.MapNeuron(nid, neuron);
        // Hidden nodes!
        if (nid.moduleID == -1) {
            neuron.currentValue = new float[1];
            neuron.neuronType = NeuronGenome.NeuronType.Hid;
            neuron.previousValue = 0f;
        }
    }

    public void ReplaceBrain(AgentGenome genome) {
        brain = new Brain(genome.brainGenome, this);
    }
    /*public void ResetAgent() {  // for when an agent dies, this resets attributes, moves to new spawnPos, switches Brain etc.aw

    }*/
    public void ResetBrainState() {
        brain.ResetBrainState();
    }
    
    public void TickBrain() {
        brain.BrainMasterFunction();
    }
    public void TickModules() {
        testModule.Tick();
        //rangefinderModule.Tick();
        //enemyModule.Tick();
    }

    private void CheckForLifeStageTransition() {
        switch(curLifeStage) {
            case AgentLifeStage.Egg:
                //
                if(lifeStageTransitionTimeStepCounter >= gestationDurationTimeSteps) {
                    curLifeStage = AgentLifeStage.Mature;
                    //Debug.Log("EGG HATCHED!");
                    lifeStageTransitionTimeStepCounter = 0;
                }
                break;
            case AgentLifeStage.Mature:
                //
                // Check for Death:
                //float minFood = Mathf.Min(Mathf.Min(testModule.foodAmountR[0], testModule.foodAmountG[0]), testModule.foodAmountB[0]);
                float maxFood = Mathf.Max(Mathf.Max(testModule.foodAmountR[0], testModule.foodAmountG[0]), testModule.foodAmountB[0]);
                if (maxFood <= 0f) {
                    curLifeStage = AgentLifeStage.Decaying;
                    //isNull = true;
                }
                if (testModule.hitPoints[0] <= 0f) {

                    curLifeStage = Agent.AgentLifeStage.Decaying;
                    lifeStageTransitionTimeStepCounter = 0;
                    wasImpaled = true;
                    //agent.isNull = true; // OLD
                    //Debug.Log("Agent DEAD!");
                }
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
                Debug.Log("agent is null - probably shouldn't have gotten to this point...;");
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
        // Any external inputs updated by simManager just before this

        // Check for StateChange:
        CheckForLifeStageTransition();
        
        switch(curLifeStage) {
            case AgentLifeStage.Egg:
                //
                TickEgg();
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
        prevPos = curPos;

        //transform.localScale = new Vector3(fullSize.x, fullSize.y, 1f);
        transform.localRotation = Quaternion.FromToRotation(new Vector3(1f, 0f, 0f), new Vector3(facingDirection.x, facingDirection.y, 0f));
    }

    private void TickEgg() {        
        lifeStageTransitionTimeStepCounter++;
    }
    private void TickMature() {
        TickModules(); // update inputs for Brain        
        TickBrain(); // Tick Brain
        TickActions(); // Execute Actions
        
        // DebugDisplay
        if(texture != null) {
            //Debug.Log(texture.ToString());
            texture.SetPixel(0, 0, new Color(testModule.hitPoints[0], testModule.hitPoints[0], testModule.hitPoints[0]));
            texture.SetPixel(1, 0, new Color(testModule.foodAmountR[0], testModule.foodAmountR[0], testModule.foodAmountR[0]));
            texture.SetPixel(2, 0, new Color(testModule.foodAmountG[0], testModule.foodAmountG[0], testModule.foodAmountG[0]));
            texture.SetPixel(3, 0, new Color(testModule.foodAmountB[0], testModule.foodAmountB[0], testModule.foodAmountB[0]));

            float comm0 = testModule.outComm0[0] * 0.5f + 0.5f;
            float comm1 = testModule.outComm1[0] * 0.5f + 0.5f;
            float comm2 = testModule.outComm2[0] * 0.5f + 0.5f;
            float comm3 = testModule.outComm3[0] * 0.5f + 0.5f;
            texture.SetPixel(0, 1, new Color(comm0, comm0, comm0));
            texture.SetPixel(1, 1, new Color(comm1, comm1, comm1));
            texture.SetPixel(2, 1, new Color(comm2, comm2, comm2));
            texture.SetPixel(3, 1, new Color(comm3, comm3, comm3));

            texture.Apply();
        }

        ageCounterMature++;
    }
    private void TickDecaying() {
        lifeStageTransitionTimeStepCounter++;
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

        float horAI = Mathf.Round(testModule.throttleX[0] * 3f / 2f);
        float verAI = Mathf.Round(testModule.throttleY[0] * 3f / 2f);

        horizontalMovementInput = Mathf.Lerp(horAI, horHuman, humanControlLerp);
        verticalMovementInput = Mathf.Lerp(verAI, verHuman, humanControlLerp);
        
        // MOVEMENT HERE:
        this.GetComponent<Rigidbody2D>().AddForce(new Vector2(speed * horizontalMovementInput * Time.deltaTime, speed * verticalMovementInput * Time.deltaTime), ForceMode2D.Impulse);
        
        // Facing Direction:
        throttle = new Vector2(horizontalMovementInput, verticalMovementInput);
        smoothedThrottle = Vector2.Lerp(smoothedThrottle, throttle, 0.2f);
        Vector2 throttleForwardDir = throttle.normalized;

        if(curLifeStage == AgentLifeStage.Decaying || curLifeStage == AgentLifeStage.Egg) {
            throttle = Vector2.zero;
            smoothedThrottle = Vector2.zero;
        }
        
        if(throttle.sqrMagnitude > 0.01f)
            facingDirection = Vector2.Lerp(facingDirection + Vector2.one * 0.025f, throttleForwardDir, 0.25f).normalized;
    }
    
    public void InitializeModules(AgentGenome genome, Agent agent, StartPositionGenome startPos) {
        testModule = new TestModule();
        testModule.Initialize(genome.bodyGenome.testModuleGenome, agent, startPos);        
    }

    public void InitializeAgentFromGenome(AgentGenome genome, StartPositionGenome startPos) {
        //sourceGenomeIndex = genomeIndex;

        curLifeStage = AgentLifeStage.Egg;
        this.fullSize = new Vector2(genome.bodyGenome.sizeAndAspectRatio.x * genome.bodyGenome.sizeAndAspectRatio.y, genome.bodyGenome.sizeAndAspectRatio.x * (1.0f / genome.bodyGenome.sizeAndAspectRatio.y));
        isNull = false;
        wasImpaled = false;
        lifeStageTransitionTimeStepCounter = 0;
        ageCounterMature = 0;
        this.transform.localPosition = startPos.startPosition;
        this.transform.localScale = new Vector3(fullSize.x, fullSize.y, 1f);
        InitializeModules(genome, this, startPos);      // Modules need to be created first so that Brain can map its neurons to existing modules  
        brain = new Brain(genome.brainGenome, this);
        facingDirection = new Vector2(0f, 1f);
        throttle = Vector2.zero;
        smoothedThrottle = new Vector2(0f, 0.01f);
        prevPos = transform.localPosition;
    }
}
