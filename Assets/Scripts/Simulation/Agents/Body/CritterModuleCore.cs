﻿using UnityEngine;

[System.Serializable] // Temporary for debug purposes (view in inspector)
public class CritterModuleCore {

    public int parentID;
    public int inno;

    // **** LOOK INTO::: close in its own class or store in bigger array rather than all individual length-one arrays? 
    public float[] bias;

    public float foodConsumptionRate = 0.00125f;

    //float _energy = 1f;
    public float energy = 1f;
    //{
    //    get => _energy;
    //    set => Mathf.Clamp(_energy + value, 0f, 1000000f); // ***EC  REVISIT!!!!  use integers?
    //}

    public float health = 1f;
    //public float stamina = 1f;
    //public float healthHead = 1f;
    //public float healthBody = 1f;
    //public float healthExternal = 1f;

    // *** WPP: mixed metaphor -> is this defined as the total contents / capacity
    // or a number that can be incremented independently?
    //public float stomachContentsTotal01 = 0f;
     
    public float stomachCapacity = 1f;  // absolute value in units of (area?)
    public float stomachContentsDecay;
    public float stomachContentsPlant;
    public float stomachContentsMeat;

    //public float debugFoodValue = 0f;
    
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

    CritterModuleCoreGenome genome;

    public float talentSpecAttackNorm => genome.talentSpecAttackNorm;
    public float talentSpecDefenseNorm => genome.talentSpecDefenseNorm;
    public float talentSpecSpeedNorm => genome.talentSpecSpeedNorm;
    public float talentSpecUtilityNorm => genome.talentSpecUtilityNorm;

    // Diet specialization: influences efficiency.  All 3 should add up to 1
    // Derived from foodEfficiency
    //public float dietSpecDecayNorm;
    //public float dietSpecPlantNorm;
    //public float dietSpecMeatNorm;

    // Diet specialization: 0-1
    // Mutates occasionally
    public float digestEfficiencyPlant => genome.dietSpecializationPlant;
    public float digestEfficiencyDecay => genome.dietSpecializationDecay;
    public float digestEfficiencyMeat => genome.dietSpecializationMeat;
    
    //public float health => healthHead + healthBody + healthExternal;

    public float stomachSpace => stomachCapacity - totalStomachContents;
    public float totalStomachContents => stomachContentsPlant + stomachContentsMeat + stomachContentsDecay;
    public float stomachContentsPercent => totalStomachContents / stomachCapacity;
    public bool stomachEmpty => stomachContentsPercent <= 0f;
    public bool isFull => stomachContentsPercent >= 1f;
    
    public float plantEatenPercent => Mathf.Clamp01(stomachContentsPlant / (totalStomachContents + 0.0000001f));
    public float meatEatenPercent => Mathf.Clamp01(stomachContentsMeat / (totalStomachContents + 0.0000001f));
    public float decayEatenPercent => Mathf.Clamp01(stomachContentsDecay / (totalStomachContents + 0.0000001f));
    
    public float GetEnergyCreatedFromDigestion(float plantMassDigested, float meatMassDigested, float decayMassDigested)
    {
        //Debug.Log("GetEnergyCreatedFromDigestion: " + plantMassDigested + ", " + meatMassDigested + ", " + decayMassDigested);
        return  plantMassDigested * digestEfficiencyPlant + 
                meatMassDigested * digestEfficiencyMeat + 
                decayMassDigested * digestEfficiencyDecay;
    }
    
    public void Regenerate(float healEnergyUsageRate, float energyToHealth)
    {
        if (health >= 1f)
            return;
    
        health += healEnergyUsageRate * energyToHealth;
        energy -= healEnergyUsageRate;
    }

	public CritterModuleCore(CritterModuleCoreGenome genome) {
        Initialize(genome);
    }

    public void Initialize(CritterModuleCoreGenome genome) {
        this.genome = genome;
        
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
                                                                          
        energy = 1f;
        
        SetAllHealth(1f);

        bias[0] = 1f;
        
        stamina[0] = 1f;
        energyStored[0] = 1f;
        foodStored[0] = 0f;
        
        parentID = genome.parentID;
        inno = genome.inno;

        // * WPP: expose magic numbers
        damageBonus = Mathf.Lerp(0.33f, 2f, talentSpecAttackNorm);
        healthBonus = Mathf.Lerp(0.33f, 2f, talentSpecDefenseNorm);
        speedBonus = Mathf.Lerp(0.33f, 2f, talentSpecSpeedNorm);        
        energyBonus = Mathf.Lerp(0.75f, 1.5f, talentSpecUtilityNorm);
    }

    // * WPP: move to Neuron, 
    // use lookup table to set NeuronType and switch to set currentValue
    public void MapNeuron(NID nid, Neuron neuron) {
        if (inno != nid.moduleID) return;

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

    public void Tick() {
        //temperature[0] = 0f;
        //pressure[0] = 0f;
        isContact[0] = 0f;
        contactForceX[0] = 0f;
        contactForceY[0] = 0f;
        hitPoints[0] = Mathf.Max(health, 0f);
        //stamina[0] = stamina; // set in Agent.cs
        energyStored[0] = Mathf.Clamp01(energy * 0.001f);  // Mathf.Clamp01(energyRaw / maxEnergyStorage); //***EAC will need to be changed once energy is adjusted
        foodStored[0] = stomachContentsPercent; // / stomachCapacity;
    }

    public void DirectDamage(float damage) {
        health -= damage;
    }

    public void SetAllHealth(float value) {
        health = value;
    }
}

/*public void DirectDamageToRandomBodyPart(float damage)
{
    int rand = Random.Range(0, 3);
    
    switch (rand)
    {
        case 0: healthHead -= damage; break;
        case 1: healthBody -= damage; break;
        default: healthExternal -= damage; break;
    }
}*/

/*public void DistributeDamage(float damage)
{
    coreModule.healthHead -= damage * UnityEngine.Random.Range(0f, 1f);
    coreModule.healthBody -= damage * UnityEngine.Random.Range(0f, 1f);
    coreModule.healthExternal -= damage * UnityEngine.Random.Range(0f, 1f);
}*/

