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

    //float _energy = 1f;
    public float energy = 1f;
    //{
    //    get => _energy;
    //    set => Mathf.Clamp(_energy + value, 0f, 1000000f); // ***EC  REVISIT!!!!  use integers?
    //}
    
    //public float stamina = 1f;
    public float healthHead = 1f;
    public float healthBody = 1f;
    public float healthExternal = 1f;

    // *** WPP: mixed metaphor -> is this defined as the total contents / capacity
    // or a number that can be incremented independently?
    public float stomachContentsNorm = 0f;
     
    public float stomachCapacity = 1f;  // absolute value in units of (area?)
    
    float _stomachContentsDecay;
    public float stomachContentsDecay
    {
        get => _stomachContentsDecay;
        set => Mathf.Min(_stomachContentsDecay + value, 0f);
    }
    
    float _stomachContentsPlant;
    public float stomachContentsPlant
    {
        get => _stomachContentsPlant;
        set => Mathf.Min(_stomachContentsPlant + value, 0f);
    }
    
    float _stomachContentsMeat;
    public float stomachContentsMeat
    {
        get => _stomachContentsMeat;
        set => Mathf.Min(_stomachContentsMeat + value, 0f);
    }

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

    // Diet specialization: influences efficiency.  All 3 should add up to 1
    // Derived from foodEfficiency
    public float dietSpecDecayNorm;
    public float dietSpecPlantNorm;
    public float dietSpecMeatNorm;

    // Diet specialization: 0-1
    // Mutates occasionally
    public float foodEfficiencyPlant;
    public float foodEfficiencyDecay;
    public float foodEfficiencyMeat;
    
    public float health => healthHead + healthBody + healthExternal;

    public float stomachSpace => stomachCapacity - totalStomachContents;
    public float totalStomachContents => stomachContentsPlant + stomachContentsMeat + stomachContentsDecay;
    public float stomachContentsPercent => totalStomachContents / stomachCapacity;
    public bool stomachEmpty => stomachContentsPercent <= .01f;
    public bool isFull => stomachContentsPercent > 1f;
    
    // *** WPP: replaced Vector math with simpler percent calculation
    // (might be causing an error)
    //public Vector3 foodProportionsVector => foodVector / (totalStomachContents + 0.000001f);
    //Vector3 foodVector => new Vector3(stomachContentsPlant, stomachContentsMeat, stomachContentsDecay);
    public float plantEatenPercent => stomachContentsPlant / (totalStomachContents + 0.000001f);
    public float meatEatenPercent => stomachContentsMeat / (totalStomachContents + 0.000001f);
    public float decayEatenPercent => stomachContentsDecay / (totalStomachContents + 0.000001f);
    
    public float GetEnergyTotal(float plantEfficiency, float meatEfficiency, float decayEfficiency)
    {
        return  dietSpecPlantNorm * plantEfficiency + 
                dietSpecMeatNorm * meatEfficiency + 
                dietSpecDecayNorm + decayEfficiency;
    }
    
    public void Regenerate(float healRate, float energyToHealth)
    {
        if (healthBody >= 1f)
            return;
    
        healthBody += healRate;
        healthHead += healRate;
        healthExternal += healRate;
        energy -= healRate / energyToHealth;
    }

	public CritterModuleCore(CritterModuleCoreGenome genome, Agent agent) {
        Initialize(genome, agent);
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
                                                                          
        energy = 1f;
        
        SetAllHealth(1f);

        bias[0] = 1f;
        
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
        energyStored[0] = Mathf.Clamp01(energy * 0.001f);  // Mathf.Clamp01(energyRaw / maxEnergyStorage);
        foodStored[0] = stomachContentsPercent; // / stomachCapacity;
    }
    
    public void DirectDamageToRandomBodyPart(float damage)
    {
        int rand = Random.Range(0, 3);
        
        switch (rand)
        {
            case 0: healthHead -= damage; break;
            case 1: healthBody -= damage; break;
            default: healthExternal -= damage; break;
        }
    }
    
    public void DistributeDamage(float damage)
    {
        //coreModule.healthHead -= damage * UnityEngine.Random.Range(0f, 1f);
        //coreModule.healthBody -= damage * UnityEngine.Random.Range(0f, 1f);
        //coreModule.healthExternal -= damage * UnityEngine.Random.Range(0f, 1f);
    }
    
    public void SetAllHealth(float value)
    {
        hitPoints[0] = value;
        healthHead = value;
        healthBody = value;
        healthExternal = value;
    }
}
