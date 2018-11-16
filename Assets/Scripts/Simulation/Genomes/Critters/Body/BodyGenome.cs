using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BodyGenome {
    
    // BRAIN:
    public TestModuleGenome testModuleGenome;

    public CritterModuleAppearanceGenome appearanceGenome;    
    public CritterModuleCoreGenome coreGenome;
    public CritterModuleDevelopmentalGenome developmentalGenome;
    //public CritterModuleDigestiveGenome digestiveGenome;  // not needed?
    //public CritterModuleEnergyGenome energyGenome;  // not needed?
    //public CritterModuleExteriorGenome exteriorGenome; // not needed?
    public CritterModuleMovementGenome movementGenome;

    public Vector3 fullsizeBoundingBox;   // Z = forward, Y = up

    //public List<HealthGenome> healthModuleList;
    //public List<OscillatorGenome> oscillatorInputList;
    //public List<ValueInputGenome> valueInputList;

    public BodyGenome() {
        //Debug.Log("BodyGenome() EMPTY CONSTRUCTOR");
    }

    public void FirstTimeInitializeCritterModuleGenomes() {

        // ID and inno# needed???? ***** should only be required to keep track of evolving body functions
        appearanceGenome = new CritterModuleAppearanceGenome(0, 0);
        coreGenome = new CritterModuleCoreGenome(0, 0);
        developmentalGenome = new CritterModuleDevelopmentalGenome(0, 0);
        //digestiveGenome = new CritterModuleDigestiveGenome(0, 0);
        //energyGenome = new CritterModuleEnergyGenome(0, 0);
        //exteriorGenome = new CritterModuleExteriorGenome(0, 0);
        movementGenome = new CritterModuleMovementGenome(0, 0);
         
    }

    public void CalculateFullsizeBoundingBox() {
        fullsizeBoundingBox = GetFullsizeBoundingBox();        
    }
    public Vector3 GetFullsizeBoundingBox() {
        
        float fullLength = coreGenome.creatureBaseLength * (coreGenome.mouthLength + coreGenome.headLength + coreGenome.bodyLength + coreGenome.tailLength);
        float approxAvgRadius = fullLength / coreGenome.creatureBaseAspectRatio;

        Vector3 size = new Vector3(approxAvgRadius, fullLength, approxAvgRadius);
        return size;        
    }

    public void GenerateRandomBodyGenome() {
        appearanceGenome.GenerateRandomGenome();
        coreGenome.GenerateRandomInitialGenome();
        developmentalGenome.GenerateRandomGenome();
        //digestiveGenome.GenerateRandomGenome();
        //energyGenome.GenerateRandomGenome();
        //exteriorGenome.GenerateRandomGenome();
        movementGenome.GenerateRandomGenome();

        // Calculate BoundingBox:
        //CalculateFullsizeBoundingBox();
    }
    
    public void InitializeBrainGenome(List<NeuronGenome> neuronList) {

        // Go through each of the Body's Modules and add In/Out neurons based on module upgrades and settings:
        appearanceGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        coreGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        developmentalGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        //digestiveGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        //energyGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        //exteriorGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        movementGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        
    }

    public void SetToMutatedCopyOfParentGenome(BodyGenome parentBodyGenome, MutationSettings settings) {

        // *** OPTIMIZATION:  convert this to use pooling rather than using new memory alloc every mutation
        FirstTimeInitializeCritterModuleGenomes(); // Re-constructs all modules as new() 
        // *** Result needs to be fully independent copy and share no references!!!
        appearanceGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.appearanceGenome, settings);
        coreGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.coreGenome, settings);
        movementGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.movementGenome, settings);
      
    }
    
}
