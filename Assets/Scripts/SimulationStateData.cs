using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationStateData {

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
    public struct EggData {  // 8f + 1i
        public int eggSackIndex;
        public Vector2 worldPos;
        public Vector2 localCoords;
        public Vector2 localScale;
        public float lifeStage;  // how grown is it?
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
    public AgentMovementAnimData[] agentMovementAnimDataArray;
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

    public Vector3[] depthAtAgentPositionsArray;
        

    public SimulationStateData(SimulationManager simManager) {
        InitializeData(simManager);
    }
	
    private void InitializeData(SimulationManager simManager) {

        /*agentSimDataArray = new AgentSimData[simManager._NumAgents];
        for(int i = 0; i < agentSimDataArray.Length; i++) {
            agentSimDataArray[i] = new AgentSimData();
        }
        agentSimDataCBuffer = new ComputeBuffer(agentSimDataArray.Length, sizeof(float) * 18);
        */
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

        eggSackSimDataArray = new EggSackSimData[simManager._NumEggSacks];
        for (int i = 0; i < eggSackSimDataArray.Length; i++) {
            eggSackSimDataArray[i] = new EggSackSimData();
        }
        eggSackSimDataCBuffer = new ComputeBuffer(eggSackSimDataArray.Length, sizeof(float) * 12 + sizeof(int) * 2);

        //StemData[] stemDataArray = new StemData[simManager._NumFood]; // one per food at first: // do this individually in a loop using the update kernel?
        //for (int i = 0; i < stemDataArray.Length; i++) {
        //    stemDataArray[i] = new StemData();
        //}
        foodStemDataCBuffer = new ComputeBuffer(simManager._NumEggSacks, sizeof(float) * 7 + sizeof(int) * 1);
        foodLeafDataCBuffer = new ComputeBuffer(simManager._NumEggSacks * 16, sizeof(float) * 7 + sizeof(int) * 1);

        eggDataCBuffer = new ComputeBuffer(eggSackSimDataCBuffer.count * 64, sizeof(float) * 8 + sizeof(int) * 1);

        /*predatorSimDataArray = new PredatorSimData[simManager._NumPredators];
        for (int i = 0; i < predatorSimDataArray.Length; i++) {
            predatorSimDataArray[i] = new PredatorSimData();
        }
        predatorSimDataCBuffer = new ComputeBuffer(predatorSimDataArray.Length, sizeof(float) * 5);
        */

        fluidVelocitiesAtAgentPositionsArray = new Vector2[simManager._NumAgents];
        fluidVelocitiesAtEggSackPositionsArray = new Vector2[simManager._NumEggSacks];
        //fluidVelocitiesAtPredatorPositionsArray = new Vector2[simManager._NumPredators];
        agentFluidPositionsArray = new Vector4[simManager._NumAgents];
        eggSackFluidPositionsArray = new Vector4[simManager._NumEggSacks];
        //predatorFluidPositionsArray = new Vector4[simManager._NumPredators];

        depthAtAgentPositionsArray = new Vector3[simManager._NumAgents];
    }

    public void PopulateSimDataArrays(SimulationManager simManager) {
        
        /*for(int i = 0; i < agentSimDataArray.Length; i++) {  
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
            
            
        }
        agentSimDataCBuffer.SetData(agentSimDataArray); // send data to GPU for Rendering
        */


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
            if(simManager.agentsArray[i].curLifeStage != Agent.AgentLifeStage.Egg) {
                //embryo = (float)simManager.agentsArray[i].lifeStageTransitionTimeStepCounter / (float)simManager.agentsArray[i]._GestationDurationTimeSteps;
                //embryo = Mathf.Clamp01(embryo);
                embryo = 0f;
            }
            //else {
            //    embryo = 1f;
            //}
            critterSimDataArray[i].embryoPercentage = embryo;
            critterSimDataArray[i].growthPercentage = simManager.agentsArray[i].growthPercentage;
            float decay = 0f;
            if(simManager.agentsArray[i].curLifeStage == Agent.AgentLifeStage.Dead) {
                decay = (float)simManager.agentsArray[i].lifeStageTransitionTimeStepCounter / (float)simManager.agentsArray[i]._DecayDurationTimeSteps;
            }
            if(simManager.agentsArray[i].curLifeStage == Agent.AgentLifeStage.AwaitingRespawn) {
                decay = 1f;
            }
            critterSimDataArray[i].decayPercentage = decay;

            if(simManager.agentsArray[i].isBeingSwallowed)
            {
                float digestedPercentage = (float)simManager.agentsArray[i].beingSwallowedFrameCounter / (float)simManager.agentsArray[i].swallowDuration;
                critterSimDataArray[i].growthPercentage = 1f - digestedPercentage;
                critterSimDataArray[i].decayPercentage = digestedPercentage;
            }
            critterSimDataArray[i].foodAmount = Mathf.Lerp(critterSimDataArray[i].foodAmount, simManager.agentsArray[i].coreModule.stomachContents / simManager.agentsArray[i].coreModule.stomachCapacity, 0.16f);
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
            if (simManager.agentsArray[i].curLifeStage == Agent.AgentLifeStage.Egg)
            {
                critterSimDataArray[i].isBiting = 0f;
                critterSimDataArray[i].biteAnimCycle *= 0.75f;
            }

            critterSimDataArray[i].moveAnimCycle = simManager.agentsArray[i].animationCycle;
            critterSimDataArray[i].turnAmount = simManager.agentsArray[i].turningAmount;
            critterSimDataArray[i].accel += Mathf.Clamp01(simManager.agentsArray[i].curAccel) * 1f; // ** RE-FACTOR!!!!
		    critterSimDataArray[i].smoothedThrottle = simManager.agentsArray[i].smoothedThrottle.magnitude;

            // Z & W coords represents agent's x/y Radii (in FluidCoords)
            agentFluidPositionsArray[i] = new Vector4(agentPos.x / SimulationManager._MapSize, 
                                                      agentPos.y / SimulationManager._MapSize, 
                                                      (simManager.agentsArray[i].fullSizeBoundingBox.x + 0.1f) * 0.5f / SimulationManager._MapSize, // **** RE-VISIT!!!!! ****
                                                      (simManager.agentsArray[i].fullSizeBoundingBox.y + 0.1f) * 0.5f / SimulationManager._MapSize); //... 0.5/140 ...
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


        for (int i = 0; i < simManager._NumEggSacks; i++) {
            Vector3 eggSackPos = simManager.eggSackArray[i].transform.position;
            int speciesSize = simManager._NumAgents / 4;
            int eggSpecies = Mathf.FloorToInt((float)i / (float)simManager._NumEggSacks * 4f);
            int agentGenomeIndex = simManager.eggSackArray[i].parentAgentIndex; // eggSpecies * speciesSize; // UnityEngine.Random.Range(eggSpecies * speciesSize, (eggSpecies + 1) * speciesSize);
            //if(simManager.eggSackArray[i].parentAgentRef != null) {
                //agentGenomeIndex = eggSpecies * speciesSize;
                //agentGenomeIndex = simManager.eggSackArray[i].parentAgentRef.index;
            //}            
            eggSackSimDataArray[i].parentAgentIndex = agentGenomeIndex;
            eggSackSimDataArray[i].worldPos = new Vector2(eggSackPos.x, eggSackPos.y);
            // *** Revisit to avoid using GetComponent, should use cached reference instead for speed:
            eggSackSimDataArray[i].velocity = simManager.eggSackArray[i].rigidbodyRef.velocity; // new Vector2(simManager.eggSackArray[i].rigidbodyRef.velocity.x, simManager.eggSackArray[i].rigidbodyRef.velocity.y);
            eggSackSimDataArray[i].heading = simManager.eggSackArray[i].facingDirection;
            //eggSackSimDataArray[i].heading = new Vector2(Mathf.Cos(Time.realtimeSinceStartup), Mathf.Sin(Time.realtimeSinceStartup)).normalized;
            eggSackSimDataArray[i].fullSize = simManager.eggSackArray[i].fullSize;
            eggSackSimDataArray[i].foodAmount = simManager.eggSackArray[i].foodAmount; // new Vector3(simManager.eggSackArray[i].foodAmount, simManager.eggSackArray[i].foodAmount, simManager.eggSackArray[i].foodAmount);
            eggSackSimDataArray[i].growth = simManager.eggSackArray[i].growthStatus;
            eggSackSimDataArray[i].decay = simManager.eggSackArray[i].decayStatus;
            eggSackSimDataArray[i].health = simManager.eggSackArray[i].healthStructural;
            // v v v below can be moved to a more static buffer eventually since they don't change every frame:
            //eggSackSimDataArray[i].stemBrushType = simManager.eggSackGenomePoolArray[i].stemBrushType;
            //eggSackSimDataArray[i].leafBrushType = simManager.eggSackGenomePoolArray[i].leafBrushType;
            //eggSackSimDataArray[i].fruitBrushType = simManager.eggSackGenomePoolArray[i].fruitBrushType;
            //eggSackSimDataArray[i].stemHue = simManager.eggSackGenomePoolArray[i].stemHue;
            //eggSackSimDataArray[i].leafHue = simManager.eggSackGenomePoolArray[i].leafHue;
            //eggSackSimDataArray[i].fruitHue = simManager.eggSackGenomePoolArray[i].fruitHue;
            

            // Z & W coords represents agent's x/y Radii (in FluidCoords)
            // convert from scene coords (-mapSize --> +mapSize to fluid coords (0 --> 1):::
            // **** Revisit and get working properly in both X and Y dimensions independently *********
            //float sampleRadius = (simManager.foodArray[i].curSize.magnitude + 0.1f) / (simManager._MapSize * 2f); // ****  ***** Revisit the 0.1f offset -- should be one pixel in fluidCoords?
            eggSackFluidPositionsArray[i] = new Vector4(eggSackPos.x / SimulationManager._MapSize, 
                                                      eggSackPos.y / SimulationManager._MapSize, 
                                                      (simManager.eggSackArray[i].curSize.x + 0.1f) * 0.5f / SimulationManager._MapSize, 
                                                      (simManager.eggSackArray[i].curSize.y + 0.1f) * 0.5f / SimulationManager._MapSize);
        }
        eggSackSimDataCBuffer.SetData(eggSackSimDataArray); // send data to GPU for Rendering
        
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
        // Grab data from GPU FluidSim!        
        fluidVelocitiesAtAgentPositionsArray = simManager.environmentFluidManager.GetFluidVelocityAtObjectPositions(agentFluidPositionsArray);
        fluidVelocitiesAtEggSackPositionsArray = simManager.environmentFluidManager.GetFluidVelocityAtObjectPositions(eggSackFluidPositionsArray);
        //fluidVelocitiesAtPredatorPositionsArray = simManager.environmentFluidManager.GetFluidVelocityAtObjectPositions(predatorFluidPositionsArray);

        depthAtAgentPositionsArray = simManager.theRenderKing.GetDepthAtObjectPositions(agentFluidPositionsArray);
    }
}
