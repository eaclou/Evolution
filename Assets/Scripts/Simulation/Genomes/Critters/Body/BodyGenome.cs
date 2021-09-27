using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BodyGenome 
{
    // BRAIN:
    public TestModuleGenome testModuleGenome;

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
    
    public BodyGenome() {
        //Debug.Log("BodyGenome() EMPTY CONSTRUCTOR");
    }
    
    public static float GetBodySizeScore01(BodyGenome genome) {
        // Refactor: 25f is hardcoded approximate!
        float normalizedSizeScore = Mathf.Clamp01(((genome.GetFullsizeBoundingBox().x + genome.GetFullsizeBoundingBox().z) / genome.GetFullsizeBoundingBox().y) / 25f); 
        return normalizedSizeScore;
    }
    
    public void FirstTimeInitializeCritterModuleGenomes() {
        // ID and inno# needed???? ***** should only be required to keep track of evolving body functions
        appearanceGenome = new CritterModuleAppearanceGenome(0, 0);
        communicationGenome = new CritterModuleCommunicationGenome(0, 1);
        coreGenome = new CritterModuleCoreGenome(0, 2);
        developmentalGenome = new CritterModuleDevelopmentalGenome(0, 3);
        environmentalGenome = new CritterModuleEnvironmentSensorsGenome(0, 4);
        foodGenome = new CritterModuleFoodSensorsGenome(0, 5);
        friendGenome = new CritterModuleFriendSensorsGenome(0, 6);
        movementGenome = new CritterModuleMovementGenome(0, 7);
        threatGenome = new CritterModuleThreatSensorsGenome(0, 8);
    }
        
    public Vector3 GetFullsizeBoundingBox() {
        float fullLength = coreGenome.creatureBaseLength * (coreGenome.mouthLength + coreGenome.headLength + coreGenome.bodyLength + coreGenome.tailLength);
        float approxAvgRadius = fullLength * coreGenome.creatureAspectRatio;

        Vector3 size = new Vector3(approxAvgRadius, fullLength, approxAvgRadius);
        return size;        
    }

    public void GenerateInitialRandomBodyGenome() {
        appearanceGenome.GenerateRandomInitialGenome();
        communicationGenome.GenerateRandomInitialGenome();
        coreGenome.GenerateRandomInitialGenome();
        developmentalGenome.GenerateRandomInitialGenome();
        environmentalGenome.GenerateRandomInitialGenome();
        foodGenome.GenerateRandomInitialGenome();
        friendGenome.GenerateRandomInitialGenome();
        movementGenome.GenerateRandomInitialGenome();
        threatGenome.GenerateRandomInitialGenome();
    }
    
    public void InitializeBrainGenome(List<NeuronGenome> neuronList) {
        // Go through each of the Body's Modules and add In/Out neurons based on module upgrades and settings:
        appearanceGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        communicationGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        coreGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        developmentalGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        environmentalGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        foodGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        friendGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        movementGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        threatGenome.AppendModuleNeuronsToMasterList(ref neuronList);        
    }

    // Mutable by Player
    public void SetToMutatedCopyOfParentGenome(BodyGenome parentBodyGenome, MutationSettingsInstance settings) {   
        // *** Result needs to be fully independent copy and share no references!!!
        // *** OPTIMIZATION:  convert this to use pooling rather than using new memory alloc every mutation
        FirstTimeInitializeCritterModuleGenomes(); // Re-constructs all modules as new() 

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
