using System.Collections.Generic;
using UnityEngine;

public class BodyGenome 
{
    Lookup lookup => Lookup.instance;
    NeuralMap map => lookup.neuralMap;
    
    public BodyGenomeData data;

    public BodyGenome() 
    {
        unlockedTech = lookup.baseInitialAbilities.GetInitialUnlocks();
        data = new BodyGenomeData(unlockedTech);

        FirstTimeInitializeCritterModuleGenomes();
        GenerateInitialRandomBodyGenome();
    }

    public BodyGenome(BodyGenome parentGenome, MutationSettingsInstance mutationSettings)
    {
        unlockedTech = new UnlockedTech(parentGenome.unlockedTech);
        data = new BodyGenomeData(unlockedTech);

        FirstTimeInitializeCritterModuleGenomes();
        SetToMutatedCopyOfParentGenome(parentGenome, mutationSettings);
    }
    
    UnlockedTech unlockedTech;
    
    public CritterModuleAppearanceGenome appearanceGenome;
    public CritterModuleCoreGenome coreGenome;
    
    public static float GetBodySizeScore01(BodyGenome genome) {
        // Refactor: 25f is hardcoded approximate! // * WPP: approximate of what? (use a constant or exposed value)
        float normalizedSizeScore = Mathf.Clamp01(((genome.GetFullsizeBoundingBox().x + genome.GetFullsizeBoundingBox().z) / genome.GetFullsizeBoundingBox().y) / 25f); 
        return normalizedSizeScore;
    }
    
    /// Reconstructs all modules as new 
    public void FirstTimeInitializeCritterModuleGenomes() 
    {
        appearanceGenome = new CritterModuleAppearanceGenome();  
        coreGenome = new CritterModuleCoreGenome();
    }
        
    public Vector3 GetFullsizeBoundingBox() {
        float fullLength = coreGenome.creatureBaseLength * (coreGenome.mouthLength + coreGenome.headLength + coreGenome.bodyLength + coreGenome.tailLength);
        float approxAvgRadius = fullLength * coreGenome.creatureAspectRatio;

        Vector3 size = new Vector3(approxAvgRadius, fullLength, approxAvgRadius);
        return size;        
    }
    
    public void GenerateInitialRandomBodyGenome() 
    {
        appearanceGenome.InitializeRandom();
        coreGenome.InitializeRandom();
    }
    
    List<NeuronGenome> masterList = new List<NeuronGenome>();
    
    /// Creates neurons based on state of "use" variables
    public void InitializeBrainGenome(List<NeuronGenome> masterList)
    {
        this.masterList = masterList;
    
        foreach (var tech in unlockedTech.values)
            foreach (var template in tech.unlocks)
                masterList.Add(template.GetNeuronGenome());
                
        AddNonConditionalNeurons();

        // * Unable to remove (yet) as this depends on talent specializations
        //coreGenome.AppendModuleNeuronsToMasterList(masterList);
    }
    
    void AddNonConditionalNeurons()
    {
        AddNeuron("Bias");
        AddNeuron("isMouthTrigger");
        //AddNeuron("isContact");
        //AddNeuron("contactForceX");
        //AddNeuron("contactForceY");
        //AddNeuron("hitPoints");
        //AddNeuron("stamina");
        //AddNeuron("energyStored");
        AddNeuron("foodStored");
        //AddNeuron("ownVelX");
        //AddNeuron("ownVelY");
        AddNeuron("mouthFeedEffector");        
        //AddNeuron("facingDirX");
        //AddNeuron("facingDirY");
        AddNeuron("throttleX");
        AddNeuron("throttleY");
        // AddNeuron("dash");
    }
    
    void AddNeuron(string name) { masterList.Add(map.GetData(name)); }

    // Mutable by Player
    // Add body-sensor/effector mutations here and cleanup Brain Genome:
    void SetToMutatedCopyOfParentGenome(BodyGenome parentBodyGenome, MutationSettingsInstance settings) {
        appearanceGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.appearanceGenome, settings);
        coreGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.coreGenome, settings);
        unlockedTech = unlockedTech.GetMutatedCopy();
    }
}

/// Quick-access properties, calculated by unlocked tech
public class BodyGenomeData
{
    UnlockedTech unlockedTech;

    public bool hasComms;
    public bool hasAnimalSensor;
    public bool useWaterStats;
    public bool useCardinals;
    public bool useDiagonals;
    
    public bool useNutrients;
    public bool useFoodPosition;
    public bool useFoodVelocity;
    public bool useFoodDirection;
    public bool useFoodStats;
    public bool useEggs;
    public bool useCorpse;
    
    public BodyGenomeData(UnlockedTech unlockedTech)
    {
        this.unlockedTech = unlockedTech;
        
        hasComms = HasTech(TechElementId.VocalCords);
        hasAnimalSensor = HasTech(TechElementId.AnimalSensor);
        useWaterStats = HasTech(TechElementId.WaterSensor);
        useCardinals = HasTech(TechElementId.SensoryGanglia);
        useDiagonals = HasTech(TechElementId.SensoryGanglia); 
        useNutrients = HasTech(TechElementId.SensoryGanglia);
        useFoodPosition = HasTech(TechElementId.FoodSensor);
        useFoodVelocity = HasTech(TechElementId.FoodSensor);
        useFoodDirection = HasTech(TechElementId.FoodSensor);
        useFoodStats = HasTech(TechElementId.Eyes);
        useEggs = HasTech(TechElementId.EggSensor);
        useCorpse = HasTech(TechElementId.CorpseSensor);
    }
    
    public bool HasTech(TechElementId id) { return unlockedTech.Contains(id); }
}
