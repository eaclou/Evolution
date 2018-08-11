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
    public struct CritterInitData {
        public Vector3 boundingBoxSize;
        public float spawnSizePercentage;
        public float maxEnergy;
        public Vector3 primaryHue;
        public Vector3 secondaryHue;
        public float mouthIsActive;
        public int bodyPatternX;  // what grid cell of texture sheet to use
        public int bodyPatternY;  // what grid cell of texture sheet to use
    }
    public struct CritterSimData {
        public Vector3 worldPos;
        public Vector2 velocity;
        public Vector2 heading;
        public float embryoPercentage;
        public float growthPercentage;
        public float decayPercentage;
        public float foodAmount;
        public float energy;
        public float health;
        public float stamina;
        public float isBiting;
        public float biteAnimCycle;
        public float moveAnimCycle;
        public float turnAmount;
        public float accel;
		public float smoothedThrottle;
    }
    /*public struct DebugBodyResourcesData {
        public float developmentPercentage;
        public float health;
        public float energy;
        public float stamina;
        public float stomachContents;
        public Vector2 mouthDimensions;
        public float mouthOffset;
        public float isBiting;
        public float isDamageFrame;        
    }*/
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
    public CritterInitData[] critterInitDataArray;
    public CritterSimData[] critterSimDataArray;
    //public DebugBodyResourcesData[] debugBodyResourcesArray;
    public AgentMovementAnimData[] agentMovementAnimDataArray;
    public FoodSimData[] foodSimDataArray;
    public PredatorSimData[] predatorSimDataArray;

    // Store computeBuffers here or in RenderKing?? These ones seem appropo for StateData but buffers of floatyBits for ex. might be better in RK....
    public ComputeBuffer agentSimDataCBuffer;
    public ComputeBuffer critterInitDataCBuffer;
    public ComputeBuffer critterSimDataCBuffer;
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

        critterInitDataArray = new CritterInitData[simManager._NumAgents];
        for(int i = 0; i < critterInitDataArray.Length; i++) {
            critterInitDataArray[i] = new CritterInitData();
        }
        critterInitDataCBuffer = new ComputeBuffer(critterInitDataArray.Length, sizeof(float) * 12 + sizeof(int) * 2);

        critterSimDataArray = new CritterSimData[simManager._NumAgents];
        for(int i = 0; i < critterSimDataArray.Length; i++) {
            critterSimDataArray[i] = new CritterSimData();
        }
        critterSimDataCBuffer = new ComputeBuffer(critterSimDataArray.Length, sizeof(float) * 20);
        /*
        debugBodyResourcesArray = new DebugBodyResourcesData[simManager._NumAgents];
        for(int i = 0; i < debugBodyResourcesArray.Length; i++) {
            debugBodyResourcesArray[i] = new DebugBodyResourcesData();
        }
        debugBodyResourcesCBuffer = new ComputeBuffer(debugBodyResourcesArray.Length, sizeof(float) * 10);
        */
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
        foodFruitDataCBuffer = new ComputeBuffer(foodSimDataCBuffer.count * 64, sizeof(float) * 7 + sizeof(int) * 1);

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
            if(simManager.agentsArray[i].curLifeStage == Agent.AgentLifeStage.Dead) {
                decay = (float)simManager.agentsArray[i].lifeStageTransitionTimeStepCounter / (float)simManager.agentsArray[i]._DecayDurationTimeSteps;
            }
            agentSimDataArray[i].maturity = maturity;
            agentSimDataArray[i].decay = decay;            
            if(simManager.agentsArray[i].mouthRef.isBiting) {
                agentSimDataArray[i].eatingStatus = (float)simManager.agentsArray[i].mouthRef.bitingFrameCounter /
                                                    ((float)simManager.agentsArray[i].mouthRef.biteHalfCycleDuration + (float)simManager.agentsArray[i].mouthRef.biteCooldownDuration);
                // (agentSimDataArray[i].eatingStatus + 0.11f) % 1.0f;  // cycle around 0-1
                agentSimDataArray[i].eatingStatus = Mathf.Pow(agentSimDataArray[i].eatingStatus, 0.5f);
            }
            else {
                //if(agentSimDataArray[i].eatingStatus > 0.5f) {
                //    agentSimDataArray[i].eatingStatus *= 1.1f;
                //}
                //else {
                //    agentSimDataArray[i].eatingStatus *= 0.9f;
                //}
                //agentSimDataArray[i].eatingStatus *= 0.1f;
            }
            // Experimental! move display data forward when chomping:
            //float eatingCycle = Mathf.Sin(agentSimDataArray[i].eatingStatus * Mathf.PI);
            //agentSimDataArray[i].worldPos += simManager.agentsArray[i].facingDirection * eatingCycle * 0.25f;
            //agentSimDataArray[i].heading = (simManager.agentsArray[i].facingDirection + UnityEngine.Random.insideUnitCircle * 0.2f * eatingCycle).normalized;
            //agentSimDataArray[i].size.y *= (eatingCycle * 0.25f + 1.0f);
            //agentSimDataArray[i].size.x *= (1.0f - eatingCycle * 0.25f);

            agentSimDataArray[i].foodAmount = Mathf.Lerp(agentSimDataArray[i].foodAmount, simManager.agentsArray[i].coreModule.stomachContents / simManager.agentsArray[i].coreModule.stomachCapacity, 0.16f);
            
            // Z & W coords represents agent's x/y Radii (in FluidCoords)
            agentFluidPositionsArray[i] = new Vector4(agentPos.x / SimulationManager._MapSize, 
                                                      agentPos.y / SimulationManager._MapSize, 
                                                      (simManager.agentsArray[i].fullSizeBoundingBox.x + 0.1f) * 0.5f / SimulationManager._MapSize, // **** RE-VISIT!!!!! ****
                                                      (simManager.agentsArray[i].fullSizeBoundingBox.y + 0.1f) * 0.5f / SimulationManager._MapSize); //... 0.5/140 ...
        }
        agentSimDataCBuffer.SetData(agentSimDataArray); // send data to GPU for Rendering

        // CRITTER INIT: // *** MOVE INTO OWN FUNCTION -- update more efficiently with compute shader?
        for(int i = 0; i < critterInitDataArray.Length; i++) {
            critterInitDataArray[i].boundingBoxSize = simManager.agentsArray[i].fullSizeBoundingBox;
            critterInitDataArray[i].spawnSizePercentage = simManager.agentsArray[i].spawnStartingScale;
            critterInitDataArray[i].maxEnergy = Mathf.Min(simManager.agentsArray[i].fullSizeBoundingBox.x * simManager.agentsArray[i].fullSizeBoundingBox.y, 0.5f);
            critterInitDataArray[i].primaryHue = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.huePrimary;
            critterInitDataArray[i].secondaryHue = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.hueSecondary;
            critterInitDataArray[i].mouthIsActive = 1f;
            if(simManager.agentsArray[i].mouthRef.isPassive) {
                critterInitDataArray[i].mouthIsActive = 0f;
            }
            critterInitDataArray[i].bodyPatternX = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.bodyStrokeBrushTypeX;
            critterInitDataArray[i].bodyPatternY = simManager.agentGenomePoolArray[i].bodyGenome.appearanceGenome.bodyStrokeBrushTypeY;  // what grid cell of texture sheet to use
        }
        critterInitDataCBuffer.SetData(critterInitDataArray);
        
        // CRITTER SIM DATA: updated every frame:
        for(int i = 0; i < critterSimDataArray.Length; i++) {
            Vector3 agentPos = simManager.agentsArray[i].bodyRigidbody.position;
            critterSimDataArray[i].worldPos = new Vector3(agentPos.x, agentPos.y, 1f); // simManager.agentsArray[i].fullSizeBoundingBox.x * 0.5f);  // in world(scene) coordinates
            if(simManager.agentsArray[i].smoothedThrottle.sqrMagnitude > 0f) {
                critterSimDataArray[i].velocity = simManager.agentsArray[i].smoothedThrottle.normalized;
            }
            critterSimDataArray[i].heading = simManager.agentsArray[i].facingDirection;
            float embryo = 1f;
            if(simManager.agentsArray[i].curLifeStage == Agent.AgentLifeStage.Egg) {
                embryo = (float)simManager.agentsArray[i].lifeStageTransitionTimeStepCounter / (float)simManager.agentsArray[i]._GestationDurationTimeSteps;
                embryo = Mathf.Clamp01(embryo);
            }
            else {
                embryo = 1f;
            }
            critterSimDataArray[i].embryoPercentage = embryo;
            critterSimDataArray[i].growthPercentage = simManager.agentsArray[i].growthPercentage;
            float decay = 0f;
            if(simManager.agentsArray[i].curLifeStage == Agent.AgentLifeStage.Dead) {
                decay = (float)simManager.agentsArray[i].lifeStageTransitionTimeStepCounter / (float)simManager.agentsArray[i]._DecayDurationTimeSteps;
            }
            critterSimDataArray[i].decayPercentage = decay;

            if(simManager.agentsArray[i].isBeingSwallowed)
            {
                float digestedPercentage = (float)simManager.agentsArray[i].beingSwallowedFrameCounter / (float)simManager.agentsArray[i].swallowDuration;
                critterSimDataArray[i].growthPercentage = 1f - digestedPercentage;
                critterSimDataArray[i].decayPercentage = digestedPercentage;
            }
            critterSimDataArray[i].foodAmount = Mathf.Lerp(agentSimDataArray[i].foodAmount, simManager.agentsArray[i].coreModule.stomachContents / simManager.agentsArray[i].coreModule.stomachCapacity, 0.16f);
            critterSimDataArray[i].energy = simManager.agentsArray[i].coreModule.energyRaw / simManager.agentsArray[i].coreModule.maxEnergyStorage;
            critterSimDataArray[i].health = simManager.agentsArray[i].coreModule.healthHead;
            critterSimDataArray[i].stamina = simManager.agentsArray[i].coreModule.stamina[0];
            critterSimDataArray[i].isBiting = 0f;
            if(simManager.agentsArray[i].growthPercentage > 0.01f)
            {
                if (simManager.agentsArray[i].coreModule.mouthEffector[0] > 0.0f)
                {
                    if (simManager.agentsArray[i].mouthRef.isPassive)
                    {
                        critterSimDataArray[i].isBiting = 1f;
                    }
                    else
                    {
                        if (simManager.agentsArray[i].mouthRef.isBiting)
                        {
                            if (simManager.agentsArray[i].mouthRef.bitingFrameCounter <= simManager.agentsArray[i].mouthRef.biteHalfCycleDuration)
                            {
                                critterSimDataArray[i].isBiting = 1f;
                            }
                        }
                    }
                }
                if (simManager.agentsArray[i].mouthRef.isBiting)
                {
                    if (simManager.agentsArray[i].mouthRef.isPassive)
                    {
                        critterSimDataArray[i].biteAnimCycle = Mathf.Lerp(1f, critterSimDataArray[i].biteAnimCycle, 0.1f);
                    }
                    else
                    {
                        critterSimDataArray[i].biteAnimCycle = Mathf.Clamp01((float)simManager.agentsArray[i].mouthRef.bitingFrameCounter / (float)(simManager.agentsArray[i].mouthRef.biteHalfCycleDuration * 2));

                    }
                }
            }
            
            critterSimDataArray[i].moveAnimCycle = simManager.agentsArray[i].animationCycle;
            critterSimDataArray[i].turnAmount = simManager.agentsArray[i].turningAmount;
            critterSimDataArray[i].accel += Mathf.Clamp01(simManager.agentsArray[i].curAccel) * 1f; // ** RE-FACTOR!!!!
		    critterSimDataArray[i].smoothedThrottle = simManager.agentsArray[i].smoothedThrottle.magnitude;
        }
        critterSimDataCBuffer.SetData(critterSimDataArray);

        /*for(int i = 0; i < debugBodyResourcesArray.Length; i++) {  
            
            debugBodyResourcesArray[i].developmentPercentage = simManager.agentsArray[i].growthPercentage;
            debugBodyResourcesArray[i].energy = simManager.agentsArray[i].coreModule.energyRaw / simManager.agentsArray[i].coreModule.maxEnergyStorage; // *** max energy storage?? ****
            debugBodyResourcesArray[i].health = simManager.agentsArray[i].coreModule.healthBody;
            float biting = 0f;
            if (simManager.agentsArray[i].mouthRef.isBiting)
                biting = 1f;
            float damageFrame = 0f;
            if (simManager.agentsArray[i].mouthRef.bitingFrameCounter == simManager.agentsArray[i].mouthRef.biteHalfCycleDuration)
                damageFrame = 1f;
            debugBodyResourcesArray[i].isBiting = biting;
            debugBodyResourcesArray[i].isDamageFrame = damageFrame;
            debugBodyResourcesArray[i].mouthDimensions = new Vector2(1f, 1f);
            debugBodyResourcesArray[i].mouthOffset = 0f;
            debugBodyResourcesArray[i].stamina = simManager.agentsArray[i].coreModule.stamina[0];
            debugBodyResourcesArray[i].stomachContents = simManager.agentsArray[i].coreModule.stomachContents / simManager.agentsArray[i].coreModule.stomachCapacity;
        }
        debugBodyResourcesCBuffer.SetData(debugBodyResourcesArray); // send data to GPU for Rendering
        */

        // Movement Animation Test:
        for(int i = 0; i < agentMovementAnimDataArray.Length; i++) {            
            agentMovementAnimDataArray[i].animCycle = simManager.agentsArray[i].animationCycle;
            agentMovementAnimDataArray[i].turnAmount = simManager.agentsArray[i].turningAmount;
            agentMovementAnimDataArray[i].accel += Mathf.Clamp01(simManager.agentsArray[i].curAccel) * 1f;
            agentMovementAnimDataArray[i].smoothedThrottle = simManager.agentsArray[i].smoothedThrottle.magnitude;
        }
        agentMovementAnimDataCBuffer.SetData(agentMovementAnimDataArray); // send data to GPU for Rendering


        for (int i = 0; i < simManager._NumFood; i++) {
            Vector3 foodPos = simManager.foodArray[i].transform.position;
            foodSimDataArray[i].worldPos = new Vector2(foodPos.x, foodPos.y);
            // *** Revisit to avoid using GetComponent, should use cached reference instead for speed:
            foodSimDataArray[i].velocity = new Vector2(simManager.foodArray[i].GetComponent<Rigidbody2D>().velocity.x, simManager.foodArray[i].GetComponent<Rigidbody2D>().velocity.y);
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
            foodFluidPositionsArray[i] = new Vector4(foodPos.x / SimulationManager._MapSize, 
                                                      foodPos.y / SimulationManager._MapSize, 
                                                      (simManager.foodArray[i].curSize.x + 0.1f) * 0.5f / SimulationManager._MapSize, 
                                                      (simManager.foodArray[i].curSize.y + 0.1f) * 0.5f / SimulationManager._MapSize);
        }
        /*for (int i = 0; i < 32; i++) {  // Agent corpse food
            int dataIndex = simManager._NumFood + i;

            Vector3 foodPos = simManager.foodDeadAnimalArray[i].transform.position;
            foodSimDataArray[dataIndex].worldPos = new Vector2(foodPos.x, foodPos.y);
            // *** Revisit to avoid using GetComponent, should use cached reference instead for speed:
            foodSimDataArray[dataIndex].velocity = new Vector2(simManager.foodDeadAnimalArray[i].GetComponent<Rigidbody2D>().velocity.x, simManager.foodDeadAnimalArray[i].GetComponent<Rigidbody2D>().velocity.y);
            foodSimDataArray[dataIndex].heading = simManager.foodDeadAnimalArray[i].facingDirection;
            foodSimDataArray[dataIndex].fullSize = simManager.foodDeadAnimalArray[i].fullSize;
            foodSimDataArray[dataIndex].foodAmount = new Vector3(simManager.foodDeadAnimalArray[i].amountR, simManager.foodDeadAnimalArray[i].amountR, simManager.foodDeadAnimalArray[i].amountR);
            foodSimDataArray[dataIndex].growth = simManager.foodDeadAnimalArray[i].growthStatus;
            foodSimDataArray[dataIndex].decay = simManager.foodDeadAnimalArray[i].decayStatus;
            foodSimDataArray[dataIndex].health = simManager.foodDeadAnimalArray[i].healthStructural;
            // v v v below can be moved to a more static buffer eventually since they don't change every frame:
            foodSimDataArray[dataIndex].stemBrushType = simManager.foodGenomePoolArray[0].stemBrushType;
            foodSimDataArray[dataIndex].leafBrushType = simManager.foodGenomePoolArray[0].leafBrushType;
            foodSimDataArray[dataIndex].fruitBrushType = simManager.foodGenomePoolArray[0].fruitBrushType;
            foodSimDataArray[dataIndex].stemHue = simManager.foodGenomePoolArray[0].stemHue;
            foodSimDataArray[dataIndex].leafHue = simManager.foodGenomePoolArray[0].leafHue;
            foodSimDataArray[dataIndex].fruitHue = simManager.foodGenomePoolArray[0].fruitHue;
                       
        }*/
        foodSimDataCBuffer.SetData(foodSimDataArray); // send data to GPU for Rendering
        
        for (int i = 0; i < predatorSimDataArray.Length; i++) {
            Vector3 predatorPos = simManager.predatorArray[i].transform.position;
            predatorSimDataArray[i].worldPos = new Vector2(predatorPos.x, predatorPos.y);
            predatorSimDataArray[i].velocity = new Vector2(simManager.predatorArray[i].rigidBody.velocity.x, simManager.predatorArray[i].rigidBody.velocity.y);
            predatorSimDataArray[i].scale = simManager.predatorArray[i].curScale;

            // Z & W coords represents agent's x/y Radii (in FluidCoords)
            // convert from scene coords (-mapSize --> +mapSize to fluid coords (0 --> 1):::
            //float sampleRadius = (simManager.predatorArray[i].curScale + 0.1f) / (simManager._MapSize * 2f); // ****  ***** Revisit the 0.1f offset -- should be one pixel in fluidCoords?
            predatorFluidPositionsArray[i] = new Vector4(predatorPos.x / SimulationManager._MapSize, 
                                                      predatorPos.y / SimulationManager._MapSize, 
                                                      (simManager.predatorArray[i].curScale + 0.1f) * 0.5f / SimulationManager._MapSize, 
                                                      (simManager.predatorArray[i].curScale + 0.1f) * 0.5f / SimulationManager._MapSize);
        }
        predatorSimDataCBuffer.SetData(predatorSimDataArray);  // send data to GPU for Rendering

        // Grab data from GPU FluidSim!        
        fluidVelocitiesAtAgentPositionsArray = simManager.environmentFluidManager.GetFluidVelocityAtObjectPositions(agentFluidPositionsArray);
        fluidVelocitiesAtFoodPositionsArray = simManager.environmentFluidManager.GetFluidVelocityAtObjectPositions(foodFluidPositionsArray);
        fluidVelocitiesAtPredatorPositionsArray = simManager.environmentFluidManager.GetFluidVelocityAtObjectPositions(predatorFluidPositionsArray);
    }
}
