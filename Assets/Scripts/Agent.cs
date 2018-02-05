using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {
   
    public Brain brain;

    public TestModule testModule;
    private Rigidbody2D rigidBody2D;
    //public bool isVisible = false;
    //public float horizontalMovementInput = 0f;
    //public float verticalMovementInput = 0f;
    public float speed = 150f;
    public bool humanControlled = false;

    // Use this for initialization
    void Start() {
        rigidBody2D = GetComponent<Rigidbody2D>();
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

    public void ResetBrainState() {
        brain.ResetBrainState();
    }
    
    public void TickBrain() {
        float startTime = Time.realtimeSinceStartup;
        brain.BrainMasterFunction();
        float endTime = Time.realtimeSinceStartup;
        if(endTime - startTime > 0.1f) {
            Debug.Log("TickBrain " + (endTime - startTime).ToString() + "s");
        }
    }
    public void TickModules() {
        testModule.Tick();
        //rangefinderModule.Tick();
        //enemyModule.Tick();
    }

    public void Tick() {
        // Any external inputs updated by simManager just before this
        TickModules(); // update inputs for Brain
        TickBrain(); // Tick Brain
        TickActions(); // Execute Actions                
    }

    public void TickActions() {
        float horizontalMovementInput = 0f;
        float verticalMovementInput = 0f;

        if (humanControlled) {
            horizontalMovementInput = 0f;
            if (Input.GetKey("left") || Input.GetKey("a")) {
                horizontalMovementInput -= 1f;
            }
            if (Input.GetKey("right") || Input.GetKey("d")) {
                horizontalMovementInput += 1f;
            }
            verticalMovementInput = 0f;
            if (Input.GetKey("up") || Input.GetKey("w")) {
                verticalMovementInput += 1f;
            }
            if (Input.GetKey("down") || Input.GetKey("s")) {
                verticalMovementInput -= 1f;
            }
        }
        else {
            horizontalMovementInput = Mathf.Round(testModule.throttleX[0] * 3f / 2f);
            verticalMovementInput = Mathf.Round(testModule.throttleY[0] * 3f / 2f);
        }

        // MOVEMENT HERE:
        this.GetComponent<Rigidbody2D>().AddForce(new Vector2(speed * horizontalMovementInput * Time.deltaTime, 0f), ForceMode2D.Impulse);
        this.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, speed * verticalMovementInput * Time.deltaTime), ForceMode2D.Impulse);
    }
    
    public void InitializeModules(AgentGenome genome, Agent agent, StartPositionGenome startPos) {
        testModule = new TestModule();
        testModule.Initialize(genome.bodyGenome.testModuleGenome, agent, startPos);        
    }

    public void InitializeAgentFromGenome(AgentGenome genome, StartPositionGenome startPos) {
        this.transform.localPosition = startPos.agentStartPosition;
        InitializeModules(genome, this, startPos);      // Modules need to be created first so that Brain can map its neurons to existing modules  
        brain = new Brain(genome.brainGenome, this);
    }
}
