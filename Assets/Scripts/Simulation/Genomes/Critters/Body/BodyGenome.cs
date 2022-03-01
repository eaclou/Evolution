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
        AddNeuron("mouthFeedEffector");  // This only works with Collider2Ds, won't detect Plants or Microbes
        AddNeuron("throttleX");
        AddNeuron("throttleY");
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
        
        hasComms = HasTech(TechElementId.OutComms01);
        hasAnimalSensor = HasTech(TechElementId.Social);
        useWaterStats = HasTech(TechElementId.Water);
        useCardinals = HasTech(TechElementId.CorpseDist);
        useDiagonals = HasTech(TechElementId.CorpseVel); 
        useNutrients = HasTech(TechElementId.Nutrients);
        useFoodPosition = HasTech(TechElementId.FoodSensors);
        useFoodVelocity = HasTech(TechElementId.FoodSensors);
        useFoodDirection = HasTech(TechElementId.FoodSensors); //***EAC CHANGE THIS!!
        useFoodStats = HasTech(TechElementId.FoodSensors);
        useEggs = HasTech(TechElementId.Predation);
        useCorpse = HasTech(TechElementId.CorpseDir);
    }
    
    public bool HasTech(TechElementId id) { return unlockedTech.Contains(id); }
}
