using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TestModule {
    //public int parentID;
    //public int inno;
    public int parentID;
    public int inno;
    //public bool isVisible;

        // INPUTS
    // Bias
    // Food Sensor  xPos, yPos, xDir, yDir, typeR, typeG, typeB (type includes amplitude?)
    // Friend Sensor   xPos, yPos, xVel, yVel, xDir, yDir
    // Hazard Sensor   xPos, yPos, xVel, yVel, xDir, yDir
    // Self Sensor   ownVelX, ownVelY, Temperature, Pressure
    // Contact Sensor   isContact, contactForce
    // Health    physicalHealth(damage), stamina(energy),  foodR, foodG, foodB
    // 8 cardinal Raycast rangefinders
    // Communication module (4 inputs based on nearest broadcasting object (friend or foe)) -- might only work for organic unsupervised learning, otherwise it's noise
    
        //OUTPUTS:
    // throttleX, throttleY
    // Communication Module (4 channels)
    // Dash?

    //public bool destroyed = false;
    //public float maxHealth;
    //public float prevHealth;
    //public float health;
    public float[] bias;
    //public float[] ownPosX;
    //public float[] ownPosY;
    public float[] foodPosX;
    public float[] foodPosY;
    public float[] foodDirX;
    public float[] foodDirY;
    public float[] foodTypeR;
    public float[] foodTypeG;
    public float[] foodTypeB;

    public float[] friendPosX;
    public float[] friendPosY;
    public float[] friendVelX;
    public float[] friendVelY;
    public float[] friendDirX;
    public float[] friendDirY;

    public float[] enemyPosX;
    public float[] enemyPosY;
    public float[] enemyVelX;
    public float[] enemyVelY;
    public float[] enemyDirX;
    public float[] enemyDirY;

    public float[] ownVelX;
    public float[] ownVelY;
    public float[] temperature;
    public float[] pressure;
    public float[] isContact;
    public float[] contactForceX;
    public float[] contactForceY;
    public float[] hitPoints;
    public float[] stamina;
    public float[] foodAmountR;
    public float[] foodAmountG;
    public float[] foodAmountB;

    public float[] distUp; // start up and go clockwise!
    public float[] distTopRight;
    public float[] distRight;
    public float[] distBottomRight;
    public float[] distDown;    
    public float[] distBottomLeft;
    public float[] distLeft;
    public float[] distTopLeft;

    public float[] inComm0;
    public float[] inComm1;
    public float[] inComm2;
    public float[] inComm3;  // 44 In?

    public float[] throttleX;
    public float[] throttleY;
    public float[] dash;
    public float[] outComm0;
    public float[] outComm1;
    public float[] outComm2;
    public float[] outComm3;  // 7 Out?

    //public float maxSpeed = 0.5f;
    //public float accel = 0.05f;
    //public float radius = 1f;

    //public Transform enemyTransform;
    public Rigidbody2D ownRigidBody2D;
    public FoodModule nearestFoodModule;
    public TestModule friendTestModule;
    public PredatorModule nearestPredatorModule;

    //public HealthModuleComponent component;

    public TestModule() {
        /*healthSensor = new float[1];
        takingDamage = new float[1];
        health = maxHealth;
        prevHealth = health;
        parentID = genome.parentID;
        inno = genome.inno;*/
    }

    public void Initialize(TestModuleGenome genome, Agent agent, StartPositionGenome startPos) {
        ownRigidBody2D = agent.GetComponent<Rigidbody2D>();
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
        // 7 Total Outputs

        //destroyed = false;
        /*bias = new float[1];
        bias[0] = 1f;

        //ownPosX = new float[1];
        //ownPosX[0] = startPos.agentStartPosition.x;
        //ownPosY = new float[1];
        //ownPosY[0] = startPos.agentStartPosition.y;
        ownVelX = new float[1];
        ownVelY = new float[1];

        enemyPosX = new float[1];
        enemyPosY = new float[1];
        enemyVelX = new float[1];
        enemyVelY = new float[1];
        enemyDirX = new float[1];
        enemyDirY = new float[1];

        distLeft = new float[1];
        distRight = new float[1];
        distUp = new float[1];
        distDown = new float[1];

        throttleX = new float[1];
        throttleY = new float[1];
        */

        //maxHealth = genome.maxHealth;
        //health = maxHealth;
        //prevHealth = health;

        bias[0] = 1f;
        foodAmountR[0] = 0f;
        foodAmountG[0] = 0f;
        foodAmountB[0] = 0f;
        hitPoints[0] = 1f;
        stamina[0] = 1f;

        parentID = genome.parentID;
        inno = genome.inno;
        //isVisible = agent.isVisible;

        //component = agent.segmentList[parentID].AddComponent<HealthModuleComponent>();
        //if (component == null) {
        //    Debug.LogAssertion("No existing HealthModuleComponent on segment " + parentID.ToString());
        //}
        //component.healthModule = this;
    }

    public void MapNeuron(NID nid, Neuron neuron) {
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
        if (inno == nid.moduleID) {
            if (nid.neuronID == 0) {
                neuron.currentValue = bias;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 1) {
                neuron.currentValue = foodPosX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 2) {
                neuron.currentValue = foodPosY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 3) {
                neuron.currentValue = foodDirX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 4) {
                neuron.currentValue = foodDirY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 5) {
                neuron.currentValue = foodTypeR;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 6) {
                neuron.currentValue = foodTypeG;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 7) {
                neuron.currentValue = foodTypeB;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 8) {
                neuron.currentValue = friendPosX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 9) {
                neuron.currentValue = friendPosY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 10) {
                neuron.currentValue = friendVelX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 11) {
                neuron.currentValue = friendVelY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 12) {
                neuron.currentValue = friendDirX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 13) {
                neuron.currentValue = friendDirY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 14) {
                neuron.currentValue = enemyPosX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 15) {
                neuron.currentValue = enemyPosY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 16) {
                neuron.currentValue = enemyVelX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 17) {
                neuron.currentValue = enemyVelY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 18) {
                neuron.currentValue = enemyDirX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 19) {
                neuron.currentValue = enemyDirY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 20) {
                neuron.currentValue = ownVelX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 21) {
                neuron.currentValue = ownVelY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 22) {
                neuron.currentValue = temperature;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 23) {
                neuron.currentValue = pressure;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 24) {
                neuron.currentValue = isContact;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 25) {
                neuron.currentValue = contactForceX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 26) {
                neuron.currentValue = contactForceY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 27) {
                neuron.currentValue = hitPoints;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 28) {
                neuron.currentValue = stamina;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 29) {
                neuron.currentValue = foodAmountR;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 30) {
                neuron.currentValue = foodAmountG;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 31) {
                neuron.currentValue = foodAmountB;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 32) {
                neuron.currentValue = distUp;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 33) {
                neuron.currentValue = distTopRight;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 34) {
                neuron.currentValue = distRight;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 35) {
                neuron.currentValue = distBottomRight;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 36) {
                neuron.currentValue = distDown;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 37) {
                neuron.currentValue = distBottomLeft;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 38) {
                neuron.currentValue = distLeft;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 39) {
                neuron.currentValue = distTopLeft;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 40) {
                neuron.currentValue = inComm0;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 41) {
                neuron.currentValue = inComm1;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 42) {
                neuron.currentValue = inComm2;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 43) {
                neuron.currentValue = inComm3;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }




            if (nid.neuronID == 100) {
                neuron.currentValue = throttleX;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
            if (nid.neuronID == 101) {
                neuron.currentValue = throttleY;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
            if (nid.neuronID == 102) {
                neuron.currentValue = dash;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
            if (nid.neuronID == 103) {
                neuron.currentValue = outComm0;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
            if (nid.neuronID == 104) {
                neuron.currentValue = outComm1;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
            if (nid.neuronID == 105) {
                neuron.currentValue = outComm2;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
            if (nid.neuronID == 106) {
                neuron.currentValue = outComm3;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
        }
    }

    public void Tick() {        

        //float xPos = ownRigidBody2D.transform.localPosition.x;
        //float yPos = ownRigidBody2D.transform.localPosition.y;
        Vector2 ownPos = new Vector2(ownRigidBody2D.transform.localPosition.x, ownRigidBody2D.transform.localPosition.y);
        Vector2 ownVel = new Vector2(ownRigidBody2D.velocity.x, ownRigidBody2D.velocity.y);

        Vector2 foodPos = Vector2.zero;
        Vector2 foodDir = Vector2.zero;
        float typeR = 0f;
        float typeG = 0f;
        float typeB = 0f;
        if(nearestFoodModule != null) {
            foodPos = new Vector2(nearestFoodModule.transform.localPosition.x - ownPos.x, nearestFoodModule.transform.localPosition.y - ownPos.y);
            foodDir = foodPos.normalized;
            typeR = nearestFoodModule.amountR;  // make a FoodModule Class to hold as reference which will contain Type info
            typeG = nearestFoodModule.amountG;
            typeB = nearestFoodModule.amountB;
        }

        Vector2 friendPos = Vector2.zero;
        Vector2 friendDir = Vector2.zero;
        Vector2 friendVel = Vector2.zero;
        if(friendTestModule != null) {
            friendPos = new Vector2(friendTestModule.ownRigidBody2D.transform.localPosition.x - ownPos.x, friendTestModule.ownRigidBody2D.transform.localPosition.y - ownPos.y);
            friendDir = friendPos.normalized;
            friendVel = new Vector2(friendTestModule.ownRigidBody2D.velocity.x, friendTestModule.ownRigidBody2D.velocity.y);
        }

        Vector2 enemyPos = Vector2.zero;
        Vector2 enemyDir = Vector2.zero;
        Vector2 enemyVel = Vector2.zero;
        if (nearestPredatorModule != null) {
            enemyPos = new Vector2(nearestPredatorModule.rigidBody.transform.localPosition.x - ownPos.x, nearestPredatorModule.rigidBody.transform.localPosition.y - ownPos.y);
            enemyDir = enemyPos.normalized;
            enemyVel = new Vector2(nearestPredatorModule.rigidBody.velocity.x, nearestPredatorModule.rigidBody.velocity.y);
        }

        foodPosX[0] = foodPos.x / 20f;
        foodPosY[0] = foodPos.y / 20f;
        foodDirX[0] = foodDir.x;
        foodDirY[0] = foodDir.y;
        foodTypeR[0] = typeR;
        foodTypeG[0] = typeG;
        foodTypeB[0] = typeB;

        friendPosX[0] = friendPos.x / 20f;
        friendPosY[0] = friendPos.y / 20f;
        friendVelX[0] = (friendVel.x - ownVel.x) / 15f;
        friendVelY[0] = (friendVel.y - ownVel.y) / 15f;
        friendDirX[0] = friendDir.x;
        friendDirY[0] = friendDir.y;

        enemyPosX[0] = enemyPos.x / 20f;
        enemyPosY[0] = enemyPos.y / 20f;        
        enemyVelX[0] = (enemyVel.x - ownVel.x) / 15f;
        enemyVelY[0] = (enemyVel.y - ownVel.y) / 15f;
        enemyDirX[0] = enemyDir.x;
        enemyDirY[0] = enemyDir.y;

        ownVelX[0] = ownVel.x / 15f;
        ownVelY[0] = ownVel.y / 15f;

        temperature[0] = 0f;
        pressure[0] = 0f;
        isContact[0] = 0f;
        contactForceX[0] = 0f;
        contactForceY[0] = 0f;
        hitPoints[0] = Mathf.Max(hitPoints[0], 0f);
        //stamina[0] = 1f;

        foodAmountR[0] = Mathf.Clamp(foodAmountR[0] - 0.01f, 0f, 1f);
        foodAmountG[0] = Mathf.Clamp(foodAmountG[0] - 0.01f, 0f, 1f);
        foodAmountB[0] = Mathf.Clamp(foodAmountB[0] - 0.01f, 0f, 1f);

        int rayLayer = LayerMask.GetMask("EnvironmentCollision");
        //Debug.Log(LayerMask.GetMask("EnvironmentCollision"));
        //Debug.Log(mask.ToString());

        // TOP
        float raycastMaxLength = 20f;
        RaycastHit2D hitTop = Physics2D.Raycast(ownPos, Vector2.up, raycastMaxLength, rayLayer);  // UP
        float distance = raycastMaxLength;
        if (hitTop.collider != null && hitTop.collider.tag == "HazardCollider") {
            distance = (hitTop.point - ownPos).magnitude;            
        }
        distUp[0] = (raycastMaxLength - distance) / raycastMaxLength;  // Mathf.Abs(40f - yPos) / 40f;        
        // TOP RIGHT
        RaycastHit2D hitTopRight = Physics2D.Raycast(ownPos, new Vector2(1f,1f), raycastMaxLength, rayLayer);  //  + / +
        distance = raycastMaxLength;
        if (hitTopRight.collider != null && hitTopRight.collider.tag == "HazardCollider") {
            distance = (hitTopRight.point - ownPos).magnitude;
        }
        distTopRight[0] = (raycastMaxLength - distance) / raycastMaxLength;
        // RIGHT
        RaycastHit2D hitRight = Physics2D.Raycast(ownPos, new Vector2(1f, 0f), raycastMaxLength, rayLayer);  //  + / +
        distance = raycastMaxLength;
        if (hitRight.collider != null && hitRight.collider.tag == "HazardCollider") {
            distance = (hitRight.point - ownPos).magnitude;
        }
        distRight[0] = (raycastMaxLength - distance) / raycastMaxLength;
        // BOTTOM RIGHT
        RaycastHit2D hitBottomRight = Physics2D.Raycast(ownPos, new Vector2(1f, -1f), raycastMaxLength, rayLayer);  //  + / +
        distance = raycastMaxLength;
        if (hitBottomRight.collider != null && hitBottomRight.collider.tag == "HazardCollider") {
            distance = (hitBottomRight.point - ownPos).magnitude;
        }
        distBottomRight[0] = (raycastMaxLength - distance) / raycastMaxLength;
        // BOTTOM
        RaycastHit2D hitBottom = Physics2D.Raycast(ownPos, new Vector2(0f, -1f), raycastMaxLength, rayLayer);  //  + / +
        distance = raycastMaxLength;
        if (hitBottom.collider != null && hitBottom.collider.tag == "HazardCollider") {
            distance = (hitBottom.point - ownPos).magnitude;
            //Debug.Log("HIT BOTTOM! " + ((raycastMaxLength - distance) / raycastMaxLength).ToString());
        }
        distDown[0] = (raycastMaxLength - distance) / raycastMaxLength;
        // BOTTOM LEFT
        RaycastHit2D hitBottomLeft = Physics2D.Raycast(ownPos, new Vector2(-1f, -1f), raycastMaxLength, rayLayer);  //  + / +
        distance = raycastMaxLength;
        if (hitBottomLeft.collider != null && hitBottomLeft.collider.tag == "HazardCollider") {
            distance = (hitBottomLeft.point - ownPos).magnitude;
        }
        distBottomLeft[0] = (raycastMaxLength - distance) / raycastMaxLength;
        // LEFT
        RaycastHit2D hitLeft = Physics2D.Raycast(ownPos, new Vector2(-1f, 0f), raycastMaxLength, rayLayer);  //  + / +
        distance = raycastMaxLength;
        if (hitLeft.collider != null && hitLeft.collider.tag == "HazardCollider") {
            distance = (hitLeft.point - ownPos).magnitude;
        }
        distLeft[0] = (raycastMaxLength - distance) / raycastMaxLength;
        // TOP LEFT
        RaycastHit2D hitTopLeft = Physics2D.Raycast(ownPos, new Vector2(-1f, 1f), raycastMaxLength, rayLayer);  //  + / +
        distance = raycastMaxLength;
        if (hitTopLeft.collider != null && hitTopLeft.collider.tag == "HazardCollider") {
            distance = (hitTopLeft.point - ownPos).magnitude;
        }
        distTopLeft[0] = (raycastMaxLength - distance) / raycastMaxLength;

        inComm0[0] = 0f;
        inComm1[0] = 0f;
        inComm2[0] = 0f;
        inComm3[0] = 0f;

        // TEST
        //hit = Physics2D.Raycast(new Vector2(35f, 0f), new Vector2(1f, 0f), raycastMaxLength);  //  + / +
        //distance = raycastMaxLength;
        //if (hit.collider != null && hit.collider.tag == "HazardCollider") {
            //distance = (hit.point - new Vector2(35f, 0f)).magnitude;
            //Debug.Log("HIT TEST! " + ((raycastMaxLength - distance) / raycastMaxLength).ToString());
        //}
        //distTopLeft[0] = (raycastMaxLength - distance) / raycastMaxLength;
    }
}
