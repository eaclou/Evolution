using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // Temporary for debug purposes (view in inspector)
public class CritterModuleCore {

    public int parentID;
    public int inno;

    // **** LOOK INTO::: close in its own class or store in bigger array rather than all individual length-one arrays? 
    public float[] bias;

    public float foodConsumptionRate = 0.0025f * 0.5f; // *** *0.5 temporary!

    public float coreWidth;
    public float coreLength;
    
    public float energyRaw = 1f;
    public float maxEnergyStorage = 1f;
    //public float stamina = 1f;
    public float healthHead = 1f;
    public float healthBody = 1f;
    public float healthExternal = 1f;

    public float stomachContents = 0f;  // absolute values
    public float stomachCapacity = 0.5f;

    public float debugFoodValue = 0f;

    public Vector2 currentBodySize;

    public EggSack nearestEggSackModule;
    public Vector2 nearestEggSackPos;
    public int nearestFoodParticleIndex = -1;  // debugging ** TEMP
    public Vector2 nearestFoodParticlePos;
        
    // Nearest Edible Object:
    public float[] foodPosX;
    public float[] foodPosY;
    public float[] foodDirX;
    public float[] foodDirY;
    public float[] foodRelSize;
    //public float[] foodTypeR;
    //public float[] foodTypeG;
    //public float[] foodTypeB;

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

    public float[] enemyRelSize;
    public float[] enemyHealth;
    public float[] enemyGrowthStage;
    public float[] enemyThreatRating;
    
    //public float[] temperature;
    //public float[] pressure;
    public float[] isContact;
    public float[] contactForceX;
    public float[] contactForceY;

    public float[] hitPoints;
    public float[] stamina;
    public float[] energyStored;
    public float[] foodStored;

    //public float[] ownMouthStatus;

    //public float[] foodAmountR;
    //public float[] foodAmountG;
    //public float[] foodAmountB;
    
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
        
    public float[] outComm0;
    public float[] outComm1;
    public float[] outComm2;
    public float[] outComm3;  // 7 Out?

    public float[] mouthEffector;
        
    public PredatorModule nearestPredatorModule;
    public Agent nearestFriendAgent;
    public Agent nearestEnemyAgent;  
    

	public CritterModuleCore() {

    }

    public void Initialize(CritterModuleCoreGenome genome, Agent agent, StartPositionGenome startPos) {

        coreWidth = genome.fullBodyWidth;
        coreLength = genome.fullBodyLength;

        //numSegments = genome.numSegments;

        bias = new float[1];   //0
        foodPosX = new float[1];  //1
        foodPosY = new float[1]; // 2
        foodDirX = new float[1];  // 3
        foodDirY = new float[1];  // 4
        foodRelSize = new float[1];  // 5
        
        //foodTypeR = new float[1]; // 5
        //foodTypeG = new float[1]; // 6
        //foodTypeB = new float[1]; // 7

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

        enemyRelSize = new float[1];  // 200
        enemyHealth = new float[1];   // 201
        enemyGrowthStage = new float[1]; // 202
        enemyThreatRating = new float[1]; // 203
        
        //temperature = new float[1]; // 22
        //pressure = new float[1]; // 23
        isContact = new float[1]; // 24
        contactForceX = new float[1]; // 25
        contactForceY = new float[1]; // 26

        hitPoints = new float[1]; // 27
        stamina = new float[1]; // 28
        energyStored = new float[1];  // 204
        foodStored = new float[1];  // 205

        //foodAmountR = new float[1]; // 29
        //foodAmountG = new float[1]; // 30
        //foodAmountB = new float[1]; // 31
        
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
                
        outComm0 = new float[1]; // 3
        outComm1 = new float[1]; // 4
        outComm2 = new float[1]; // 5
        outComm3 = new float[1]; // 6 

        mouthEffector = new float[1];  // 206

        // 7 Total Outputs
                
        // ===============================================================================================================================
        /*
        foodAmountR[0] = 0.01f;
        foodAmountG[0] = 0.01f;
        foodAmountB[0] = 0.01f;
        if(agent.humanControlled) {  // if is Player:
            foodAmountR[0] = 1f;
            foodAmountG[0] = 1f;
            foodAmountB[0] = 1f;
        }*/
        

        energyRaw = coreWidth * coreLength * Mathf.Lerp(agent.spawnStartingScale, 1f, agent.growthPercentage);
        healthHead = 1f;
        healthBody = 1f;
        healthExternal = 1f;

        bias[0] = 1f;
        
        hitPoints[0] = 1f;
        stamina[0] = 1f;
        energyStored[0] = 1f;
        foodStored[0] = 0f;
        
        parentID = genome.parentID;
        inno = genome.inno;
    }

    public void MapNeuron(NID nid, Neuron neuron) {

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
                neuron.currentValue = foodRelSize;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            /*if (nid.neuronID == 5) {
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
            }*/
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

            if (nid.neuronID == 200) {
                neuron.currentValue = enemyRelSize;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 201) {
                neuron.currentValue = enemyHealth;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 202) {
                neuron.currentValue = enemyGrowthStage;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 203) {
                neuron.currentValue = enemyThreatRating;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }

            
            /*if (nid.neuronID == 22) {
                neuron.currentValue = temperature;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 23) {
                neuron.currentValue = pressure;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }*/
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
            if (nid.neuronID == 204) {
                neuron.currentValue = energyStored;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 205) {
                neuron.currentValue = foodStored;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            /*if (nid.neuronID == 29) {
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
            }*/
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

            if (nid.neuronID == 206) {
                neuron.currentValue = mouthEffector;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
        }
    }

    public void Tick(SimulationManager simManager, Vector4 nutrientCellInfo, bool isPassiveMouth, bool isPlayer, Vector2 ownPos, Vector2 ownVel, int agentIndex) {

        nearestFoodParticleIndex = simManager.closestFoodParticlesDataArray[agentIndex].index;
        nearestFoodParticlePos = simManager.closestFoodParticlesDataArray[agentIndex].worldPos - new Vector2(simManager.agentsArray[agentIndex].bodyRigidbody.transform.position.x,
                                                                                                           simManager.agentsArray[agentIndex].bodyRigidbody.transform.position.y);

        

        Vector2 foodPos = Vector2.zero;
        Vector2 foodDir = Vector2.zero;
        float foodAmount = 0f;
        //float typeG = 0f;
        //float typeB = 0f;
        float nearestFoodChunkSquareDistance = 100f;
        if(nearestEggSackModule != null) {
            foodPos = new Vector2(nearestEggSackModule.transform.localPosition.x - ownPos.x, nearestEggSackModule.transform.localPosition.y - ownPos.y);
            foodDir = foodPos.normalized;
            //typeR = nearestFoodModule.amountR;  // make a FoodModule Class to hold as reference which will contain Type info
            //typeG = nearestFoodModule.amountG;
            //typeB = nearestFoodModule.amountB;
            foodAmount = nearestEggSackModule.foodAmount;
            foodRelSize[0] = foodAmount;

            nearestFoodChunkSquareDistance = foodPos.sqrMagnitude;

            nearestEggSackPos = foodPos;
        }


        Vector2 friendPos = Vector2.zero;
        Vector2 friendDir = Vector2.zero;
        Vector2 friendVel = Vector2.zero;
        if(nearestFriendAgent != null) {
            friendPos = new Vector2(nearestFriendAgent.bodyRigidbody.transform.localPosition.x - ownPos.x, nearestFriendAgent.bodyRigidbody.transform.localPosition.y - ownPos.y);
            friendDir = friendPos.normalized;
            friendVel = new Vector2(nearestFriendAgent.bodyRigidbody.velocity.x, nearestFriendAgent.bodyRigidbody.velocity.y);
        }

        Vector2 enemyPos = Vector2.zero;
        Vector2 enemyDir = Vector2.zero;
        Vector2 enemyVel = Vector2.zero;
        if(nearestEnemyAgent != null) {
            enemyPos = new Vector2(nearestEnemyAgent.bodyRigidbody.transform.localPosition.x - ownPos.x, nearestEnemyAgent.bodyRigidbody.transform.localPosition.y - ownPos.y);
            enemyDir = enemyPos.normalized;
            enemyVel = new Vector2(nearestEnemyAgent.bodyRigidbody.velocity.x, nearestEnemyAgent.bodyRigidbody.velocity.y);

            float ownSize = currentBodySize.x;
            float enemySize = nearestEnemyAgent.coreModule.currentBodySize.x;

            if(ownSize != 0f && enemySize != 0) {
                float sizeRatio = enemySize / ownSize - 1f; 
                if(enemySize < ownSize) {
                    sizeRatio = -1f * (ownSize / enemySize - 1f);
                }
                enemyRelSize[0] = TransferFunctions.Evaluate(TransferFunctions.TransferFunction.RationalSigmoid, sizeRatio);  // smaller creatures negative values, larger creatures positive, 0 = same size
            }
            else {

            }            

            enemyHealth[0] = nearestEnemyAgent.coreModule.hitPoints[0];
            enemyGrowthStage[0] = nearestEnemyAgent.growthPercentage;

            float threat = 1f;
            if(nearestEnemyAgent.mouthRef.isPassive) {
                threat = 0f;
            }
            enemyThreatRating[0] = threat;
        }
        /*if (nearestPredatorModule != null) {
            enemyPos = new Vector2(nearestPredatorModule.rigidBody.transform.localPosition.x - ownPos.x, nearestPredatorModule.rigidBody.transform.localPosition.y - ownPos.y);
            enemyDir = enemyPos.normalized;
            enemyVel = new Vector2(nearestPredatorModule.rigidBody.velocity.x, nearestPredatorModule.rigidBody.velocity.y);
        }*/

        //float nearestFoodParticle = simManager.closestFoodParticlesDataArray[agentIndex].foodAmount;
        Vector2 critterToFoodParticle = simManager.closestFoodParticlesDataArray[agentIndex].worldPos - ownPos;
        float distToNearestFoodParticle = critterToFoodParticle.magnitude;

        Vector2 foodParticleDir = critterToFoodParticle.normalized;
        float nearestFoodParticleSquareDistance = critterToFoodParticle.sqrMagnitude;
        if(isPassiveMouth) {
            //float nearestFoodParticleSquareDistance = critterToFoodParticle.sqrMagnitude;
            //if(nearestFoodParticleSquareDistance < nearestFoodChunkSquareDistance) { // GPU Food PArticle:
                foodPosX[0] = foodParticleDir.x; ; // critterToFoodParticle.x / 20f; 
                foodPosY[0] = foodParticleDir.y;
                foodDirX[0] = nutrientCellInfo.y;
                foodDirY[0] = nutrientCellInfo.z;
                foodRelSize[0] = nutrientCellInfo.x; // simManager.closestFoodParticlesDataArray[agentIndex].foodAmount; // nutrientCellInfo.x;
            /*}
            else { // CPU foodChunk:
                foodPosX[0] = foodPos.x / 20f; 
                foodPosY[0] = foodPos.y / 20f;
                foodDirX[0] = foodDir.x;
                foodDirY[0] = foodDir.y; 
                foodRelSize[0] = foodAmount; 
            }*/
            
        }
        else { // Predator -- use CPU egg chunks:
            foodPosX[0] = foodDir.x;  // foodPos.x / 20f;
            foodPosY[0] = foodDir.y;  //foodPos.y / 20f;
            foodDirX[0] = enemyDir.x; // foodParticleDir.x;
            foodDirY[0] = enemyDir.y; // foodParticleDir.y;
        }

        //foodTypeR[0] = typeR;
        //foodTypeG[0] = typeG;
        //foodTypeB[0] = typeB;
        
        //foodRelSize[0] = nutrientCellInfo.x;
        
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

        

        //ownVelX[0] = ownVel.x / 15f;
        //ownVelY[0] = ownVel.y / 15f;

        //temperature[0] = 0f;
        //pressure[0] = 0f;
        isContact[0] = 0f;
        contactForceX[0] = 0f;
        contactForceY[0] = 0f;
        hitPoints[0] = Mathf.Max(healthBody, 0f);
        //stamina[0] = stamina; // set in Agent.cs
        energyStored[0] = Mathf.Clamp01(energyRaw / maxEnergyStorage);
        foodStored[0] = stomachContents / stomachCapacity;
        
        //mouthRef.foodInRange = false;
        
        // *** Handled within Agent.TickActions()
       // float foodDrain = foodConsumptionRate;
        //if(isPlayer) {
        //    foodDrain = foodDrain * 0.75f;
        //}        
        //foodAmountR[0] = Mathf.Max(foodAmountR[0] - foodDrain, 0f);
        //foodAmountG[0] = Mathf.Max(foodAmountG[0] - foodDrain, 0f);
        //foodAmountB[0] = Mathf.Max(foodAmountB[0] - foodDrain, 0f);
    
        
        int rayLayer = LayerMask.GetMask("EnvironmentCollision");
        //Debug.Log(LayerMask.GetMask("EnvironmentCollision"));
        //Debug.Log(mask.ToString());
        
        
        // TOP
        float raycastMaxLength = 10f;
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
        
        
        
        inComm0[0] = Mathf.Round(nearestFriendAgent.coreModule.outComm0[0] * 3f / 2f);
        inComm1[0] = Mathf.Round(nearestFriendAgent.coreModule.outComm1[0] * 3f / 2f);
        inComm2[0] = Mathf.Round(nearestFriendAgent.coreModule.outComm2[0] * 3f / 2f);
        inComm3[0] = Mathf.Round(nearestFriendAgent.coreModule.outComm3[0] * 3f / 2f);
        
        
    }
}
