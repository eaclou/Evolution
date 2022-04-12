using System;
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
    
    public UnlockedTech unlockedTech { get; private set; }
    
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

    /// Creates neurons based on state of "use" variables
    public List<Neuron> GetUnlockedNeurons(int priorCount)
    {
        var result = new List<Neuron>();
        
        foreach (var tech in unlockedTech.values)
            foreach (var template in tech.unlocks)
                result.Add(template.GetNeuron(priorCount + result.Count));

        result.Add(map.GetData("bias", priorCount + result.Count)); 
        result.Add(map.GetData("_mouthTriggerOutputs", priorCount + result.Count));
         
        return result;
    }

    /// Mutable by Player
    /// Add body-sensor/effector mutations here and cleanup Brain Genome
    void SetToMutatedCopyOfParentGenome(BodyGenome parent, MutationSettingsInstance settings) 
    {
        appearanceGenome.SetToMutatedCopyOfParentGenome(parent.appearanceGenome, settings);
        coreGenome.SetToMutatedCopyOfParentGenome(parent.coreGenome, settings);
        unlockedTech = unlockedTech.GetMutatedCopy();
        newlyUnlockedNeuronInfo = GetNewlyUnlockedNeurons(parent);
    }
    
    public List<MetaNeuron> newlyUnlockedNeuronInfo = new List<MetaNeuron>();

    List<MetaNeuron> GetNewlyUnlockedNeurons(BodyGenome parent)
    {
        var newTech = new List<TechElement>();
        foreach (var tech in unlockedTech.values)
            if (!parent.unlockedTech.Contains(tech))
                newTech.Add(tech);
                
        var newMetaNeurons = new List<MetaNeuron>();
        foreach (var tech in newTech)
            foreach (var unlock in tech.unlocks)
                newMetaNeurons.Add(unlock);
        
        return newMetaNeurons;
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
        useFoodPosition = HasTech(TechElementId.MicrobesDir);
        useFoodVelocity = HasTech(TechElementId.PlantsVel);
        useFoodDirection = HasTech(TechElementId.PlantsDir); //***EAC CHANGE THIS!!
        useFoodStats = HasTech(TechElementId.FoodSensors);
        useEggs = HasTech(TechElementId.EggDir);
        useCorpse = HasTech(TechElementId.CorpseDir);
    }
    
    public bool HasTech(TechElementId id) { return unlockedTech.Contains(id); }
}
