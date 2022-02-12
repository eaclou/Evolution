//using System;
using System.Collections.Generic;
using UnityEngine;

//[Serializable]
public class BodyGenome 
{
    public BodyGenome() 
    {
        FirstTimeInitializeCritterModuleGenomes();
        GenerateInitialRandomBodyGenome();
    }

    public BodyGenome(BodyGenome parentGenome, MutationSettingsInstance mutationSettings)
    {
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

    //public Vector3 fullsizeBoundingBox;   // Z = forward, Y = up

    public static float GetBodySizeScore01(BodyGenome genome) {
        // Refactor: 25f is hardcoded approximate! // * WPP: approximate of what? (use a constant or exposed value)
        float normalizedSizeScore = Mathf.Clamp01(((genome.GetFullsizeBoundingBox().x + genome.GetFullsizeBoundingBox().z) / genome.GetFullsizeBoundingBox().y) / 25f); 
        return normalizedSizeScore;
    }
    
    /// Re-constructs all modules as new 
    public void FirstTimeInitializeCritterModuleGenomes() 
    {
        appearanceGenome = new CritterModuleAppearanceGenome(0);            // 0
        communicationGenome = new CritterModuleCommunicationGenome(0);      // 1
        coreGenome = new CritterModuleCoreGenome(0);                        // 2
        developmentalGenome = new CritterModuleDevelopmentalGenome(0);      // 3
        environmentalGenome = new CritterModuleEnvironmentSensorsGenome(0); // 4
        foodGenome = new CritterModuleFoodSensorsGenome(0);                 // 5
        friendGenome = new CritterModuleFriendSensorsGenome(0);             // 6
        movementGenome = new CritterModuleMovementGenome(0);                // 7
        threatGenome = new CritterModuleThreatSensorsGenome(0);             // 8
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
        appearanceGenome.GenerateRandomInitialGenome();
        communicationGenome.GenerateRandomInitialGenome(); // useComms
        coreGenome.GenerateRandomInitialGenome();
        developmentalGenome.GenerateRandomInitialGenome();
        environmentalGenome.GenerateRandomInitialGenome(); // useCardinals, useDiagonals, useWaterStats
        foodGenome.GenerateRandomInitialGenome();          // usePos, useVel, useDir, useStats, useNutrients, useEggs, useCorpse
        friendGenome.GenerateRandomInitialGenome();        // usePos, useVel, useDir
        movementGenome.GenerateRandomInitialGenome();
        threatGenome.GenerateRandomInitialGenome();        // usePos, useVel, useDir, useStats
    }
    
    /// Creates neurons based on state of "use" variables
    public void InitializeBrainGenome(List<NeuronGenome> masterList)
    {
        appearanceGenome.AppendModuleNeuronsToMasterList(masterList);
        communicationGenome.AppendModuleNeuronsToMasterList(masterList);  // useComms
        coreGenome.AppendModuleNeuronsToMasterList(masterList);           // talentSpec[s] > 0.2f
        developmentalGenome.AppendModuleNeuronsToMasterList(masterList);
        environmentalGenome.AppendModuleNeuronsToMasterList(masterList);  // useWaterStats
        foodGenome.AppendModuleNeuronsToMasterList(masterList);           // usePos, useVel, useDir, useStats, useNutrients, useEggs, useCorpse
        friendGenome.AppendModuleNeuronsToMasterList(masterList);         // usePos, useVel, useDir
        movementGenome.AppendModuleNeuronsToMasterList(masterList);
        threatGenome.AppendModuleNeuronsToMasterList(masterList);         // usePos, useVel, useDir, useStats
    }
    
    // WPP: ref removed from arguments, Lists are pass-by-reference
    // Go through each of the Body's Modules and add In/Out neurons based on module upgrades and settings:
    /*public void InitializeBrainGenome(List<NeuronGenome> neuronList) {
        appearanceGenome.AppendModuleNeuronsToMasterList(neuronList);
        communicationGenome.AppendModuleNeuronsToMasterList(neuronList);
        coreGenome.AppendModuleNeuronsToMasterList(neuronList);
        developmentalGenome.AppendModuleNeuronsToMasterList(neuronList);
        environmentalGenome.AppendModuleNeuronsToMasterList(neuronList);
        foodGenome.AppendModuleNeuronsToMasterList(neuronList);
        friendGenome.AppendModuleNeuronsToMasterList(neuronList);
        movementGenome.AppendModuleNeuronsToMasterList(neuronList);
        threatGenome.AppendModuleNeuronsToMasterList(neuronList);        
    }*/

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
