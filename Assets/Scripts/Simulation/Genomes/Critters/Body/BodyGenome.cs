using System.Collections.Generic;
using UnityEngine;

public class BodyGenome 
{
    Lookup lookup => Lookup.instance;

    public UnlockedTech unlockedTech;

    public BodyGenome() 
    {
        unlockedTech = lookup.baseInitialAbilities.GetInitialUnlocks();
        FirstTimeInitializeCritterModuleGenomes();
        GenerateInitialRandomBodyGenome();
    }

    public BodyGenome(BodyGenome parentGenome, MutationSettingsInstance mutationSettings)
    {
        unlockedTech = new UnlockedTech(parentGenome.unlockedTech);
        FirstTimeInitializeCritterModuleGenomes();  // * Is this necessary?
        SetToMutatedCopyOfParentGenome(parentGenome, mutationSettings);
    }
    
    //public TestModuleGenome testModuleGenome;
    public CritterModuleAppearanceGenome appearanceGenome;
    public CritterModuleCommunicationGenome communicationGenome;
    public CritterModuleCoreGenome coreGenome;
    public CritterModuleDevelopmentalGenome developmentalGenome;
    public CritterModuleEnvironmentSensorsGenome environmentalGenome;
    public CritterModuleFoodSensorsGenome foodGenome;
    public CritterModuleFriendSensorsGenome friendGenome;
    public CritterModuleMovementGenome movementGenome;
    public CritterModuleThreatSensorsGenome threatGenome;
    
    
    public static float GetBodySizeScore01(BodyGenome genome) {
        // Refactor: 25f is hardcoded approximate! // * WPP: approximate of what? (use a constant or exposed value)
        float normalizedSizeScore = Mathf.Clamp01(((genome.GetFullsizeBoundingBox().x + genome.GetFullsizeBoundingBox().z) / genome.GetFullsizeBoundingBox().y) / 25f); 
        return normalizedSizeScore;
    }
    
    /// Reconstructs all modules as new 
    public void FirstTimeInitializeCritterModuleGenomes() 
    {
        appearanceGenome = new CritterModuleAppearanceGenome();  
        communicationGenome = new CritterModuleCommunicationGenome();
        coreGenome = new CritterModuleCoreGenome();
        developmentalGenome = new CritterModuleDevelopmentalGenome(); 
        environmentalGenome = new CritterModuleEnvironmentSensorsGenome(); 
        foodGenome = new CritterModuleFoodSensorsGenome();
        friendGenome = new CritterModuleFriendSensorsGenome(); 
        movementGenome = new CritterModuleMovementGenome();
        threatGenome = new CritterModuleThreatSensorsGenome(); 
    }
        
    public Vector3 GetFullsizeBoundingBox() {
        float fullLength = coreGenome.creatureBaseLength * (coreGenome.mouthLength + coreGenome.headLength + coreGenome.bodyLength + coreGenome.tailLength);
        float approxAvgRadius = fullLength * coreGenome.creatureAspectRatio;

        Vector3 size = new Vector3(approxAvgRadius, fullLength, approxAvgRadius);
        return size;        
    }

    /// Sets "use" variables based on coin tosses
    public void GenerateInitialRandomBodyGenome() 
    {
        appearanceGenome.InitializeRandom();
        communicationGenome.Initialize(unlockedTech); 
        coreGenome.InitializeRandom();          // WPP: Unclear mapping to abilities.  What (if any) traits are activated isPassive?
        developmentalGenome.Initialize();
        environmentalGenome.InitializeRandom(); // Unclear mapping to abilities
        foodGenome.InitializeRandom();          // Unclear mapping to abilities
        friendGenome.InitializeRandom();        // Unclear mapping to abilities
        movementGenome.InitializeRandom();
        threatGenome.InitializeRandom();        // Unclear mapping to abilities
    }
    
    /// Creates neurons based on state of "use" variables
    public void InitializeBrainGenome(List<NeuronGenome> masterList)
    {
        appearanceGenome.AppendModuleNeuronsToMasterList(masterList);
        communicationGenome.AppendModuleNeuronsToMasterList(masterList);
        coreGenome.AppendModuleNeuronsToMasterList(masterList);
        developmentalGenome.AppendModuleNeuronsToMasterList(masterList);
        environmentalGenome.AppendModuleNeuronsToMasterList(masterList);
        foodGenome.AppendModuleNeuronsToMasterList(masterList); 
        friendGenome.AppendModuleNeuronsToMasterList(masterList);
        movementGenome.AppendModuleNeuronsToMasterList(masterList);
        threatGenome.AppendModuleNeuronsToMasterList(masterList);
    }

    // Mutable by Player
    public void SetToMutatedCopyOfParentGenome(BodyGenome parentBodyGenome, MutationSettingsInstance settings) {   
        // *** Result needs to be fully independent copy and share no references!!!
        // *** OPTIMIZATION:  convert this to use pooling rather than using new memory alloc every mutation

        // Add body-sensor/effector mutations here and Cleanup Brain Genome:
        appearanceGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.appearanceGenome, settings);
        communicationGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.communicationGenome, settings);
        coreGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.coreGenome, settings);
        developmentalGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.developmentalGenome, settings);
        environmentalGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.environmentalGenome, settings);
        foodGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.foodGenome, settings);
        friendGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.friendGenome, settings);        
        movementGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.movementGenome, settings);
        threatGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.threatGenome, settings);
    }
}
