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
    public CritterModuleDigestiveGenome digestiveGenome;
    public CritterModuleEnergyGenome energyGenome;
    public CritterModuleExteriorGenome exteriorGenome;
    public CritterModuleMovementGenome movementGenome;

    //public List<HealthGenome> healthModuleList;
    //public List<OscillatorGenome> oscillatorInputList;
    //public List<ValueInputGenome> valueInputList;

    public BodyGenome() {
        Debug.Log("BodyGenome() EMPTY CONSTRUCTOR");
    }

    public void FirstTimeInitializeCritterModuleGenomes() {

        // ID and inno# needed???? ***** should only be required to keep track of evolving body functions
        appearanceGenome = new CritterModuleAppearanceGenome(0, 0);
        coreGenome = new CritterModuleCoreGenome(0, 0);
        developmentalGenome = new CritterModuleDevelopmentalGenome(0, 0);
        digestiveGenome = new CritterModuleDigestiveGenome(0, 0);
        energyGenome = new CritterModuleEnergyGenome(0, 0);
        exteriorGenome = new CritterModuleExteriorGenome(0, 0);
        movementGenome = new CritterModuleMovementGenome(0, 0);
        
        //OLD:
        //testModuleGenome = new TestModuleGenome(0, 0);
                   
    }

    public void GenerateRandomBodyGenome() {
        appearanceGenome.GenerateRandomGenome();
        coreGenome.GenerateRandomGenome();
        developmentalGenome.GenerateRandomGenome();
        digestiveGenome.GenerateRandomGenome();
        energyGenome.GenerateRandomGenome();
        exteriorGenome.GenerateRandomGenome();
        movementGenome.GenerateRandomGenome();
    }
    
    public void InitializeBrainGenome(List<NeuronGenome> neuronList) {

        // Go through each of the Body's Modules and add In/Out neurons based on module upgrades and settings:
        appearanceGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        coreGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        developmentalGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        digestiveGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        energyGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        exteriorGenome.AppendModuleNeuronsToMasterList(ref neuronList);
        movementGenome.AppendModuleNeuronsToMasterList(ref neuronList);



        //testModuleGenome.InitializeBrainGenome(neuronList);

        // Centralized neuron lists instead of based on modules:
        // Manually set inno/neuron ID's in function args:
        /*
        NeuronGenome constant = new NeuronGenome(NeuronGenome.NeuronType.In, 0, 0);
        NeuronGenome posX = new NeuronGenome(NeuronGenome.NeuronType.In, 1, 0);
        NeuronGenome posY = new NeuronGenome(NeuronGenome.NeuronType.In, 1, 1);
        NeuronGenome moveX = new NeuronGenome(NeuronGenome.NeuronType.Out, 2, 0);
        NeuronGenome moveY = new NeuronGenome(NeuronGenome.NeuronType.Out, 2, 1);
        neuronList.Add(constant);
        neuronList.Add(posX);
        neuronList.Add(posY);
        neuronList.Add(moveX);
        neuronList.Add(moveY);
        */
    }

    public void SetToMutatedCopyOfParentGenome(BodyGenome parentBodyGenome, MutationSettings settings) {

        // *** OPTIMIZATION:  convert this to use pooling rather than using new memory alloc every mutation
        FirstTimeInitializeCritterModuleGenomes(); // Re-constructs all modules as new() 
        // *** Result needs to be fully independent copy and share no references!!!
        appearanceGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.appearanceGenome, settings);
        coreGenome.SetToMutatedCopyOfParentGenome(parentBodyGenome.coreGenome, settings);

        /*
        //testModuleGenome = new TestModuleGenome(0, 0); // for now.. **** Revisit if updating Modules/Abilites!!!
        
        // Set equal to parent at first, then check for possible mutation of that value:
        // SIZE IS: (x= size, y= aspectRatio)   aspect = x/y
        sizeAndAspectRatio = UtilityMutationFunctions.GetMutatedVector2Additive(parentBodyGenome.sizeAndAspectRatio, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, new Vector2(0.8f, 0.6f), new Vector2(2.4f, 1f));
        huePrimary = UtilityMutationFunctions.GetMutatedVector3Additive(parentBodyGenome.huePrimary, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        hueSecondary = UtilityMutationFunctions.GetMutatedVector3Additive(parentBodyGenome.hueSecondary, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        // ***** v v v Revisit when implementing #BrushTypes!! **** REVISIT!!
        bodyStrokeBrushTypeX = UtilityMutationFunctions.GetMutatedIntAdditive(parentBodyGenome.bodyStrokeBrushTypeX, settings.defaultBodyMutationChance, 2, 0, 7); // *****
        bodyStrokeBrushTypeY = UtilityMutationFunctions.GetMutatedIntAdditive(parentBodyGenome.bodyStrokeBrushTypeY, settings.defaultBodyMutationChance, 2, 0, 7);

        eyeGenome = new EyeGenome();

        eyeGenome.localPos = UtilityMutationFunctions.GetMutatedVector2Additive(parentBodyGenome.eyeGenome.localPos, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, new Vector2(0.2f, 0f), new Vector2(1f, 1f));
        // EYES SCALE IS: (x= size, y= aspectRatio)
        eyeGenome.localScale = UtilityMutationFunctions.GetMutatedVector2Additive(parentBodyGenome.eyeGenome.localScale, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, new Vector2(0.33f, 0.85f), new Vector2(0.5f, 1.2f));
        eyeGenome.irisHue = UtilityMutationFunctions.GetMutatedVector3Additive(parentBodyGenome.eyeGenome.irisHue, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        eyeGenome.pupilHue = UtilityMutationFunctions.GetMutatedVector3Additive(parentBodyGenome.eyeGenome.pupilHue, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        eyeGenome.eyeBrushType = UtilityMutationFunctions.GetMutatedIntAdditive(parentBodyGenome.eyeGenome.eyeBrushType, settings.defaultBodyMutationChance, 7, 0, 7);
        //eyeGenome.pupilRadius = UtilityMutationFunctions.GetMutatedFloatAdditive(parentBodyGenome.eyeGenome.pupilRadius, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.25f, 0.95f);
        */

        /*decorationGenomeList = new List<DecorationGenome>();
        for(int i = 0; i < parentBodyGenome.decorationGenomeList.Count; i++) {
            DecorationGenome newGenome = new DecorationGenome();
            //parentBodyGenome.decorationGenomeList[i]; // struct is value type so *hopefully* shouldn't be a problem w/ shared references....
            newGenome.localPos = UtilityMutationFunctions.GetMutatedVector2Additive(parentBodyGenome.decorationGenomeList[i].localPos, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f);
            newGenome.localDir = UtilityMutationFunctions.GetMutatedVector2Additive(parentBodyGenome.decorationGenomeList[i].localDir, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f).normalized;
            newGenome.localScale = UtilityMutationFunctions.GetMutatedVector2Additive(parentBodyGenome.decorationGenomeList[i].localScale, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.2f, 0.4f);
            newGenome.colorLerp = UtilityMutationFunctions.GetMutatedFloatAdditive(parentBodyGenome.decorationGenomeList[i].colorLerp, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
            newGenome.strength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentBodyGenome.decorationGenomeList[i].strength, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
            // ***** v v v Revisit when implementing #BrushTypes!! **** REVISIT!!
            //bodyStrokeBrushTypeX = UtilityMutationFunctions.GetMutatedIntAdditive(parentBodyGenome.decorationGenomeList[i].brushType, settings.defaultBodyMutationChance, 4, 0, 0); // *****

            decorationGenomeList.Add(newGenome);
        }

        tentacleGenomeList = new List<TentacleGenome>();
        for(int i = 0; i < parentBodyGenome.tentacleGenomeList.Count; i++) {
            TentacleGenome newGenome = new TentacleGenome();

            newGenome.attachDir = UtilityMutationFunctions.GetMutatedVector2Additive(parentBodyGenome.tentacleGenomeList[i].attachDir, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f).normalized;
            newGenome.length = UtilityMutationFunctions.GetMutatedFloatAdditive(parentBodyGenome.tentacleGenomeList[i].length, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 1f, 10f);
            newGenome.startWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentBodyGenome.tentacleGenomeList[i].startWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.1f, 0.5f);
            newGenome.endWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentBodyGenome.tentacleGenomeList[i].endWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.1f, 0.5f);
            
            tentacleGenomeList.Add(newGenome);
        }*/        
    }


    /*public void CopyBodyGenomeFromTemplate(BodyGenome templateGenome) { // This method creates a clone of the provided BodyGenome - should have no shared references!!!
        testModuleGenome = new TestModuleGenome(templateGenome.testModuleGenome);

        
    }*/
}
