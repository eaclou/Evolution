using System;
using UnityEngine;

[Serializable]
public class CritterModuleFood : IBrainModule
{
    SimulationManager simulation => SimulationManager.instance;
    VegetationManager vegetation => simulation.vegetationManager;
    ZooplanktonManager microbes => simulation.zooplanktonManager;

    public BrainModuleID moduleID => BrainModuleID.FoodSensors;

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
    
    public BodyGenomeData genome;

    public CritterModuleFood(BodyGenomeData genome) {
        Initialize(genome);
    }

    public void Initialize(BodyGenomeData genome) {
        this.genome = genome;
        
        // * WPP: Vulnerable to index out of range errors (such as in UI)
        // consider initializing in variable declarations and leaving values at zero
        if(genome.useNutrients) {
            nutrientDensity = new float[1];
            nutrientGradX = new float[1];
            nutrientGradY = new float[1];
        }
        if(genome.useFoodPosition) {
            foodPlantPosX = new float[1];
            foodPlantPosY = new float[1];
            
            foodAnimalPosX = new float[1];
            foodAnimalPosY = new float[1];
        }
        if(genome.useFoodVelocity) {
            foodPlantVelX = new float[1];
            foodPlantVelY = new float[1];
            foodAnimalVelX = new float[1];
            foodAnimalVelY = new float[1];
        }
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
        
        if(genome.useFoodStats) {
            foodPlantQuality = new float[1];
            foodPlantRelSize = new float[1];
            foodAnimalQuality = new float[1];
            foodAnimalRelSize = new float[1];
        }
        
        sensorRange = 16f; // TEMP HARDCODED *****
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
    }
    
    public void GetNeuralValue(MetaNeuron data, Neuron neuron)
    {
        if (moduleID != data.moduleID) return;
        neuron.currentValues = (float[])GetType().GetField(neuron.name).GetValue(this);
    }

    // WPP: using reflection
    /*float[] GetNeuralValue(string neuronID)
    {
        switch(neuronID)
        {
            case "nutrientDensity": return nutrientDensity;
            case "nutrientGradX": return nutrientGradX;
            case "nutrientGradY": return nutrientGradY;
            case "foodPosX": return foodPlantPosX;
            case "foodPosY": return foodPlantPosY;
            case "foodDistance": return foodPlantDistance;
            case "foodVelX": return foodPlantVelX;
            case "foodVelY": return foodPlantVelY;
            case "foodDirX": return foodPlantDirX;
            case "foodDirY": return foodPlantDirY;
            case "foodQuality": return foodPlantQuality;
            case "foodRelSize": return foodPlantRelSize;
            case "distanceToEgg": return foodEggDistance;
            case "eggDirX": return foodEggDirX;
            case "eggDirY": return foodEggDirY;
            case "distanceToCorpse": return foodCorpseDistance;
            case "corpseDirX": return foodCorpseDirX;
            case "corpseDirY": return foodCorpseDirY;
            case "animalPosX": return foodAnimalPosX;
            case "animalPosY": return foodAnimalPosY;
            case "animalDistance": return foodAnimalDistance;
            case "animalVelX": return foodAnimalVelX;
            case "animalVelY": return foodAnimalVelY;
            case "animalDirX": return foodAnimalDirX;
            case "animalDirY": return foodAnimalDirY;
            case "animalQuality": return foodAnimalQuality;
            case "animalRelSize": return foodAnimalRelSize;
            default: return null;
        }
    }*/

    public void Tick(Agent agent) {
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
        nearestFoodParticleIndex = vegetation.closestPlantParticlesDataArray[agent.index].index;
        nearestFoodParticlePos = vegetation.closestPlantParticlesDataArray[agent.index].worldPos - 
                                 new Vector2(simulation.agents[agent.index].bodyRigidbody.transform.position.x,
                                     simulation.agents[agent.index].bodyRigidbody.transform.position.y);

        nearestFoodParticleAmount = vegetation.closestPlantParticlesDataArray[agent.index].biomass;
        Vector2 critterToFoodParticle = vegetation.closestPlantParticlesDataArray[agent.index].worldPos - agent.ownPos;
        float distToNearestFoodParticle = critterToFoodParticle.magnitude;
        Vector2 foodParticleDir = critterToFoodParticle.normalized;
        float nearestFoodParticleDistance = Mathf.Clamp01((sensorRange - critterToFoodParticle.magnitude) / sensorRange); // inverted dist(proximity) 0-1


        //ANIMAL ZOOPLANKTON:
        nearestAnimalParticleIndex = microbes.closestAnimalParticlesDataArray[agent.index].index;
        nearestAnimalParticlePos = microbes.closestAnimalParticlesDataArray[agent.index].worldPos - simulation.agents[agent.index].bodyRigidbody.transform.position;

        nearestAnimalParticleAmount = microbes.closestAnimalParticlesDataArray[agent.index].biomass;
        Vector2 critterToAnimalParticle = new Vector2(nearestAnimalParticlePos.x, nearestAnimalParticlePos.y); // - agent.ownPos;
        float distToNearestAnimalParticle = critterToAnimalParticle.magnitude;
        Vector2 animalParticleDir = critterToAnimalParticle.normalized;
        float nearestAnimalParticleDistance = Mathf.Clamp01((sensorRange - critterToAnimalParticle.magnitude) / sensorRange); // inverted dist(proximity) 0-1
        
        if(genome.useFoodStats) {
            foodPlantQuality[0] = 1f; // *** temp until particles can decay naturally
            foodPlantRelSize[0] = nearestFoodParticleAmount;

            foodAnimalQuality[0] = 1f; // *** temp until particles can decay naturally
            foodAnimalRelSize[0] = nearestAnimalParticleAmount;
        }
        if(genome.useFoodPosition) {
            foodPlantPosX[0] = Mathf.Clamp(critterToFoodParticle.x / sensorRange, -1f, 1f);
            foodPlantPosY[0] = Mathf.Clamp(critterToFoodParticle.y / sensorRange, -1f, 1f);

            foodAnimalPosX[0] = Mathf.Clamp(critterToAnimalParticle.x / sensorRange, -1f, 1f);
            foodAnimalPosY[0] = Mathf.Clamp(critterToAnimalParticle.y / sensorRange, -1f, 1f);
        }
        if(genome.useFoodVelocity) { }
        if(genome.useFoodDirection) {
            foodPlantDirX[0] = foodParticleDir.x;
            foodPlantDirY[0] = foodParticleDir.y;
            foodPlantDistance[0] = nearestFoodParticleDistance;

            foodAnimalDistance[0] = nearestAnimalParticleDistance;
            foodAnimalDirX[0] = animalParticleDir.x;
            foodAnimalDirY[0] = animalParticleDir.y;
        }
        
        // * WPP: move to functions
        // EGGS
        if (agent.coreModule.nearestEggSackModule) {
            Vector2 eggSackPos = new Vector2(agent.coreModule.nearestEggSackModule.rigidbodyRef.position.x - agent.ownPos.x, agent.coreModule.nearestEggSackModule.rigidbodyRef.position.y - agent.ownPos.y);
            Vector2 eggSackDir = eggSackPos.normalized;
            float nearestEggSackDistance = Mathf.Clamp01((sensorRange - eggSackPos.magnitude) / sensorRange);

            if(genome.useFoodDirection) {
                foodEggDistance[0] = nearestEggSackDistance;
                foodEggDirX[0] = eggSackDir.x;
                foodEggDirY[0] = eggSackDir.y;
            }
        }

        // CORPSE FOOD          
        if (agent.coreModule.nearestEnemyAgent && agent.isDead) {
            Vector3 preyPos3 = agent.coreModule.nearestEnemyAgent.bodyRigidbody.position;
            Vector2 preyPos = new Vector2(preyPos3.x - agent.ownPos.x, preyPos3.y - agent.ownPos.y);
            Vector2 preyDir = preyPos.normalized;
            float nearestPreyDistance = Mathf.Clamp01((sensorRange - preyPos.magnitude) / sensorRange);

            if (genome.useFoodDirection) {
                foodCorpseDistance[0] = nearestPreyDistance;
                foodCorpseDirX[0] = preyDir.x;
                foodCorpseDirY[0] = preyDir.y;
            }
        }
    }
}
