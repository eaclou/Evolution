using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationStateData {

    // Stores data about the current simulation to be shared with RenderKing and FluidManager:

    public struct AgentSimData {
        public Vector2 worldPos;
        public Vector2 velocity;
        public Vector2 heading;
        public Vector2 size;
        public Vector3 primaryHue;  // can eventually pull these static variables out of here to avoid per-frame updates on non-dynamic attributes
        public Vector3 secondaryHue;
        public float maturity;  // 0-1 indicating growth
        public float decay;  // 0-1 indicating decayStatus
        public float eatingStatus;
        public float foodAmount;
    }
    public struct DebugBodyResourcesData {
        public float developmentPercentage;
        public float health;
        public float energy;
        public float stamina;
        public float stomachContents;
        public Vector2 mouthDimensions;
        public float mouthOffset;
        public float isBiting;
        public float isDamageFrame;        
    }
    public struct AgentMovementAnimData {
        public float animCycle;
        public float turnAmount;
        public float accel;
		public float smoothedThrottle;
    }
    public struct FoodSimData {  
        public Vector2 worldPos;
        public Vector2 velocity;
        public Vector2 heading;
        public Vector2 fullSize;
        public Vector3 foodAmount;
        public float growth;
        public float decay;
        public float health;
        public int stemBrushType;
        public int leafBrushType;
        public int fruitBrushType;
        public Vector3 stemHue;
        public Vector3 leafHue;
        public Vector3 fruitHue;
    }
    public struct StemData {  // Only the main trunk for now!!! worry about other ones later!!! SIMPLIFY!!!
        public int foodIndex;
        public Vector2 localBaseCoords;  // main trunk is always (0, -1f) --> (0f, 1f), secondary stems need to start with x=0 (to be on main trunk)
        public Vector2 localTipCoords;  // scaled with main Food scale
        public float width; // thickness of branch
        public float childGrowth; // for future use:  // 0-1, 1 means fully mature childFood attached to this, 0 means empty end  
        public float attached;
    }
    public struct LeafData { // fixed number, but some aren't used (zero scale??)
        public int foodIndex;
        public Vector2 worldPos;
        public Vector2 localCoords;
        public Vector2 localScale;
        public float attached;  // if attached, sticks to parent food, else, floats in water
    }
    public struct FruitData {
        public int foodIndex;
        public Vector2 worldPos;
        public Vector2 localCoords;
        public Vector2 localScale;
        public float attached;  // if attached, sticks to parent food, else, floats in water
    }

    public struct PredatorSimData {
        public Vector2 worldPos;
        public Vector2 velocity;
        public float scale;
    }

    // Store information from CPU simulation, to be passed to GPU:
    public AgentSimData[] agentSimDataArray;
    public DebugBodyResourcesData[] debugBodyResourcesArray;
    public AgentMovementAnimData[] agentMovementAnimDataArray;
    public FoodSimData[] foodSimDataArray;
    public PredatorSimData[] predatorSimDataArray;

    // Store computeBuffers here or in RenderKing?? These ones seem appropo for StateData but buffers of floatyBits for ex. might be better in RK....
    public ComputeBuffer agentSimDataCBuffer;
    public ComputeBuffer debugBodyResourcesCBuffer;
    public ComputeBuffer agentMovementAnimDataCBuffer;
    public ComputeBuffer foodSimDataCBuffer;
    public ComputeBuffer predatorSimDataCBuffer;

    public ComputeBuffer foodStemDataCBuffer;
    public ComputeBuffer foodLeafDataCBuffer;
    public ComputeBuffer foodFruitDataCBuffer;
        
    public Vector2[] fluidVelocitiesAtAgentPositionsArray;  // Grabs info about how Fluid should affect Agents from GPU
    public Vector4[] agentFluidPositionsArray;  // zw coords holds xy radius of agent  // **** Revisit this?? Redundancy btw AgentSimData worldPos (but simData doesn't have agent Radius)
    public Vector2[] fluidVelocitiesAtFoodPositionsArray;
    public Vector4[] foodFluidPositionsArray;
    public Vector2[] fluidVelocitiesAtPredatorPositionsArray;
    public Vector4[] predatorFluidPositionsArray;
        

    public SimulationStateData(SimulationManager simManager) {
        InitializeData(simManager);
    }
	
    private void InitializeData(SimulationManager simManager) {

        agentSimDataArray = new AgentSimData[simManager._NumAgents];
        for(int i = 0; i < agentSimDataArray.Length; i++) {
            agentSimDataArray[i] = new AgentSimData();
        }
        agentSimDataCBuffer = new ComputeBuffer(agentSimDataArray.Length, sizeof(float) * 18);

        debugBodyResourcesArray = new DebugBodyResourcesData[simManager._NumAgents];
        for(int i = 0; i < debugBodyResourcesArray.Length; i++) {
            debugBodyResourcesArray[i] = new DebugBodyResourcesData();
        }
        debugBodyResourcesCBuffer = new ComputeBuffer(debugBodyResourcesArray.Length, sizeof(float) * 10);

        agentMovementAnimDataArray = new AgentMovementAnimData[simManager._NumAgents];
        for(int i = 0; i < agentMovementAnimDataArray.Length; i++) {
            agentMovementAnimDataArray[i] = new AgentMovementAnimData();
        }
        agentMovementAnimDataCBuffer = new ComputeBuffer(agentMovementAnimDataArray.Length, sizeof(float) * 4);

        foodSimDataArray = new FoodSimData[simManager._NumFood];
        for (int i = 0; i < foodSimDataArray.Length; i++) {
            foodSimDataArray[i] = new FoodSimData();
        }
        foodSimDataCBuffer = new ComputeBuffer(foodSimDataArray.Length, sizeof(float) * 23 + sizeof(int) * 3); // got big

        //StemData[] stemDataArray = new StemData[simManager._NumFood]; // one per food at first: // do this individually in a loop using the update kernel?
        //for (int i = 0; i < stemDataArray.Length; i++) {
        //    stemDataArray[i] = new StemData();
        //}
        foodStemDataCBuffer = new ComputeBuffer(simManager._NumFood, sizeof(float) * 7 + sizeof(int) * 1);
        foodLeafDataCBuffer = new ComputeBuffer(simManager._NumFood * 16, sizeof(float) * 7 + sizeof(int) * 1);
        foodFruitDataCBuffer = new ComputeBuffer(simManager._NumFood * 64, sizeof(float) * 7 + sizeof(int) * 1);

        predatorSimDataArray = new PredatorSimData[simManager._NumPredators];
        for (int i = 0; i < predatorSimDataArray.Length; i++) {
            predatorSimDataArray[i] = new PredatorSimData();
        }
        predatorSimDataCBuffer = new ComputeBuffer(predatorSimDataArray.Length, sizeof(float) * 5);

        fluidVelocitiesAtAgentPositionsArray = new Vector2[simManager._NumAgents];
        fluidVelocitiesAtFoodPositionsArray = new Vector2[simManager._NumFood];
        fluidVelocitiesAtPredatorPositionsArray = new Vector2[simManager._NumPredators];
        agentFluidPositionsArray = new Vector4[simManager._NumAgents];
        foodFluidPositionsArray = new Vector4[simManager._NumFood];
        predatorFluidPositionsArray = new Vector4[simManager._NumPredators];
    }

    public void PopulateSimDataArrays(SimulationManager simManager) {
        
        for(int i = 0; i < agentSimDataArray.Length; i++) {  
            Vector3 agentPos = simManager.agentsArray[i].bodyRigidbody.position;
            agentSimDataArray[i].worldPos = new Vector2(agentPos.x, agentPos.y);  // in world(scene) coordinates
            //agentSimDataArray[i].velocity = simManager.agentsArray[i].smoothedThrottle;
            if(simManager.agentsArray[i].smoothedThrottle.sqrMagnitude == 0f) {
                //agentSimDataArray[i].velocity = simManager.agentsArray[i].facingDirection;
            }
            else {
                agentSimDataArray[i].velocity = simManager.agentsArray[i].smoothedThrottle.normalized;
            }
            agentSimDataArray[i].heading = simManager.agentsArray[i].facingDirection;
            agentSimDataArray[i].size = simManager.agentsArray[i].bodyCritterSegment.GetComponent<CapsuleCollider2D>().size;
            agentSimDataArray[i].primaryHue = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.huePrimary;
            agentSimDataArray[i].secondaryHue = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.hueSecondary;
            float maturity = 1f;
            float decay = 0f;
            if(simManager.agentsArray[i].curLifeStage == Agent.AgentLifeStage.Egg) {
                maturity = simManager.agentsArray[i].spawnStartingScale; // (float)simManager.agentsArray[i].lifeStageTransitionTimeStepCounter / (float)simManager.agentsArray[i]._GestationDurationTimeSteps;
            }
            if(simManager.agentsArray[i].curLifeStage == Agent.AgentLifeStage.Young) {
                maturity = Mathf.Lerp(simManager.agentsArray[i].spawnStartingScale, 1f, simManager.agentsArray[i].growthPercentage);
            }
            if(simManager.agentsArray[i].curLifeStage == Agent.AgentLifeStage.Decaying) {
                decay = (float)simManager.agentsArray[i].lifeStageTransitionTimeStepCounter / (float)simManager.agentsArray[i]._DecayDurationTimeSteps;
            }
            agentSimDataArray[i].maturity = maturity;
            agentSimDataArray[i].decay = decay;            
            if(simManager.agentsArray[i].isInsideFood) {
                agentSimDataArray[i].eatingStatus = (agentSimDataArray[i].eatingStatus + 0.11f) % 1.0f;  // cycle around 0-1
            }
            else {
                //if(agentSimDataArray[i].eatingStatus > 0.5f) {
                //    agentSimDataArray[i].eatingStatus *= 1.1f;
                //}
                //else {
                //    agentSimDataArray[i].eatingStatus *= 0.9f;
                //}
                agentSimDataArray[i].eatingStatus *= 0.7f;
            }
            // Experimental! move display data forward when chomping:
            float eatingCycle = Mathf.Sin(agentSimDataArray[i].eatingStatus * Mathf.PI);
            agentSimDataArray[i].worldPos += simManager.agentsArray[i].facingDirection * eatingCycle * 0.25f;
            agentSimDataArray[i].heading = (simManager.agentsArray[i].facingDirection + UnityEngine.Random.insideUnitCircle * 0.2f * eatingCycle).normalized;
            //agentSimDataArray[i].size.y *= (eatingCycle * 0.2f + 1.0f);
            //agentSimDataArray[i].size.x *= (1.0f - eatingCycle * 0.2f);

            agentSimDataArray[i].foodAmount = simManager.agentsArray[i].coreModule.foodAmountR[0];
            
            // Z & W coords represents agent's x/y Radii (in FluidCoords)
            // convert from scene coords (-mapSize --> +mapSize to fluid coords (0 --> 1):::
            agentFluidPositionsArray[i] = new Vector4((agentPos.x + simManager._MapSize) / (simManager._MapSize * 2f), 
                                                      (agentPos.y + simManager._MapSize) / (simManager._MapSize * 2f), 
                                                      (simManager.agentsArray[i].fullSizeBoundingBox.x + 0.1f) / (simManager._MapSize * 2f), 
                                                      (simManager.agentsArray[i].fullSizeBoundingBox.y + 0.1f) / (simManager._MapSize * 2f)); //... 0.5/140 ...
        }
        agentSimDataCBuffer.SetData(agentSimDataArray); // send data to GPU for Rendering

        for(int i = 0; i < debugBodyResourcesArray.Length; i++) {  
            
            debugBodyResourcesArray[i].developmentPercentage = simManager.agentsArray[i].growthPercentage;
            debugBodyResourcesArray[i].energy = simManager.agentsArray[i].coreModule.energyRaw; // *** max energy storage?? ****
            debugBodyResourcesArray[i].health = simManager.agentsArray[i].coreModule.healthBody;
            float biting = 0f;
            if (simManager.agentsArray[i].mouthRef.isBiting)
                biting = 1f;
            float damageFrame = 0f;
            if (simManager.agentsArray[i].mouthRef.bitingFrameCounter == simManager.agentsArray[i].mouthRef.biteChargeUpDuration)
                damageFrame = 1f;
            debugBodyResourcesArray[i].isBiting = biting;
            debugBodyResourcesArray[i].isDamageFrame = damageFrame;
            debugBodyResourcesArray[i].mouthDimensions = new Vector2(1f, 1f);
            debugBodyResourcesArray[i].mouthOffset = 0f;
            debugBodyResourcesArray[i].stamina = simManager.agentsArray[i].coreModule.stamina[0];
            debugBodyResourcesArray[i].stomachContents = simManager.agentsArray[i].coreModule.stomachContents / simManager.agentsArray[i].coreModule.stomachCapacity;
        }
        debugBodyResourcesCBuffer.SetData(debugBodyResourcesArray); // send data to GPU for Rendering

        // Movement Animation Test:
        for(int i = 0; i < agentMovementAnimDataArray.Length; i++) {            
            agentMovementAnimDataArray[i].animCycle = simManager.agentsArray[i].animationCycle;
            agentMovementAnimDataArray[i].turnAmount = simManager.agentsArray[i].turningAmount;
            agentMovementAnimDataArray[i].accel += Mathf.Clamp01(simManager.agentsArray[i].curAccel) * 1f;
            agentMovementAnimDataArray[i].smoothedThrottle = simManager.agentsArray[i].smoothedThrottle.magnitude;
        }
        agentMovementAnimDataCBuffer.SetData(agentMovementAnimDataArray); // send data to GPU for Rendering


        for (int i = 0; i < foodSimDataArray.Length; i++) {
            Vector3 foodPos = simManager.foodArray[i].transform.position;
            foodSimDataArray[i].worldPos = new Vector2(foodPos.x, foodPos.y);
            // *** Revisit to avoid using GetComponent, should use cached reference instead for speed:
            foodSimDataArray[i].velocity = new Vector2(simManager.foodArray[i].GetComponent<Rigidbody2D>().velocity.x, simManager.agentsArray[i].bodyRigidbody.velocity.y);
            foodSimDataArray[i].heading = simManager.foodArray[i].facingDirection;
            foodSimDataArray[i].fullSize = simManager.foodArray[i].fullSize;
            foodSimDataArray[i].foodAmount = new Vector3(simManager.foodArray[i].amountR, simManager.foodArray[i].amountR, simManager.foodArray[i].amountR);
            foodSimDataArray[i].growth = simManager.foodArray[i].growthStatus;
            foodSimDataArray[i].decay = simManager.foodArray[i].decayStatus;
            foodSimDataArray[i].health = simManager.foodArray[i].healthStructural;
            // v v v below can be moved to a more static buffer eventually since they don't change every frame:
            foodSimDataArray[i].stemBrushType = simManager.foodGenomePoolArray[i].stemBrushType;
            foodSimDataArray[i].leafBrushType = simManager.foodGenomePoolArray[i].leafBrushType;
            foodSimDataArray[i].fruitBrushType = simManager.foodGenomePoolArray[i].fruitBrushType;
            foodSimDataArray[i].stemHue = simManager.foodGenomePoolArray[i].stemHue;
            foodSimDataArray[i].leafHue = simManager.foodGenomePoolArray[i].leafHue;
            foodSimDataArray[i].fruitHue = simManager.foodGenomePoolArray[i].fruitHue;
            

            // Z & W coords represents agent's x/y Radii (in FluidCoords)
            // convert from scene coords (-mapSize --> +mapSize to fluid coords (0 --> 1):::
            // **** Revisit and get working properly in both X and Y dimensions independently *********
            //float sampleRadius = (simManager.foodArray[i].curSize.magnitude + 0.1f) / (simManager._MapSize * 2f); // ****  ***** Revisit the 0.1f offset -- should be one pixel in fluidCoords?
            foodFluidPositionsArray[i] = new Vector4((foodPos.x + simManager._MapSize) / (simManager._MapSize * 2f), 
                                                      (foodPos.y + simManager._MapSize) / (simManager._MapSize * 2f), 
                                                      (simManager.foodArray[i].curSize.x + 0.1f) / (simManager._MapSize * 2f), 
                                                      (simManager.foodArray[i].curSize.y + 0.1f) / (simManager._MapSize * 2f));
        }
        foodSimDataCBuffer.SetData(foodSimDataArray); // send data to GPU for Rendering
        
        for (int i = 0; i < predatorSimDataArray.Length; i++) {
            Vector3 predatorPos = simManager.predatorArray[i].transform.position;
            predatorSimDataArray[i].worldPos = new Vector2(predatorPos.x, predatorPos.y);
            predatorSimDataArray[i].velocity = new Vector2(simManager.predatorArray[i].rigidBody.velocity.x, simManager.predatorArray[i].rigidBody.velocity.y);
            predatorSimDataArray[i].scale = simManager.predatorArray[i].curScale;

            // Z & W coords represents agent's x/y Radii (in FluidCoords)
            // convert from scene coords (-mapSize --> +mapSize to fluid coords (0 --> 1):::
            //float sampleRadius = (simManager.predatorArray[i].curScale + 0.1f) / (simManager._MapSize * 2f); // ****  ***** Revisit the 0.1f offset -- should be one pixel in fluidCoords?
            predatorFluidPositionsArray[i] = new Vector4((predatorPos.x + simManager._MapSize) / (simManager._MapSize * 2f), 
                                                      (predatorPos.y + simManager._MapSize) / (simManager._MapSize * 2f), 
                                                      (simManager.predatorArray[i].curScale + 0.1f) / (simManager._MapSize * 2f), 
                                                      (simManager.predatorArray[i].curScale + 0.1f) / (simManager._MapSize * 2f));
        }
        predatorSimDataCBuffer.SetData(predatorSimDataArray);  // send data to GPU for Rendering

        // Grab data from GPU FluidSim!        
        fluidVelocitiesAtAgentPositionsArray = simManager.environmentFluidManager.GetFluidVelocityAtObjectPositions(agentFluidPositionsArray);
        fluidVelocitiesAtFoodPositionsArray = simManager.environmentFluidManager.GetFluidVelocityAtObjectPositions(foodFluidPositionsArray);
        fluidVelocitiesAtPredatorPositionsArray = simManager.environmentFluidManager.GetFluidVelocityAtObjectPositions(predatorFluidPositionsArray);
    }
}
