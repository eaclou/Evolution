using UnityEngine;

[System.Serializable] // Temporary for debug purposes (view in inspector)
public class CritterModuleCore : IBrainModule
{
    SettingsManager settings => SettingsManager.instance;

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
    public PredatorModule nearestPredatorModule;    // * WPP: not used
    public Agent nearestFriendAgent;
    public Agent nearestEnemyAgent;
    //public Vector2 nearestEggSackPos;

    public float[] _mouthTriggerOutputs;
    public float[] mouthTriggerOutputs
    {
        get
        {
            if (_mouthTriggerOutputs == null)
                _mouthTriggerOutputs = new float[1];
            
            return _mouthTriggerOutputs;
        }
    }
    public bool objectInRangeOfMouth { set => mouthTriggerOutputs[0] = value ? 1f : 0f; }


    // Need to use arrays instead of floats here so that they are reference type
    // and changes that occur in neuron will also be reflected here 
    //public float[] temperature;
    //public float[] pressure;
    private int contactSensorDuration = 15;
    private int contactFrameCounter = 0;
    private bool isContactEvent = false;
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
     
    public void requestContactEvent(float gradX, float gradY) {
        isContactEvent = true;
        contactFrameCounter = 0;
        isContact[0] = 1f;
        contactForceX[0] = gradX;
        contactForceY[0] = gradY;
    }
    private void EndContactEvent() {
        isContact[0] = 0f;
        contactForceX[0] = 0f;
        contactForceY[0] = 0f;
        isContactEvent = false;
        contactFrameCounter = 0;
    }
    // *** Remember to Re-Implement dietary specialization!!! ****
    // How much of what was eaten is actually digested this frame (absolute value)
    public void TickDigestion(float totalMassDigested)
    {
        float digestedPlantMass = totalMassDigested * plantEatenPercent;
        float digestedMeatMass = totalMassDigested * meatEatenPercent;
        float digestedDecayMass = totalMassDigested * decayEatenPercent; 
        
        stomachContentsPlant -= digestedPlantMass;        
        stomachContentsMeat -= digestedMeatMass;
        stomachContentsDecay -= digestedDecayMass;
        
        energy += GetEnergyCreatedFromDigestion(digestedPlantMass, digestedMeatMass, digestedDecayMass) * settings.agentSettings._DigestionEnergyEfficiency;
    }
    
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
        
        bias = new float[1];

        //temperature = new float[1];
        //pressure = new float[1];
        isContact = new float[1];
        contactForceX = new float[1];
        contactForceY = new float[1];

        hitPoints = new float[1];
        stamina = new float[1];
        energyStored = new float[1];
        foodStored = new float[1];
                        
        mouthFeedEffector = new float[1];
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
        
        // * WPP: expose magic numbers
        damageBonus = Mathf.Lerp(0.33f, 2f, talentSpecAttackNorm);
        healthBonus = Mathf.Lerp(0.33f, 2f, talentSpecDefenseNorm);
        speedBonus = Mathf.Lerp(0.33f, 2f, talentSpecSpeedNorm);        
        energyBonus = Mathf.Lerp(0.75f, 1.5f, talentSpecUtilityNorm);
    }
    
    public void SetNeuralValue(Neuron neuron)
    {
        // Example error checking on name, apply to other modules if needed
        var field = GetType().GetField(neuron.name);
        if (field == null) Debug.LogError($"Cannot find matching field for {neuron.name} in CritterModuleCore");
        else neuron.currentValues = (float[])field.GetValue(this);
    }

    public void Tick() 
    {
        //temperature[0] = 0f;
        //pressure[0] = 0f;
        if(isContactEvent) {
            if(contactFrameCounter >= contactSensorDuration) {
                //end
                EndContactEvent();
            }
            else {

            }
            contactFrameCounter++;            
        }
        else {

        }
        
        hitPoints[0] = Mathf.Max(health, 0f);
        //stamina[0] = stamina; // set in Agent.cs
        energyStored[0] = Mathf.Clamp01(energy * 0.001f);  // Mathf.Clamp01(energyRaw / maxEnergyStorage); //***EAC will need to be changed once energy is adjusted
        foodStored[0] = stomachContentsPercent; // / stomachCapacity;
    }

    public void DirectDamage(float damage) { health -= damage; }

    public void SetAllHealth(float value) { health = value; }
}

#region old code
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
#endregion

