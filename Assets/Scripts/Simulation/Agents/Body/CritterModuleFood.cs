using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterModuleFood {

	public int parentID;
    public int inno;

    // Nearest Edible Object:
    public float[] nutrientDensity;
    public float[] nutrientGradX;
    public float[] nutrientGradY;
    public float[] foodPosX;
    public float[] foodPosY;
    public float[] distance;
    public float[] foodVelX;
    public float[] foodVelY;
    public float[] foodDirX;
    public float[] foodDirY;    
    public float[] foodQuality;
    public float[] foodRelSize;

    public float sensorRange;
    public float preferredSize;
    public int[] foodPreferenceOrder;

    public int nearestFoodParticleIndex = -1;  // debugging ** TEMP
    public Vector2 nearestFoodParticlePos;
    public float nearestFoodParticleAmount;

    public CritterModuleFoodSensorsGenome genome;

    public CritterModuleFood() {
                
    }

    public void Initialize(CritterModuleFoodSensorsGenome genome, Agent agent) {
        this.genome = genome;

        //if(genome.useNutrients) {
            nutrientDensity = new float[1];
            nutrientGradX = new float[1];
            nutrientGradY = new float[1];
        //}
        //if(genome.usePos) {
            foodPosX = new float[1];
            foodPosY = new float[1];
            distance = new float[1];
        //}
        //if(genome.useVel) {
            foodVelX = new float[1];
            foodVelY = new float[1];
        //}
        //if(genome.useDir) {
            foodDirX = new float[1];
            foodDirY = new float[1];
        //}
        //if(genome.useStats) {
            foodQuality = new float[1];
            foodRelSize = new float[1];
        //}

        sensorRange = 25f; // TEMP HARDCODED *****
        preferredSize = genome.preferredSize;

        foodPreferenceOrder = new int[3];  // 0 == particle, 1=egg, 2==creature
        if(genome.preferenceParticles >= genome.preferenceEggs) {
            if(genome.preferenceParticles >= genome.preferenceCreatures) { // particles first place
                foodPreferenceOrder[0] = 0;    
                if(genome.preferenceEggs >= genome.preferenceCreatures) { // particles --> eggs --> creatures
                    foodPreferenceOrder[1] = 1;
                    foodPreferenceOrder[2] = 2;
                }
                else {  // particles --> creatures --> eggs
                    foodPreferenceOrder[1] = 2;
                    foodPreferenceOrder[2] = 1;
                }
            }
            else { // creatures --> particles --> eggs
                foodPreferenceOrder[0] = 2; 
                foodPreferenceOrder[1] = 0; 
                foodPreferenceOrder[2] = 1;                 
            }
        }
        else {
            if(genome.preferenceEggs >= genome.preferenceCreatures) { // eggs first place!
                foodPreferenceOrder[0] = 1;  
                if(genome.preferenceParticles >= genome.preferenceCreatures) { // eggs --> particles --> creatures
                    foodPreferenceOrder[1] = 0;
                    foodPreferenceOrder[2] = 2;
                }
                else {  // eggs --> creatures --> particles
                    foodPreferenceOrder[1] = 2;
                    foodPreferenceOrder[2] = 0;
                }
            }
            else {  // creatures --> eggs --> particles
                foodPreferenceOrder[0] = 2; 
                foodPreferenceOrder[1] = 1; 
                foodPreferenceOrder[2] = 0;  
            }
        }

        this.parentID = genome.parentID;
        this.inno = genome.inno;
    }

    public void MapNeuron(NID nid, Neuron neuron) {
        if (inno == nid.moduleID) {
            if (nid.neuronID == 1) {
                neuron.currentValue = nutrientDensity;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 2) {
                neuron.currentValue = nutrientGradX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 3) {
                neuron.currentValue = nutrientGradY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 4) {
                neuron.currentValue = foodPosX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 5) {
                neuron.currentValue = foodPosY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 6) {
                neuron.currentValue = distance;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 7) {
                neuron.currentValue = foodVelX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 8) {
                neuron.currentValue = foodVelY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 9) {
                neuron.currentValue = foodDirX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 10) {
                neuron.currentValue = foodDirY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 11) {
                neuron.currentValue = foodQuality;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 12) {
                neuron.currentValue = foodRelSize;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
        }
    }

    public void Tick(SimulationManager simManager, Vector4 nutrientCellInfo, Agent agent) {
        
        if(genome.useNutrients) {
            nutrientDensity[0] = nutrientCellInfo.x;
            nutrientGradX[0] = nutrientCellInfo.y;
            nutrientGradY[0] = nutrientCellInfo.z;
        }

        Vector2 foodPos = Vector2.zero;
        Vector2 foodDir = Vector2.zero;
        Vector2 foodVel = Vector2.zero;
        float dist = 0f;
        float quality = 1f;
        float relSize = 0f;
        
        // eventually, if none of first preference are in range of sensor, use next in line: *****
        if(foodPreferenceOrder[0] == 0) {  // particle
            nearestFoodParticleIndex = simManager.foodManager.closestFoodParticlesDataArray[agent.index].index;
            nearestFoodParticlePos = simManager.foodManager.closestFoodParticlesDataArray[agent.index].worldPos - 
                                     new Vector2(simManager.agentsArray[agent.index].bodyRigidbody.transform.position.x,
                                     simManager.agentsArray[agent.index].bodyRigidbody.transform.position.y);

            nearestFoodParticleAmount = simManager.foodManager.closestFoodParticlesDataArray[agent.index].foodAmount;
            Vector2 critterToFoodParticle = simManager.foodManager.closestFoodParticlesDataArray[agent.index].worldPos - agent.ownPos;
            float distToNearestFoodParticle = critterToFoodParticle.magnitude;
            Vector2 foodParticleDir = critterToFoodParticle.normalized;
            float nearestFoodParticleDistance = Mathf.Clamp01((sensorRange - critterToFoodParticle.magnitude) / sensorRange); // inverted dist(proximity) 0-1

            foodPos = new Vector2(Mathf.Clamp(critterToFoodParticle.x / sensorRange, -1f, 1f), Mathf.Clamp(critterToFoodParticle.y / sensorRange, -1f, 1f));
            foodDir = foodParticleDir;
            dist = nearestFoodParticleDistance;
            quality = 1f;
            relSize = nearestFoodParticleAmount;
            // no food particle Velocity yet! ***** nned to store in gpu struct?
        }
        else {
            if(foodPreferenceOrder[0] == 1) {  // egg
                if(agent.coreModule.nearestEggSackModule != null) {
                    Vector2 eggSackPos = new Vector2(agent.coreModule.nearestEggSackModule.rigidbodyRef.position.x - agent.ownPos.x, agent.coreModule.nearestEggSackModule.rigidbodyRef.position.y - agent.ownPos.y);
                    Vector2 eggSackDir = eggSackPos.normalized;
                    float nearestEggSackDistance = Mathf.Clamp01((sensorRange - eggSackPos.magnitude) / sensorRange);     

                    foodPos = new Vector2(Mathf.Clamp(eggSackPos.x / sensorRange, -1f, 1f), Mathf.Clamp(eggSackPos.y / sensorRange, -1f, 1f));
                    dist = nearestEggSackDistance;
                    // NO VEL YET!!! **** FIX!!!
                    foodDir = eggSackDir;
                    quality = 1f; // **** IMPLEMENT! (decay of eggSack?)
                    relSize = agent.coreModule.nearestEggSackModule.foodAmount;
                }
            }
            else {  // creatures
                if(agent.coreModule.nearestEnemyAgent != null) {
                    Vector2 preyPos = new Vector2(agent.coreModule.nearestEnemyAgent.bodyRigidbody.position.x - agent.ownPos.x, agent.coreModule.nearestEnemyAgent.bodyRigidbody.position.y - agent.ownPos.y);
                    Vector2 preyDir = preyPos.normalized;
                    float nearestPreyDistance = Mathf.Clamp01((sensorRange - preyPos.magnitude) / sensorRange);     

                    foodPos = new Vector2(Mathf.Clamp(preyPos.x / sensorRange, -1f, 1f), Mathf.Clamp(preyPos.y / sensorRange, -1f, 1f));
                    dist = nearestPreyDistance;
                    // NO VEL YET!!! **** FIX!!!
                    foodDir = preyDir;
                    quality = 1f; // **** IMPLEMENT! (decay of eggSack?)
                    relSize = (agent.coreModule.nearestEnemyAgent.currentBoundingBoxSize.x * agent.coreModule.nearestEnemyAgent.currentBoundingBoxSize.y) / (agent.currentBoundingBoxSize.x * agent.currentBoundingBoxSize.y);
                }
            }
        }
        
        if(genome.usePos) {
            foodPosX[0] = foodPos.x;
            foodPosY[0] = foodPos.y;
            distance[0] = dist;
        }
        if(genome.useVel) {

        }
        if(genome.useDir) {
            foodDirX[0] = foodDir.x;
            foodDirY[0] = foodDir.y;
        }
        if(genome.useStats) {
            foodQuality[0] = quality; // *** temp until particles can decay naturally
            foodRelSize[0] = relSize;
        }
    }
}
