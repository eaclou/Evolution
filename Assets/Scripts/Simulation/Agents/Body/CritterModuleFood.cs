using UnityEngine;

[System.Serializable]
public class CritterModuleFood {

	public int parentID;
    public int inno;

    public int nearestFoodParticleIndex = -1;  // debugging ** TEMP
    public Vector2 nearestFoodParticlePos;
    public float nearestFoodParticleAmount;

    public int nearestAnimalParticleIndex = -1;  // debugging ** TEMP
    public Vector2 nearestAnimalParticlePos;
    public float nearestAnimalParticleAmount;

    // Nearest Edible Object:
    public float[] nutrientDensity;
    public float[] nutrientGradX;
    public float[] nutrientGradY;

    public float[] foodPlantPosX;
    public float[] foodPlantPosY;
    public float[] foodPlantDistance;
    public float[] foodPlantVelX;
    public float[] foodPlantVelY;
    public float[] foodPlantDirX;
    public float[] foodPlantDirY;    
    public float[] foodPlantQuality;
    public float[] foodPlantRelSize;

    public float[] foodAnimalPosX;
    public float[] foodAnimalPosY;
    public float[] foodAnimalDistance;
    public float[] foodAnimalVelX;
    public float[] foodAnimalVelY;
    public float[] foodAnimalDirX;
    public float[] foodAnimalDirY;    
    public float[] foodAnimalQuality;
    public float[] foodAnimalRelSize;

    public float[] foodEggDistance;
    public float[] foodEggDirX;
    public float[] foodEggDirY;

    public float[] foodCorpseDistance;
    public float[] foodCorpseDirX;
    public float[] foodCorpseDirY;

    public float sensorRange;
    public float preferredSize;
    //public int[] foodPreferenceOrder;
    

    public CritterModuleFoodSensorsGenome genome;

    public CritterModuleFood(CritterModuleFoodSensorsGenome genome, Agent agent) {
        Initialize(genome, agent);
    }

    public void Initialize(CritterModuleFoodSensorsGenome genome, Agent agent) {
        this.genome = genome;

        if(genome.useNutrients) {
            nutrientDensity = new float[1];
            nutrientGradX = new float[1];
            nutrientGradY = new float[1];
        }
        if(genome.usePos) {
            foodPlantPosX = new float[1];
            foodPlantPosY = new float[1];
            
            foodAnimalPosX = new float[1];
            foodAnimalPosY = new float[1];
            
        }
        if(genome.useVel) {
            foodPlantVelX = new float[1];
            foodPlantVelY = new float[1];
            foodAnimalVelX = new float[1];
            foodAnimalVelY = new float[1];
        }
        //if(genome.useDir) {
        foodPlantDistance = new float[1];
        foodPlantDirX = new float[1];
        foodPlantDirY = new float[1];

        foodAnimalDistance = new float[1];
        foodAnimalDirX = new float[1];
        foodAnimalDirY = new float[1];

        foodEggDistance = new float[1];
        foodEggDirX = new float[1];
        foodEggDirY = new float[1];

        foodCorpseDistance = new float[1];
        foodCorpseDirX = new float[1];
        foodCorpseDirY = new float[1];
        //}
        if(genome.useStats) {
            foodPlantQuality = new float[1];
            foodPlantRelSize = new float[1];
            foodAnimalQuality = new float[1];
            foodAnimalRelSize = new float[1];
        }
                

        sensorRange = 16f; // TEMP HARDCODED *****
        preferredSize = genome.preferredSize;

        /*foodPreferenceOrder = new int[3];  // 0 == particle, 1=egg, 2==creature
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
        }*/

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
                neuron.currentValue = foodPlantPosX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 5) {
                neuron.currentValue = foodPlantPosY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 6) {
                neuron.currentValue = foodPlantDistance;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 7) {
                neuron.currentValue = foodPlantVelX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 8) {
                neuron.currentValue = foodPlantVelY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 9) {
                neuron.currentValue = foodPlantDirX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 10) {
                neuron.currentValue = foodPlantDirY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 11) {
                neuron.currentValue = foodPlantQuality;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 12) {
                neuron.currentValue = foodPlantRelSize;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }

            if (nid.neuronID == 13) {
                neuron.currentValue = foodEggDistance;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 14) {
                neuron.currentValue = foodEggDirX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 15) {
                neuron.currentValue = foodEggDirY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 16) {
                neuron.currentValue = foodCorpseDistance;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 17) {
                neuron.currentValue = foodCorpseDirX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 18) {
                neuron.currentValue = foodCorpseDirY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }

            if (nid.neuronID == 24) {
                neuron.currentValue = foodAnimalPosX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 25) {
                neuron.currentValue = foodAnimalPosY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 26) {
                neuron.currentValue = foodAnimalDistance;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 27) {
                neuron.currentValue = foodAnimalVelX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 28) {
                neuron.currentValue = foodAnimalVelY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 29) {
                neuron.currentValue = foodAnimalDirX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 30) {
                neuron.currentValue = foodAnimalDirY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 31) {
                neuron.currentValue = foodAnimalQuality;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 32) {
                neuron.currentValue = foodAnimalRelSize;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
        }
    }

    public void Tick(SimulationManager simManager, Agent agent) {
        /*if(genome.useNutrients) {
            nutrientDensity[0] = nutrientCellInfo.x;
            nutrientGradX[0] = nutrientCellInfo.y;
            nutrientGradY[0] = nutrientCellInfo.z;
        }*/

        ////Vector2 foodPos = Vector2.zero;
        //Vector2 foodDir = Vector2.zero;
        //Vector2 foodVel = Vector2.zero;
        //float dist = 0f;
        //float quality = 1f;
        //float relSize = 0f;

        // eventually, if none of first preference are in range of sensor, use next in line: *****
        //foodPreferenceOrder[0] = 0;
        //if(foodPreferenceOrder[0] == 0) {  // particle
        nearestFoodParticleIndex = simManager.vegetationManager.closestPlantParticlesDataArray[agent.index].index;
        nearestFoodParticlePos = simManager.vegetationManager.closestPlantParticlesDataArray[agent.index].worldPos - 
                                    new Vector2(simManager.agents[agent.index].bodyRigidbody.transform.position.x,
                                    simManager.agents[agent.index].bodyRigidbody.transform.position.y);

        nearestFoodParticleAmount = simManager.vegetationManager.closestPlantParticlesDataArray[agent.index].biomass;
        Vector2 critterToFoodParticle = simManager.vegetationManager.closestPlantParticlesDataArray[agent.index].worldPos - agent.ownPos;
        float distToNearestFoodParticle = critterToFoodParticle.magnitude;
        Vector2 foodParticleDir = critterToFoodParticle.normalized;
        float nearestFoodParticleDistance = Mathf.Clamp01((sensorRange - critterToFoodParticle.magnitude) / sensorRange); // inverted dist(proximity) 0-1


        //ANIMAL ZOOPLANKTON:
        nearestAnimalParticleIndex = simManager.zooplanktonManager.closestAnimalParticlesDataArray[agent.index].index;
        nearestAnimalParticlePos = simManager.zooplanktonManager.closestAnimalParticlesDataArray[agent.index].worldPos - simManager.agents[agent.index].bodyRigidbody.transform.position;

        nearestAnimalParticleAmount = simManager.zooplanktonManager.closestAnimalParticlesDataArray[agent.index].biomass;
        Vector2 critterToAnimalParticle = new Vector2(nearestAnimalParticlePos.x, nearestAnimalParticlePos.y); // - agent.ownPos;
        float distToNearestAnimalParticle = critterToAnimalParticle.magnitude;
        Vector2 animalParticleDir = critterToAnimalParticle.normalized;
        float nearestAnimalParticleDistance = Mathf.Clamp01((sensorRange - critterToAnimalParticle.magnitude) / sensorRange); // inverted dist(proximity) 0-1
        
        if(genome.useStats) {
            foodPlantQuality[0] = 1f; // *** temp until particles can decay naturally
            foodPlantRelSize[0] = nearestFoodParticleAmount;

            foodAnimalQuality[0] = 1f; // *** temp until particles can decay naturally
            foodAnimalRelSize[0] = nearestAnimalParticleAmount;
        }
        if(genome.usePos) {
            foodPlantPosX[0] = Mathf.Clamp(critterToFoodParticle.x / sensorRange, -1f, 1f);
            foodPlantPosY[0] = Mathf.Clamp(critterToFoodParticle.y / sensorRange, -1f, 1f);

            foodAnimalPosX[0] = Mathf.Clamp(critterToAnimalParticle.x / sensorRange, -1f, 1f);
            foodAnimalPosY[0] = Mathf.Clamp(critterToAnimalParticle.y / sensorRange, -1f, 1f);
        }
        if(genome.useVel) {

        }
        if(genome.useDir) {
            foodPlantDirX[0] = foodParticleDir.x;
            foodPlantDirY[0] = foodParticleDir.y;
            foodPlantDistance[0] = nearestFoodParticleDistance;

            foodAnimalDistance[0] = nearestAnimalParticleDistance;
            foodAnimalDirX[0] = animalParticleDir.x;
            foodAnimalDirY[0] = animalParticleDir.y;
        }
        
        // EGGS:::::
        if (agent.coreModule.nearestEggSackModule) {
            Vector2 eggSackPos = new Vector2(agent.coreModule.nearestEggSackModule.rigidbodyRef.position.x - agent.ownPos.x, agent.coreModule.nearestEggSackModule.rigidbodyRef.position.y - agent.ownPos.y);
            Vector2 eggSackDir = eggSackPos.normalized;
            float nearestEggSackDistance = Mathf.Clamp01((sensorRange - eggSackPos.magnitude) / sensorRange);

            if(genome.useDir) {
                foodEggDistance[0] = nearestEggSackDistance;
                foodEggDirX[0] = eggSackDir.x;
                foodEggDirY[0] = eggSackDir.y;
            }
        }

        // CORPSE FOOD:::::           
        if (agent.coreModule.nearestEnemyAgent && agent.isDead) {
            Vector3 preyPos3 = agent.coreModule.nearestEnemyAgent.bodyRigidbody.position;
            Vector2 preyPos = new Vector2(preyPos3.x - agent.ownPos.x, preyPos3.y - agent.ownPos.y);
            Vector2 preyDir = preyPos.normalized;
            float nearestPreyDistance = Mathf.Clamp01((sensorRange - preyPos.magnitude) / sensorRange);

            if (genome.useDir) {
                foodCorpseDistance[0] = nearestPreyDistance;
                foodCorpseDirX[0] = preyDir.x;
                foodCorpseDirY[0] = preyDir.y;
            }
        }
    }
}
