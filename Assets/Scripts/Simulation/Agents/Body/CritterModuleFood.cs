using System.Collections;
using System.Collections.Generic;
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
            foodPlantPosX = new float[1];
            foodPlantPosY = new float[1];
            foodPlantDistance = new float[1];
        //}
        //if(genome.useVel) {
            foodPlantVelX = new float[1];
            foodPlantVelY = new float[1];
        //}
        //if(genome.useDir) {
            foodPlantDirX = new float[1];
            foodPlantDirY = new float[1];
        //}
        //if(genome.useStats) {
            foodPlantQuality = new float[1];
            foodPlantRelSize = new float[1];
        //}


        foodAnimalPosX = new float[1];
        foodAnimalPosY = new float[1];
        foodAnimalDistance = new float[1];
        foodAnimalVelX = new float[1];
        foodAnimalVelY = new float[1];
        foodAnimalDirX = new float[1];
        foodAnimalDirY = new float[1];
        foodAnimalQuality = new float[1];
        foodAnimalRelSize = new float[1];
        

        foodEggDistance = new float[1];
        foodEggDirX = new float[1];
        foodEggDirY = new float[1];

        foodCorpseDistance = new float[1];
        foodCorpseDirX = new float[1];
        foodCorpseDirY = new float[1];

        sensorRange = 25f; // TEMP HARDCODED *****
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
        nearestFoodParticleIndex = simManager.vegetationManager.closestAlgaeParticlesDataArray[agent.index].index;
        nearestFoodParticlePos = simManager.vegetationManager.closestAlgaeParticlesDataArray[agent.index].worldPos - 
                                    new Vector2(simManager.agentsArray[agent.index].bodyRigidbody.transform.position.x,
                                    simManager.agentsArray[agent.index].bodyRigidbody.transform.position.y);

        nearestFoodParticleAmount = simManager.vegetationManager.closestAlgaeParticlesDataArray[agent.index].foodAmount;
        Vector2 critterToFoodParticle = simManager.vegetationManager.closestAlgaeParticlesDataArray[agent.index].worldPos - agent.ownPos;
        float distToNearestFoodParticle = critterToFoodParticle.magnitude;
        Vector2 foodParticleDir = critterToFoodParticle.normalized;
        float nearestFoodParticleDistance = Mathf.Clamp01((sensorRange - critterToFoodParticle.magnitude) / sensorRange); // inverted dist(proximity) 0-1

        foodPlantPosX[0] = Mathf.Clamp(critterToFoodParticle.x / sensorRange, -1f, 1f);
        foodPlantPosY[0] = Mathf.Clamp(critterToFoodParticle.y / sensorRange, -1f, 1f);
        foodPlantDistance[0] = nearestFoodParticleDistance;
        foodPlantDirX[0] = foodParticleDir.x;
        foodPlantDirY[0] = foodParticleDir.y;
        foodPlantQuality[0] = 1f; // *** temp until particles can decay naturally
        foodPlantRelSize[0] = nearestFoodParticleAmount;


        nearestAnimalParticleIndex = simManager.vegetationManager.closestAnimalParticlesDataArray[agent.index].index;
        nearestAnimalParticlePos = simManager.vegetationManager.closestAnimalParticlesDataArray[agent.index].worldPos - simManager.agentsArray[agent.index].bodyRigidbody.transform.position; // 
                                    //new Vector2(simManager.agentsArray[agent.index].bodyRigidbody.transform.position.x,
                                    //simManager.agentsArray[agent.index].bodyRigidbody.transform.position.y);

        nearestAnimalParticleAmount = simManager.vegetationManager.closestAnimalParticlesDataArray[agent.index].nutrientContent;
        Vector2 critterToAnimalParticle = new Vector2(nearestAnimalParticlePos.x, nearestAnimalParticlePos.y); // - agent.ownPos;
        float distToNearestAnimalParticle = critterToAnimalParticle.magnitude;
        Vector2 animalParticleDir = critterToAnimalParticle.normalized;
        float nearestAnimalParticleDistance = Mathf.Clamp01((sensorRange - critterToAnimalParticle.magnitude) / sensorRange); // inverted dist(proximity) 0-1

        foodAnimalPosX[0] = Mathf.Clamp(critterToAnimalParticle.x / sensorRange, -1f, 1f);
        foodAnimalPosY[0] = Mathf.Clamp(critterToAnimalParticle.y / sensorRange, -1f, 1f);
        foodAnimalDistance[0] = nearestAnimalParticleDistance;
        foodAnimalDirX[0] = animalParticleDir.x;
        foodAnimalDirY[0] = animalParticleDir.y;
        foodAnimalQuality[0] = 1f; // *** temp until particles can decay naturally
        foodAnimalRelSize[0] = nearestAnimalParticleAmount;

        //dist = nearestFoodParticleDistance;
        //quality = 1f;
        //relSize = nearestFoodParticleAmount;
        // no food particle Velocity yet! ***** nned to store in gpu struct?
        //}
        //else {
        //    if(foodPreferenceOrder[0] == 1) {  // egg

        // EGGS:::::
        if(agent.coreModule.nearestEggSackModule != null) {
            Vector2 eggSackPos = new Vector2(agent.coreModule.nearestEggSackModule.rigidbodyRef.position.x - agent.ownPos.x, agent.coreModule.nearestEggSackModule.rigidbodyRef.position.y - agent.ownPos.y);
            Vector2 eggSackDir = eggSackPos.normalized;
            float nearestEggSackDistance = Mathf.Clamp01((sensorRange - eggSackPos.magnitude) / sensorRange);

            foodEggDistance[0] = nearestEggSackDistance;
            foodEggDirX[0] = eggSackDir.x;
            foodEggDirY[0] = eggSackDir.y;

            //foodPos = new Vector2(Mathf.Clamp(eggSackPos.x / sensorRange, -1f, 1f), Mathf.Clamp(eggSackPos.y / sensorRange, -1f, 1f));
            //dist = nearestEggSackDistance;
            // NO VEL YET!!! **** FIX!!!
            //foodDir = eggSackDir;
            //quality = 1f; // **** IMPLEMENT! (decay of eggSack?)
            //relSize = agent.coreModule.nearestEggSackModule.foodAmount;
        }
        else {

        }

        // CORPSE FOOD:::::
            //}
            //else {  // creatures
        if(agent.coreModule.nearestEnemyAgent != null) {
            if(agent.curLifeStage == Agent.AgentLifeStage.Dead) {
                Vector2 preyPos = new Vector2(agent.coreModule.nearestEnemyAgent.bodyRigidbody.position.x - agent.ownPos.x, agent.coreModule.nearestEnemyAgent.bodyRigidbody.position.y - agent.ownPos.y);
                Vector2 preyDir = preyPos.normalized;
                float nearestPreyDistance = Mathf.Clamp01((sensorRange - preyPos.magnitude) / sensorRange);  

                foodCorpseDistance[0] = nearestPreyDistance;
                foodCorpseDirX[0] = preyDir.x;
                foodCorpseDirY[0] = preyDir.y;
            }
            else {

            }
            /*Vector2 preyPos = new Vector2(agent.coreModule.nearestEnemyAgent.bodyRigidbody.position.x - agent.ownPos.x, agent.coreModule.nearestEnemyAgent.bodyRigidbody.position.y - agent.ownPos.y);
            Vector2 preyDir = preyPos.normalized;
            float nearestPreyDistance = Mathf.Clamp01((sensorRange - preyPos.magnitude) / sensorRange);     

            foodPos = new Vector2(Mathf.Clamp(preyPos.x / sensorRange, -1f, 1f), Mathf.Clamp(preyPos.y / sensorRange, -1f, 1f));
            dist = nearestPreyDistance;
            // NO VEL YET!!! **** FIX!!!
            foodDir = preyDir;
            quality = 1f; // **** IMPLEMENT! (decay of eggSack?)
            relSize = (agent.coreModule.nearestEnemyAgent.currentBoundingBoxSize.x * agent.coreModule.nearestEnemyAgent.currentBoundingBoxSize.y) / (agent.currentBoundingBoxSize.x * agent.currentBoundingBoxSize.y);
            */
        }
        else {

        }
            //}
        //}
        
        /*if(genome.usePos) {
            foodPlantPosX[0] = foodPos.x;
            foodPlantPosY[0] = foodPos.y;
            foodPlantDistance[0] = dist;
        }
        if(genome.useVel) {

        }
        if(genome.useDir) {
            foodPlantDirX[0] = foodDir.x;
            foodPlantDirY[0] = foodDir.y;
        }
        if(genome.useStats) {
            foodPlantQuality[0] = quality; // *** temp until particles can decay naturally
            foodPlantRelSize[0] = relSize;
        }*/
    }
}
