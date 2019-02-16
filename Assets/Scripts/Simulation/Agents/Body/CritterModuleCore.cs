using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // Temporary for debug purposes (view in inspector)
public class CritterModuleCore {

    public int parentID;
    public int inno;

    // **** LOOK INTO::: close in its own class or store in bigger array rather than all individual length-one arrays? 
    public float[] bias;

    public float foodConsumptionRate = 0.00125f;
    
    public float energy = 1f;  // 0-1
    //public float stamina = 1f;
    public float healthHead = 1f;
    public float healthBody = 1f;
    public float healthExternal = 1f;

    public float stomachContentsNorm = 0f;  // 0-1 normalized
    public float stomachCapacity = 1f;  // absolute value in units of (area?)
    //public float stomachContentsDecay = 0f;
    public float stomachContentsPlant = 0f;
    public float stomachContentsMeat = 0f;

    public float debugFoodValue = 0f;
    
    public EggSack nearestEggSackModule;
    public PredatorModule nearestPredatorModule;
    public Agent nearestFriendAgent;
    public Agent nearestEnemyAgent;
    //public Vector2 nearestEggSackPos;

    public float[] isMouthTrigger;
    //public float[] temperature;
    //public float[] pressure;
    public float[] isContact;
    public float[] contactForceX;
    public float[] contactForceY;

    public float[] hitPoints;
    public float[] stamina;
    public float[] energyStored;
    public float[] foodStored;
    
    public float[] mouthFeedEffector;
    public float[] mouthAttackEffector;
    public float[] defendEffector;
    public float[] dashEffector;
    public float[] healEffector;
    
    public float damageBonus;
    public float speedBonus;
    public float healthBonus;
    public float energyBonus;

    public float talentSpecAttackNorm;
    public float talentSpecDefenseNorm;
    public float talentSpecSpeedNorm;
    public float talentSpecUtilityNorm;

    public float dietSpecDecayNorm;
    public float dietSpecPlantNorm;
    public float dietSpecMeatNorm;

    // Diet specialization:
    public float foodEfficiencyPlant;
    public float foodEfficiencyDecay;
    public float foodEfficiencyMeat;

    public bool isDashing = false;
    public int dashFrameCounter = 0;
    public int dashDuration = 20;
    public int dashCooldown = 80;

    public bool isDefending = false;
    public int defendFrameCounter = 0;
    public int defendDuration = 40;
    public int defendCooldown = 120;

	public CritterModuleCore() {

    }

    public void Initialize(CritterModuleCoreGenome genome, Agent agent) {
        
        bias = new float[1];   //0

        isMouthTrigger = new float[1];
        //temperature = new float[1]; // 22
        //pressure = new float[1]; // 23
        isContact = new float[1]; // 24
        contactForceX = new float[1]; // 25
        contactForceY = new float[1]; // 26

        hitPoints = new float[1]; // 27
        stamina = new float[1]; // 28
        energyStored = new float[1];  // 204
        foodStored = new float[1];  // 205
                        
        mouthFeedEffector = new float[1];  // 206
        mouthAttackEffector = new float[1];
        defendEffector = new float[1];
        dashEffector = new float[1];
        healEffector = new float[1];
                                                                          
        energy = 1;
        healthHead = 1f;
        healthBody = 1f;
        healthExternal = 1f;

        bias[0] = 1f;
        
        hitPoints[0] = 1f;
        stamina[0] = 1f;
        energyStored[0] = 1f;
        foodStored[0] = 0f;
        
        parentID = genome.parentID;
        inno = genome.inno;

        float talentSpecTotal = genome.talentSpecializationAttack + genome.talentSpecializationDefense + genome.talentSpecializationSpeed + genome.talentSpecializationUtility;
        talentSpecAttackNorm = genome.talentSpecializationAttack / talentSpecTotal;
        talentSpecDefenseNorm = genome.talentSpecializationDefense / talentSpecTotal;
        talentSpecSpeedNorm = genome.talentSpecializationSpeed / talentSpecTotal;
        talentSpecUtilityNorm = genome.talentSpecializationUtility / talentSpecTotal;
        damageBonus = Mathf.Lerp(0.33f, 2f, talentSpecAttackNorm);
        healthBonus = Mathf.Lerp(0.33f, 2f, talentSpecDefenseNorm);
        speedBonus = Mathf.Lerp(0.33f, 2f, talentSpecSpeedNorm);        
        energyBonus = Mathf.Lerp(0.75f, 1.5f, talentSpecUtilityNorm);
        
        // Diet specialization:
        float dietSpecTotal = genome.dietSpecializationDecay + genome.dietSpecializationPlant + genome.dietSpecializationMeat;        
        dietSpecDecayNorm = genome.dietSpecializationDecay / dietSpecTotal;
        foodEfficiencyDecay = Mathf.Lerp(0.33f, 2f, dietSpecDecayNorm);
        dietSpecPlantNorm = genome.dietSpecializationPlant / dietSpecTotal;
        foodEfficiencyPlant = Mathf.Lerp(0.33f, 2f, dietSpecPlantNorm);
        dietSpecMeatNorm = genome.dietSpecializationMeat / dietSpecTotal;
        foodEfficiencyMeat = Mathf.Lerp(0.33f, 2f, dietSpecMeatNorm);
    }

    public void MapNeuron(NID nid, Neuron neuron) {

        if (inno == nid.moduleID) {
            if (nid.neuronID == 0) {
                neuron.currentValue = bias;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }            
            if (nid.neuronID == 21) {
                neuron.currentValue = isMouthTrigger;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 24) {
                neuron.currentValue = isContact;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 25) {
                neuron.currentValue = contactForceX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 26) {
                neuron.currentValue = contactForceY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 27) {
                neuron.currentValue = hitPoints;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 28) {
                neuron.currentValue = stamina;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 204) {
                neuron.currentValue = energyStored;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 205) {
                neuron.currentValue = foodStored;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }            

            if (nid.neuronID == 206) {
                neuron.currentValue = mouthFeedEffector;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
            if (nid.neuronID == 207) {
                neuron.currentValue = mouthAttackEffector;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
            if (nid.neuronID == 208) {
                neuron.currentValue = defendEffector;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
            if (nid.neuronID == 209) {
                neuron.currentValue = dashEffector;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
            if (nid.neuronID == 210) {
                neuron.currentValue = healEffector;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
        }
    }

    public void Tick() {
        
        //temperature[0] = 0f;
        //pressure[0] = 0f;
        isContact[0] = 0f;
        contactForceX[0] = 0f;
        contactForceY[0] = 0f;
        hitPoints[0] = Mathf.Max(healthBody, 0f);
        //stamina[0] = stamina; // set in Agent.cs
        energyStored[0] = Mathf.Clamp01(energy);  // Mathf.Clamp01(energyRaw / maxEnergyStorage);
        foodStored[0] = stomachContentsNorm; // / stomachCapacity;
        
        if(isDashing) {
            dashFrameCounter++;
            if(dashFrameCounter >= dashDuration + dashCooldown) {
                dashFrameCounter = 0;
                isDashing = false;
            }
        }
        
        if(isDefending) {
            defendFrameCounter++;
            if(defendFrameCounter >= defendDuration + defendCooldown) {
                defendFrameCounter = 0;
                isDefending = false;
            }
        }
    }
}
