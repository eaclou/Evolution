using UnityEngine;

public class SimulationStateData {
    SimulationManager simManager => SimulationManager.instance;
    TheRenderKing theRenderKing => TheRenderKing.instance;
    EnvironmentFluidManager fluidManager => EnvironmentFluidManager.instance;

    // Stores data about the current simulation to be shared with RenderKing and FluidManager:

    /*public struct AgentSimData {
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
    }*/

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
    
    /*public struct AgentMovementAnimData {
        public float animCycle;
        public float turnAmount;
        public float accel;
		public float smoothedThrottle;
    }*/
    
    public struct EggSackSimData {  // 12f + 2i
        public int parentAgentIndex;
        public Vector2 worldPos;
        public Vector2 velocity;
        public Vector2 heading;
        public Vector2 fullSize;
        public float foodAmount;
        public float growth;
        public float decay;
        public float health;
        public int brushType;
    }
    
    /*
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
    */
    
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

    /*public struct PredatorSimData {
        public Vector2 worldPos;
        public Vector2 velocity;
        public float scale;
    }*/

    // Store information from CPU simulation, to be passed to GPU:
    //public AgentSimData[] agentSimDataArray;
    public CritterInitData[] critterInitDataArray;
    public CritterSimData[] critterSimDataArray;
    //public DebugBodyResourcesData[] debugBodyResourcesArray;
    //public AgentMovementAnimData[] agentMovementAnimDataArray;
    public EggSackSimData[] eggSackSimDataArray;
    //public PredatorSimData[] predatorSimDataArray;

    // Store computeBuffers here or in RenderKing?? These ones seem appropo for StateData but buffers of floatyBits for ex. might be better in RK....
    //public ComputeBuffer agentSimDataCBuffer;
    public ComputeBuffer critterInitDataCBuffer;
    public ComputeBuffer critterSimDataCBuffer;
    public ComputeBuffer debugBodyResourcesCBuffer;
    public ComputeBuffer agentMovementAnimDataCBuffer;
    public ComputeBuffer eggSackSimDataCBuffer;
    //public ComputeBuffer predatorSimDataCBuffer;

    public ComputeBuffer foodStemDataCBuffer;
    public ComputeBuffer foodLeafDataCBuffer;
    public ComputeBuffer eggDataCBuffer;
        
    public Vector2[] fluidVelocitiesAtAgentPositionsArray;  // Grabs info about how Fluid should affect Agents from GPU
    public Vector4[] agentFluidPositionsArray;  // zw coords holds xy radius of agent  // **** Revisit this?? Redundancy btw AgentSimData worldPos (but simData doesn't have agent Radius)
    public Vector2[] fluidVelocitiesAtEggSackPositionsArray;
    public Vector4[] eggSackFluidPositionsArray;
    //public Vector2[] fluidVelocitiesAtPredatorPositionsArray;
    //public Vector4[] predatorFluidPositionsArray;

    public Vector4[] depthAtAgentPositionsArray;
    
    public SimulationStateData() {
        InitializeData();
    }

    public static int GetCritterInitDataSize() {
        return sizeof(float) * 25 + sizeof(int) * 3;
    }
    public static int GetCritterSimDataSize() {
        return sizeof(float) * 22;
    }
	
    private void InitializeData() {
        /*agentSimDataArray = new AgentSimData[simManager._NumAgents];
        for(int i = 0; i < agentSimDataArray.Length; i++) {
            agentSimDataArray[i] = new AgentSimData();
        }
        agentSimDataCBuffer = new ComputeBuffer(agentSimDataArray.Length, sizeof(float) * 18);
        */
        
        critterInitDataArray = new CritterInitData[simManager.maxAgents];
        for(int i = 0; i < critterInitDataArray.Length; i++) {
            critterInitDataArray[i] = new CritterInitData();
        }
        critterInitDataCBuffer = new ComputeBuffer(critterInitDataArray.Length, GetCritterInitDataSize());

        critterSimDataArray = new CritterSimData[simManager.maxAgents];
        for(int i = 0; i < critterSimDataArray.Length; i++) {
            critterSimDataArray[i] = new CritterSimData();
        }
        critterSimDataCBuffer = new ComputeBuffer(critterSimDataArray.Length, GetCritterSimDataSize());
        /*
        debugBodyResourcesArray = new DebugBodyResourcesData[simManager._NumAgents];
        for(int i = 0; i < debugBodyResourcesArray.Length; i++) {
            debugBodyResourcesArray[i] = new DebugBodyResourcesData();
        }
        debugBodyResourcesCBuffer = new ComputeBuffer(debugBodyResourcesArray.Length, sizeof(float) * 10);
        */
        /*agentMovementAnimDataArray = new AgentMovementAnimData[simManager._NumAgents];
        for(int i = 0; i < agentMovementAnimDataArray.Length; i++) {
            agentMovementAnimDataArray[i] = new AgentMovementAnimData();
        }
        agentMovementAnimDataCBuffer = new ComputeBuffer(agentMovementAnimDataArray.Length, sizeof(float) * 4);
        */
        eggSackSimDataArray = new EggSackSimData[simManager.maxEggSacks];
        for (int i = 0; i < eggSackSimDataArray.Length; i++) {
            eggSackSimDataArray[i] = new EggSackSimData();
        }
        eggSackSimDataCBuffer = new ComputeBuffer(eggSackSimDataArray.Length, sizeof(float) * 12 + sizeof(int) * 2);

        //StemData[] stemDataArray = new StemData[simManager._NumFood]; // one per food at first: // do this individually in a loop using the update kernel?
        //for (int i = 0; i < stemDataArray.Length; i++) {
        //    stemDataArray[i] = new StemData();
        //}
        foodStemDataCBuffer = new ComputeBuffer(simManager.maxEggSacks, sizeof(float) * 7 + sizeof(int) * 1);
        foodLeafDataCBuffer = new ComputeBuffer(simManager.maxEggSacks * 16, sizeof(float) * 7 + sizeof(int) * 1);
        eggDataCBuffer = new ComputeBuffer(eggSackSimDataCBuffer.count * 64, sizeof(float) * 8 + sizeof(int) * 1);

        /*predatorSimDataArray = new PredatorSimData[simManager._NumPredators];
        for (int i = 0; i < predatorSimDataArray.Length; i++) {
            predatorSimDataArray[i] = new PredatorSimData();
        }
        predatorSimDataCBuffer = new ComputeBuffer(predatorSimDataArray.Length, sizeof(float) * 5);
        */

        fluidVelocitiesAtAgentPositionsArray = new Vector2[simManager.maxAgents];
        fluidVelocitiesAtEggSackPositionsArray = new Vector2[simManager.maxEggSacks];
        //fluidVelocitiesAtPredatorPositionsArray = new Vector2[simManager._NumPredators];
        agentFluidPositionsArray = new Vector4[simManager.maxAgents];
        eggSackFluidPositionsArray = new Vector4[simManager.maxEggSacks];
        //predatorFluidPositionsArray = new Vector4[simManager._NumPredators];

        depthAtAgentPositionsArray = new Vector4[simManager.maxAgents];

        //Logger.Log("depthAtAgentPositionsArray " + fluidVelocitiesAtAgentPositionsArray.Length + ", " + agentFluidPositionsArray.Length);
    }

    public void PopulateSimDataArrays() {
        // CRITTER INIT: // *** MOVE INTO OWN FUNCTION -- update more efficiently with compute shader?
        for(int i = 0; i < simManager.maxAgents; i++) 
        {
            if (simManager.agents[i].isAwaitingRespawn) 
                continue;
            
            //Debug.Log("Error isInert FALSE: " + i.ToString());
            // INITDATA ::==========================================================================================================================================================================
            AgentGenome genome = simManager.agents[i].candidateRef.candidateGenome;
            critterInitDataArray[i].boundingBoxSize = simManager.agents[i].fullSizeBoundingBox; //genome.bodyGenome.GetFullsizeBoundingBox(); // simManager.agentsArray[i].fullSizeBoundingBox;
            critterInitDataArray[i].spawnSizePercentage = simManager.agents[i].spawnStartingScale;
            critterInitDataArray[i].maxEnergy = Mathf.Min(simManager.agents[i].fullSizeBoundingBox.x * simManager.agents[i].fullSizeBoundingBox.y, 0.5f);
            critterInitDataArray[i].maxStomachCapacity = simManager.agents[i].coreModule.stomachCapacity;
            critterInitDataArray[i].primaryHue = genome.primaryHue;
            critterInitDataArray[i].secondaryHue = genome.secondaryHue;
            // **** UPDATE THESE!!!
            critterInitDataArray[i].biteConsumeRadius = 1f; 
            critterInitDataArray[i].biteTriggerRadius = 1f;
            critterInitDataArray[i].biteTriggerLength = 1f;  // start out with these 3 same radius, can add second collision area later
            critterInitDataArray[i].eatEfficiencyPlant = 1f;
            critterInitDataArray[i].eatEfficiencyDecay = 1f;
            critterInitDataArray[i].eatEfficiencyMeat = 1f;  // can go negative for food/energy penalty in extreme cases?
            //critterInitDataArray[i].mouthIsActive = 1f;
            //if(simManager.agentsArray[i].mouthRef.isPassive) {
            //    critterInitDataArray[i].mouthIsActive = 0f;
            //}
            float critterFullsizeLength = genome.bodyGenome.coreGenome.tailLength + genome.bodyGenome.coreGenome.bodyLength + genome.bodyGenome.coreGenome.headLength + genome.bodyGenome.coreGenome.mouthLength;
            //0.175f, 0.525f
            float swimLerp = Mathf.Clamp01((genome.bodyGenome.coreGenome.creatureAspectRatio - 0.175f) / 0.35f);  // 0 = longest, 1 = shortest
            float flexibilityScore = 1f; // Mathf.Min((1f / genome.bodyGenome.coreGenome.creatureAspectRatio - 1f) * 0.6f, 6f);
            //float mouthLengthNormalized = genome.bodyGenome.coreGenome.mouthLength / critterFullsizeLength;
            //float approxRadius = genome.bodyGenome.coreGenome.creatureBaseLength * genome.bodyGenome.coreGenome.creatureAspectRatio;
            //float approxSize = 1f; // approxRadius * genome.bodyGenome.coreGenome.creatureBaseLength;
            // Mag range: 2 --> 0.5
            //freq range: 1 --> 2
            critterInitDataArray[i].swimMagnitude = Mathf.Lerp(0.33f, 1.5f, swimLerp); // 1f * (1f - flexibilityScore * 0.2f);
            critterInitDataArray[i].swimFrequency = Mathf.Lerp(2f, 0.8f, swimLerp);   //flexibilityScore * 1.05f;
            critterInitDataArray[i].swimAnimSpeed = 1f;    // 12f * (1f - approxSize * 0.25f);
            critterInitDataArray[i].bodyCoord = genome.bodyGenome.coreGenome.tailLength / critterFullsizeLength;
	        critterInitDataArray[i].headCoord = (genome.bodyGenome.coreGenome.tailLength + genome.bodyGenome.coreGenome.bodyLength) / critterFullsizeLength;
            critterInitDataArray[i].mouthCoord = (genome.bodyGenome.coreGenome.tailLength + genome.bodyGenome.coreGenome.bodyLength + genome.bodyGenome.coreGenome.headLength) / critterFullsizeLength;
            critterInitDataArray[i].bendiness = flexibilityScore;
            critterInitDataArray[i].bodyPatternX = simManager.agents[i].candidateRef.candidateGenome.bodyGenome.appearanceGenome.bodyStrokeBrushTypeX;
            critterInitDataArray[i].bodyPatternY = simManager.agents[i].candidateRef.candidateGenome.bodyGenome.appearanceGenome.bodyStrokeBrushTypeY;  // what grid cell of texture sheet to use
            critterInitDataArray[i].speciesID = simManager.agents[i].speciesIndex;
            
            // SIMDATA ::===========================================================================================================================================================================
            Vector2 agentPos = simManager.agents[i].bodyRigidbody.position;
            critterSimDataArray[i].worldPos = new Vector3(agentPos.x, agentPos.y, -SimulationManager._GlobalWaterLevel * SimulationManager._MaxAltitude);
            if(simManager.agents[i].smoothedThrottle.sqrMagnitude > 0f) {
                critterSimDataArray[i].velocity = simManager.agents[i].smoothedThrottle.normalized;
            }
            critterSimDataArray[i].heading = simManager.agents[i].facingDirection;            
            float embryo = 1f;
            if(simManager.agents[i].isEgg) {
                embryo = (float)simManager.agents[i].lifeStageTransitionTimeStepCounter / (float)simManager.agents[i].gestationDurationTimeSteps;
                embryo = Mathf.Clamp01(embryo);
            }
            critterSimDataArray[i].currentBiomass = simManager.agents[i].currentBiomass;
            critterSimDataArray[i].embryoPercentage = embryo;
            critterSimDataArray[i].growthPercentage = Mathf.Clamp01(simManager.agents[i].sizePercentage);
            float decay = 0f;
            if(simManager.agents[i].isDead) {
                decay = simManager.agents[i].GetDecayPercentage();
            }
            if(simManager.agents[i].isAwaitingRespawn) {
                decay = 0f;
            }
            critterSimDataArray[i].decayPercentage = decay;

            //if(simManager.agentsArray[i].isBeingSwallowed)
            //{
                //float digestedPercentage = (float)simManager.agentsArray[i].beingSwallowedFrameCounter / (float)simManager.agentsArray[i].swallowDuration;
                //critterSimDataArray[i].decayPercentage = digestedPercentage;
            //}
            critterSimDataArray[i].foodAmount = Mathf.Lerp(critterSimDataArray[i].foodAmount, simManager.agents[i].coreModule.stomachContentsPercent, 0.16f); //*********???????
            critterSimDataArray[i].energy = simManager.agents[i].coreModule.energy; // Raw / simManager.agentsArray[i].coreModule.maxEnergyStorage;
            critterSimDataArray[i].health = simManager.agents[i].coreModule.health;
            critterSimDataArray[i].stamina = simManager.agents[i].coreModule.stamina[0];
            
            // WPP: calculated in Agent with OR conditions to condense branches
            /*float isFreeToEat = 1f;
            if(simManager.agents[i].isFeeding) { // simManager.agentsArray[i].isFeeding) { // 
                isFreeToEat = 1f;
            }
            if(simManager.agents[i].isDefending) {
                isFreeToEat = 0f;
            }
            if(simManager.agents[i].isCooldown) {
                isFreeToEat = 0f;
            }
            if(simManager.agents[i].feedingFrameCounter > 4) {
                isFreeToEat = 0f;
            }*/
            critterSimDataArray[i].consumeOn = simManager.agents[i].isFreeToEat ? 1f : 0f;        

            critterSimDataArray[i].biteAnimCycle = 0f;
            if(simManager.agents[i].isFeeding) {
                critterSimDataArray[i].biteAnimCycle = Mathf.Clamp01((float)simManager.agents[i].feedingFrameCounter / (float)simManager.agents[i].feedAnimDuration);
            }
            if(simManager.agents[i].isAttacking) {
                critterSimDataArray[i].biteAnimCycle = simManager.agents[i].attackAnimCycle;
                //Mathf.Clamp01((float)simManager.agentsArray[i].attackingFrameCounter / (float)simManager.agentsArray[i].attackAnimDuration);
            }
            if (!simManager.agents[i].isMature)
            {
                critterSimDataArray[i].consumeOn = 0f;
            }

            critterSimDataArray[i].moveAnimCycle = simManager.agents[i].animationCycle;
            critterSimDataArray[i].turnAmount = simManager.agents[i].turningAmount * 2f;
            critterSimDataArray[i].accel += Mathf.Clamp01(simManager.agents[i].curAccel) * 0.8f; // ** REFACTOR!!!!
		    critterSimDataArray[i].smoothedThrottle = simManager.agents[i].smoothedThrottle.magnitude;
            critterSimDataArray[i].wasteProduced = simManager.agents[i].wasteProducedLastFrame;// 

            // Z & W coords represents agent's x/y Radii (in FluidCoords)
            float agentCenterX = agentPos.x / SimulationManager._MapSize;
            float agentCenterY = agentPos.y / SimulationManager._MapSize;
                           
            agentFluidPositionsArray[i] = new Vector4(agentCenterX, 
                                                      agentCenterY, 
                                                      (simManager.agents[i].fullSizeBoundingBox.x + 0.25f) * 0.5f / SimulationManager._MapSize,  // **** REVISIT!!!!! ****
                                                      (simManager.agents[i].fullSizeBoundingBox.y + 0.25f) * 0.5f / SimulationManager._MapSize); //... 0.5/140 ...
        }
        
        critterInitDataCBuffer.SetData(critterInitDataArray);        
        critterSimDataCBuffer.SetData(critterSimDataArray);
        
        for (int i = 0; i < simManager.maxEggSacks; i++) {
            Vector3 eggSackPos = simManager.eggSacks[i].transform.position;
            //int speciesSize = simManager._NumAgents / 4;
            //int eggSpecies = Mathf.FloorToInt((float)i / (float)simManager._NumEggSacks * 4f);
            int agentGenomeIndex = simManager.eggSacks[i].parentAgentIndex; // eggSpecies * speciesSize; // UnityEngine.Random.Range(eggSpecies * speciesSize, (eggSpecies + 1) * speciesSize);
                     
            eggSackSimDataArray[i].parentAgentIndex = agentGenomeIndex;
            eggSackSimDataArray[i].worldPos = new Vector2(eggSackPos.x, eggSackPos.y);
            eggSackSimDataArray[i].velocity = simManager.eggSacks[i].rigidbodyRef.velocity; // new Vector2(simManager.eggSackArray[i].rigidbodyRef.velocity.x, simManager.eggSackArray[i].rigidbodyRef.velocity.y);
            eggSackSimDataArray[i].heading = simManager.eggSacks[i].facingDirection;            
            eggSackSimDataArray[i].fullSize = simManager.eggSacks[i].fullSize;
            eggSackSimDataArray[i].foodAmount = simManager.eggSacks[i].foodAmount; // new Vector3(simManager.eggSackArray[i].foodAmount, simManager.eggSackArray[i].foodAmount, simManager.eggSackArray[i].foodAmount);
            eggSackSimDataArray[i].growth = simManager.eggSacks[i].developmentProgress;
            eggSackSimDataArray[i].decay = simManager.eggSacks[i].decayStatus;
            eggSackSimDataArray[i].health = simManager.eggSacks[i].healthStructural;
            
            // Z & W coords represents agent's x/y Radii (in FluidCoords)
            // convert from scene coords (-mapSize --> +mapSize to fluid coords (0 --> 1):::
            // **** Revisit and get working properly in both X and Y dimensions independently *********
            //float sampleRadius = (simManager.foodArray[i].curSize.magnitude + 0.1f) / (simManager._MapSize * 2f); // ****  ***** Revisit the 0.1f offset -- should be one pixel in fluidCoords?
            eggSackFluidPositionsArray[i] = new Vector4(eggSackPos.x / SimulationManager._MapSize, 
                                                      eggSackPos.y / SimulationManager._MapSize, 
                                                      (simManager.eggSacks[i].curSize.x + 0.1f) * 0.5f / SimulationManager._MapSize, 
                                                      (simManager.eggSacks[i].curSize.y + 0.1f) * 0.5f / SimulationManager._MapSize);
        }
        
        eggSackSimDataCBuffer.SetData(eggSackSimDataArray); // send data to GPU for Rendering
        
        // Grab data from GPU FluidSim!        
        fluidVelocitiesAtAgentPositionsArray = fluidManager.GetFluidVelocityAtObjectPositions(agentFluidPositionsArray);
        fluidVelocitiesAtEggSackPositionsArray = fluidManager.GetFluidVelocityAtObjectPositions(eggSackFluidPositionsArray);
        //fluidVelocitiesAtPredatorPositionsArray = simManager.environmentFluidManager.GetFluidVelocityAtObjectPositions(predatorFluidPositionsArray);
        //Debug.Log("agentFluidPositionsArray: " + agentFluidPositionsArray.Length.ToString());
        depthAtAgentPositionsArray = theRenderKing.GetDepthAtObjectPositions(agentFluidPositionsArray);
        
        #region Dead code (please delete)
        // Movement Animation Test:
        /*for(int i = 0; i < agentMovementAnimDataArray.Length; i++) {            
            agentMovementAnimDataArray[i].animCycle = simManager.agentsArray[i].animationCycle;
            agentMovementAnimDataArray[i].turnAmount = simManager.agentsArray[i].turningAmount;
            agentMovementAnimDataArray[i].accel += Mathf.Clamp01(simManager.agentsArray[i].curAccel) * 1f;
            agentMovementAnimDataArray[i].smoothedThrottle = simManager.agentsArray[i].smoothedThrottle.magnitude;
        }
        agentMovementAnimDataCBuffer.SetData(agentMovementAnimDataArray); // send data to GPU for Rendering
        */
        /*for (int i = 0; i < predatorSimDataArray.Length; i++) {
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
        */
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
        #endregion
    }
    
    public void Release()
    {
        //agentSimDataCBuffer?.Release();
        critterInitDataCBuffer?.Release();
        critterSimDataCBuffer?.Release();
        debugBodyResourcesCBuffer?.Release();
        agentMovementAnimDataCBuffer?.Release();            
        eggSackSimDataCBuffer?.Release();
        //predatorSimDataCBuffer?.Release();
        foodStemDataCBuffer?.Release();
        foodLeafDataCBuffer?.Release();
        eggDataCBuffer?.Release();
    }
}

// WPP: de-nest structs to avoid use of dot accessor elsewhere
// (nesting implies there is no reason for anything outside wrapping class to have access)
public struct CritterInitData 
{  
    // 25f + 3i
    public Vector3 boundingBoxSize;
    public float spawnSizePercentage;
    public float maxEnergy;
    public float maxStomachCapacity;
    public Vector3 primaryHue;
    public Vector3 secondaryHue;
    //public float mouthIsActive; // need this for animation? // morph this into mouthType?
    // public float biteRadius; // triggerArea where bite is successful
    public float biteConsumeRadius; 
    public float biteTriggerRadius;
    public float biteTriggerLength;
    public float eatEfficiencyPlant;
    public float eatEfficiencyDecay;
    public float eatEfficiencyMeat;
    public float swimMagnitude;
    public float swimFrequency;
    public float swimAnimSpeed;
    public float bodyCoord;
    public float headCoord;
    public float mouthCoord;
    public float bendiness;
    public int bodyPatternX;  // what grid cell of texture sheet to use
    public int bodyPatternY;  // what grid cell of texture sheet to use
    public int speciesID;
    
    public CritterInitData(BodyGenome bodyGenome)
    {
        boundingBoxSize = bodyGenome.GetFullsizeBoundingBox(); 
        spawnSizePercentage = 0.1f;
        maxEnergy = Mathf.Min(boundingBoxSize.x * boundingBoxSize.y, 0.5f);
        maxStomachCapacity = 1f;
        primaryHue = bodyGenome.primaryHue;
        secondaryHue = bodyGenome.secondaryHue;
        biteConsumeRadius = 1f;
        biteTriggerRadius = 1f;
        biteTriggerLength = 1f;
        eatEfficiencyPlant = 1f;
        eatEfficiencyDecay = 1f;
        eatEfficiencyMeat = 1f;

        // 0 = longest, 1 = shortest
        float swimLerp = Mathf.Clamp01((bodyGenome.coreGenome.creatureAspectRatio - 0.175f) / 0.35f); 

        // Mag range: 2 --> 0.5
        // freq range: 1 --> 2
        swimMagnitude = Mathf.Lerp(0.1f, 0.71f, swimLerp); 
        swimFrequency = Mathf.Lerp(1.05f, 0.4f, swimLerp);   
        swimAnimSpeed = 6f;

        bodyCoord = bodyGenome.coreGenome.bodyCoord;
        headCoord = bodyGenome.coreGenome.headCoord;
        mouthCoord = bodyGenome.coreGenome.mouthCoord;
        bendiness = 0.75f; 
        speciesID = 0; 
        
        // what grid cell of texture sheet to use
        bodyPatternX = bodyGenome.appearanceGenome.bodyStrokeBrushTypeX;
        bodyPatternY = bodyGenome.appearanceGenome.bodyStrokeBrushTypeY;  
    }
}

public struct CritterSimData 
{
    public Vector3 worldPos;
    public Vector2 velocity;
    public Vector2 heading;
    public float currentBiomass;
    public float embryoPercentage;
    public float growthPercentage;
    public float decayPercentage;
    public float foodAmount;  // stomach conetents normalized 01
    public float energy;
    public float health;
    public float stamina;
    public float consumeOn; // Use for consumption
    public float biteAnimCycle;
    public float moveAnimCycle;
    public float turnAmount;
    public float accel;
    public float smoothedThrottle;
    public float wasteProduced;  // newly added 8/1/2019
    
    public CritterSimData(bool isDead, bool allEvaluationsComplete)
    {
        biteAnimCycle = 0f; // (Time.realtimeSinceStartup * 1f) % 1f;
        worldPos = Vector3.one * 128f * 0.034f;
        float theta = isDead ? 0f : Time.realtimeSinceStartup;
        float angle = Mathf.Cos(theta * 0.67f) * 2f;
        Vector2 facingDir = new Vector2(0f, -1f); // new Vector2(Mathf.Cos(angle + Mathf.PI * 0.75f), Mathf.Sin(angle + Mathf.PI * 0.75f));
        heading = facingDir.normalized;
        float embryo = 1f; 
        embryoPercentage = embryo;
        growthPercentage = 2.25f;
        float decay = isDead && allEvaluationsComplete ? 0.25f : 0f;
        decayPercentage = decay;
        foodAmount = 0f; // Mathf.Lerp(simData.foodAmount, .agents[i].coreModule.stomachContents / agents[i].coreModule.stomachCapacity, 0.16f);
        energy = 1f; // agents[i].coreModule.energyRaw / agents[i].coreModule.maxEnergyStorage;
        health = 1f; // agents[i].coreModule.healthHead;
        stamina = 1f; // agents[i].coreModule.stamina[0];
        consumeOn = isDead ? 0f : Mathf.Sin(angle * 3.19f) * 0.5f + 0.5f;
        moveAnimCycle = isDead ? 0f : Time.realtimeSinceStartup * 0.6f % 1f;
        turnAmount = isDead ? 0f : Mathf.Sin(Time.realtimeSinceStartup * 0.654321f) * 0.65f + 0.25f;
        accel = isDead ? 0f : (Mathf.Sin(Time.realtimeSinceStartup * 0.79f) * 0.5f + 0.5f) * 0.081f; // Mathf.Clamp01(agents[i].curAccel) * 1f; // ** RE-FACTOR!!!!
        smoothedThrottle = isDead ? 0f : (Mathf.Sin(Time.realtimeSinceStartup * 3.97f + 0.4f) * 0.5f + 0.5f) * 0.85f;
        velocity = isDead ? Vector2.zero : facingDir.normalized * (accel + smoothedThrottle);
        
        currentBiomass = default;
        wasteProduced = default;
    }
}

// WPP: assigned but never used
public struct EggData {  // 8f + 1i
    public int eggSackIndex;
    public Vector2 worldPos;
    public Vector2 localCoords;
    public Vector2 localScale;
    public float lifeStage;  // how grown is it?
    public float attached;  // if attached, sticks to parent food, else, floats in water
    
    public EggData(int eggSackIndex, float scale, Vector2 worldPos, float attached = 1f)
    {
        this.eggSackIndex = eggSackIndex;
        localCoords = Random.insideUnitCircle;
        localScale = Vector2.one * scale;  
        this.worldPos = worldPos;
        this.attached = attached;
        
        lifeStage = default;
    }
}
