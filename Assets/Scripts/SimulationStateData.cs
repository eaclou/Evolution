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
    }
    public struct FoodSimData {
        public Vector2 worldPos;
        public Vector2 velocity;
        public float scale;
        public Vector3 foodAmount;
    }
    public struct PredatorSimData {
        public Vector2 worldPos;
        public Vector2 velocity;
        public float scale;
    }

    // Store information from CPU simulation, to be passed to GPU:
    public AgentSimData[] agentSimDataArray;
    public FoodSimData[] foodSimDataArray;
    public PredatorSimData[] predatorSimDataArray;

    // Store computeBuffers here or in RenderKing?? These ones seem appropo for StateData but buffers of floatyBits for ex. might be better in RK....
    public ComputeBuffer agentSimDataCBuffer;
    public ComputeBuffer foodSimDataCBuffer;
    public ComputeBuffer predatorSimDataCBuffer;
        
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
        agentSimDataCBuffer = new ComputeBuffer(agentSimDataArray.Length, sizeof(float) * 8);

        foodSimDataArray = new FoodSimData[simManager._NumFood];
        for (int i = 0; i < foodSimDataArray.Length; i++) {
            foodSimDataArray[i] = new FoodSimData();
        }
        foodSimDataCBuffer = new ComputeBuffer(foodSimDataArray.Length, sizeof(float) * 8);

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
            Vector3 agentPos = simManager.agentsArray[i].testModule.ownRigidBody2D.position;
            agentSimDataArray[i].worldPos = new Vector2(agentPos.x, agentPos.y);  // in world(scene) coordinates
            agentSimDataArray[i].velocity = simManager.agentsArray[i].smoothedThrottle; 
            agentSimDataArray[i].heading = simManager.agentsArray[i].facingDirection;
            agentSimDataArray[i].size = simManager.agentsArray[i].size;
            
            // Z & W coords represents agent's x/y Radii (in FluidCoords)
            // convert from scene coords (-mapSize --> +mapSize to fluid coords (0 --> 1):::
            agentFluidPositionsArray[i] = new Vector4((agentPos.x + simManager._MapSize) / (simManager._MapSize * 2f), 
                                                      (agentPos.y + simManager._MapSize) / (simManager._MapSize * 2f), 
                                                      simManager.agentsArray[i].size.x / (simManager._MapSize * 2f), 
                                                      simManager.agentsArray[i].size.y / (simManager._MapSize * 2f)); //... 0.5/140 ...
        }
        agentSimDataCBuffer.SetData(agentSimDataArray); // send data to GPU for Rendering

        for (int i = 0; i < foodSimDataArray.Length; i++) {
            Vector3 foodPos = simManager.foodArray[i].transform.position;
            foodSimDataArray[i].worldPos = new Vector2(foodPos.x, foodPos.y);
            // *** Revisit to avoid using GetComponent, should use cached reference instead for speed:
            foodSimDataArray[i].velocity = new Vector2(simManager.foodArray[i].GetComponent<Rigidbody2D>().velocity.x, simManager.agentsArray[i].GetComponent<Rigidbody2D>().velocity.y);
            foodSimDataArray[i].scale = simManager.foodArray[i].curScale;
            foodSimDataArray[i].foodAmount = new Vector3(simManager.foodArray[i].amountR, simManager.foodArray[i].amountG, simManager.foodArray[i].amountB);

            // Z & W coords represents agent's x/y Radii (in FluidCoords)
            // convert from scene coords (-mapSize --> +mapSize to fluid coords (0 --> 1):::
            float sampleRadius = (simManager.foodArray[i].curScale + 0.1f) / (simManager._MapSize * 2f); // ****  ***** Revisit the 0.1f offset -- should be one pixel in fluidCoords?
            foodFluidPositionsArray[i] = new Vector4((foodPos.x + simManager._MapSize) / (simManager._MapSize * 2f), 
                                                      (foodPos.y + simManager._MapSize) / (simManager._MapSize * 2f), 
                                                      sampleRadius, 
                                                      sampleRadius);
        }
        foodSimDataCBuffer.SetData(foodSimDataArray); // send data to GPU for Rendering

        for (int i = 0; i < predatorSimDataArray.Length; i++) {
            Vector3 predatorPos = simManager.predatorArray[i].transform.position;
            predatorSimDataArray[i].worldPos = new Vector2(predatorPos.x, predatorPos.y);
            predatorSimDataArray[i].velocity = new Vector2(simManager.predatorArray[i].rigidBody.velocity.x, simManager.predatorArray[i].rigidBody.velocity.y);
            predatorSimDataArray[i].scale = simManager.predatorArray[i].curScale;

            // Z & W coords represents agent's x/y Radii (in FluidCoords)
            // convert from scene coords (-mapSize --> +mapSize to fluid coords (0 --> 1):::
            float sampleRadius = (simManager.predatorArray[i].curScale + 0.1f) / (simManager._MapSize * 2f); // ****  ***** Revisit the 0.1f offset -- should be one pixel in fluidCoords?
            predatorFluidPositionsArray[i] = new Vector4((predatorPos.x + simManager._MapSize) / (simManager._MapSize * 2f), 
                                                      (predatorPos.y + simManager._MapSize) / (simManager._MapSize * 2f), 
                                                      sampleRadius, 
                                                      sampleRadius);
        }
        predatorSimDataCBuffer.SetData(predatorSimDataArray);  // send data to GPU for Rendering

        // Grab data from GPU FluidSim!        
        fluidVelocitiesAtAgentPositionsArray = simManager.environmentFluidManager.GetFluidVelocityAtObjectPositions(agentFluidPositionsArray);
        fluidVelocitiesAtFoodPositionsArray = simManager.environmentFluidManager.GetFluidVelocityAtObjectPositions(foodFluidPositionsArray);
        fluidVelocitiesAtPredatorPositionsArray = simManager.environmentFluidManager.GetFluidVelocityAtObjectPositions(predatorFluidPositionsArray);
    }
}
