using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterModuleFood {

	public int parentID;
    public int inno;

    // Nearest Edible Object:
    public float[] foodPosX;
    public float[] foodPosY;
    public float[] foodDirX;
    public float[] foodDirY;
    public float[] foodRelSize;

    public int nearestFoodParticleIndex = -1;  // debugging ** TEMP
    public Vector2 nearestFoodParticlePos;
    public float nearestFoodParticleAmount;

    public CritterModuleFood() {
                
    }

    public void Initialize(CritterModuleFoodSensorsGenome genome, Agent agent) {
        foodPosX = new float[1];  //1
        foodPosY = new float[1]; // 2
        foodDirX = new float[1];  // 3
        foodDirY = new float[1];  // 4
        foodRelSize = new float[1];  // 5

        this.parentID = genome.parentID;
        this.inno = genome.inno;
    }

    public void MapNeuron(NID nid, Neuron neuron) {
        if (inno == nid.moduleID) {
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
        }
    }

    public void Tick(SimulationManager simManager, Vector4 nutrientCellInfo, Agent agent) {
        nearestFoodParticleIndex = simManager.foodManager.closestFoodParticlesDataArray[agent.index].index;
        nearestFoodParticlePos = simManager.foodManager.closestFoodParticlesDataArray[agent.index].worldPos - new Vector2(simManager.agentsArray[agent.index].bodyRigidbody.transform.position.x,
                                                                                                           simManager.agentsArray[agent.index].bodyRigidbody.transform.position.y);

        nearestFoodParticleAmount = simManager.foodManager.closestFoodParticlesDataArray[agent.index].foodAmount;

        Vector2 foodPos = Vector2.zero;
        Vector2 foodDir = Vector2.zero;
        float foodAmount = 0f;
        float nearestFoodChunkSquareDistance = 100f;
        if(agent.coreModule.nearestEggSackModule != null) {
            foodPos = new Vector2(agent.coreModule.nearestEggSackModule.transform.localPosition.x - agent.ownPos.x, agent.coreModule.nearestEggSackModule.transform.localPosition.y - agent.ownPos.y);
            foodDir = foodPos.normalized;
            foodAmount = agent.coreModule.nearestEggSackModule.foodAmount;
            foodRelSize[0] = foodAmount;

            nearestFoodChunkSquareDistance = foodPos.sqrMagnitude;

            agent.coreModule.nearestEggSackPos = foodPos;
        }

        Vector2 critterToFoodParticle = simManager.foodManager.closestFoodParticlesDataArray[agent.index].worldPos - agent.ownPos;
        float distToNearestFoodParticle = critterToFoodParticle.magnitude;

        Vector2 foodParticleDir = critterToFoodParticle.normalized;
        float nearestFoodParticleSquareDistance = critterToFoodParticle.sqrMagnitude;
        if(agent.mouthRef.isPassive) {
            //float nearestFoodParticleSquareDistance = critterToFoodParticle.sqrMagnitude;
            //if(nearestFoodParticleSquareDistance < nearestFoodChunkSquareDistance) { // GPU Food PArticle:
            foodPosX[0] = nutrientCellInfo.y; //critterToFoodParticle.x / 20f; 
            foodPosY[0] = nutrientCellInfo.z;
            foodDirX[0] = foodParticleDir.x;
            foodDirY[0] = foodParticleDir.y;
            foodRelSize[0] = nutrientCellInfo.x;
                        
        }
        else { // Predator -- use CPU egg chunks:
            foodPosX[0] = foodDir.x;  // foodPos.x / 20f;
            foodPosY[0] = foodDir.y;  //foodPos.y / 20f;
            foodDirX[0] = foodParticleDir.x;
            foodDirY[0] = foodParticleDir.y;
        }
    }
}
