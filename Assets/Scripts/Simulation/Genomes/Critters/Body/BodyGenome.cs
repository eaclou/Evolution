﻿using System.Collections.Generic;
using UnityEngine;

public class BodyGenome 
{
    Lookup lookup => Lookup.instance;
    NeuralMap map => lookup.neuralMap;
    
    // Cached for quick access
    public bool hasComms;
    public bool hasAnimalSensor;
    
    public BodyGenome() 
    {
        unlockedTech = lookup.baseInitialAbilities.GetInitialUnlocks();
        hasComms = HasTech(TechElementId.VocalCords);
        hasAnimalSensor = HasTech(TechElementId.AnimalSensor);
        
        FirstTimeInitializeCritterModuleGenomes();
        GenerateInitialRandomBodyGenome();
    }

    public BodyGenome(BodyGenome parentGenome, MutationSettingsInstance mutationSettings)
    {
        unlockedTech = new UnlockedTech(parentGenome.unlockedTech);
        FirstTimeInitializeCritterModuleGenomes();
        SetToMutatedCopyOfParentGenome(parentGenome, mutationSettings);
    }
    
    public UnlockedTech unlockedTech;
    public bool HasTech(TechElementId id) { return unlockedTech.Contains(id); }
    
    //public TestModuleGenome testModuleGenome;
    public CritterModuleAppearanceGenome appearanceGenome;
    //public CritterModuleCommunicationGenome communicationGenome;
    public CritterModuleCoreGenome coreGenome;
    public CritterModuleEnvironmentSensorsGenome environmentalGenome;
    public CritterModuleFoodSensorsGenome foodGenome;
    //public CritterModuleFriendSensorsGenome friendGenome;
    //public CritterModuleMovementGenome movementGenome;
    //public CritterModuleThreatSensorsGenome threatGenome;
    
    
    public static float GetBodySizeScore01(BodyGenome genome) {
        // Refactor: 25f is hardcoded approximate! // * WPP: approximate of what? (use a constant or exposed value)
        float normalizedSizeScore = Mathf.Clamp01(((genome.GetFullsizeBoundingBox().x + genome.GetFullsizeBoundingBox().z) / genome.GetFullsizeBoundingBox().y) / 25f); 
        return normalizedSizeScore;
    }
    
    /// Reconstructs all modules as new 
    public void FirstTimeInitializeCritterModuleGenomes() 
    {
        appearanceGenome = new CritterModuleAppearanceGenome();  
        //communicationGenome = new CritterModuleCommunicationGenome();
        coreGenome = new CritterModuleCoreGenome();
        environmentalGenome = new CritterModuleEnvironmentSensorsGenome(); 
        foodGenome = new CritterModuleFoodSensorsGenome();
        //friendGenome = new CritterModuleFriendSensorsGenome(); 
        //movementGenome = new CritterModuleMovementGenome();
        //threatGenome = new CritterModuleThreatSensorsGenome(); 
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
        //foodGenome.InitializeRandom();     // Sets unused variables, safe to remove
        //movementGenome.InitializeRandom(); // Sets unused variables, safe to remove
    }
    
    List<NeuronGenome> masterList = new List<NeuronGenome>();
    
    /// Creates neurons based on state of "use" variables
    public void InitializeBrainGenome(List<NeuronGenome> masterList)
    {
        this.masterList = masterList;
    
        foreach (var tech in unlockedTech.values)
            foreach (var template in tech.unlocks)
                masterList.Add(template.GetNeuronGenome());
                
        // WPP: moved non-conditional neurons from modules
        // Core
        AddNeuron("Bias");
        AddNeuron("isMouthTrigger");
        AddNeuron("isContact");
        AddNeuron("contactForceX");
        AddNeuron("contactForceY");
        AddNeuron("hitPoints");
        AddNeuron("stamina");
        AddNeuron("energyStored");
        AddNeuron("foodStored");
        AddNeuron("mouthFeedEffector");
        
        // Movement
        AddNeuron("ownVelX");
        AddNeuron("ownVelY");
        AddNeuron("facingDirX");
        AddNeuron("facingDirY");
        AddNeuron("throttleX");
        AddNeuron("throttleY");
        AddNeuron("dash");

        // * Unable to remove (yet) as this depends on talent specializations
        coreGenome.AppendModuleNeuronsToMasterList(masterList);
    }
    
    void AddNeuron(string name) { masterList.Add(map.GetData(name)); }

    // Mutable by Player
    public void SetToMutatedCopyOfParentGenome(BodyGenome parentBodyGenome, MutationSettingsInstance settings) {   
        // *** Result needs to be fully independent copy and share no references!!!
        // *** OPTIMIZATION:  convert this to use pooling rather than using new memory alloc every mutation

        // Add body-sensor/effector mutations here and Cleanup Brain Genome:
        appearanceGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.appearanceGenome, settings);
        //communicationGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.communicationGenome, settings);
        coreGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.coreGenome, settings);
        environmentalGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.environmentalGenome, settings);
        foodGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.foodGenome, settings);
        //friendGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.friendGenome, settings);        
        //movementGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.movementGenome, settings);
        //threatGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.threatGenome, settings);
    }
}
