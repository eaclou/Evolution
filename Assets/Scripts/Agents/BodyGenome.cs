using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BodyGenome {
    
    // BRAIN:
    public TestModuleGenome testModuleGenome;

    // BODY:
    public Vector2 sizeAndAspectRatio;
    public Vector3 huePrimary;
    public Vector3 hueSecondary;
    // Body Brushstroke information
    public int bodyStrokeBrushTypeX;  // derives rest of information from Agent size & hue?
    public int bodyStrokeBrushTypeY;
    // Body Decorations strokes information
    public List<DecorationGenome> decorationGenomeList;
    // Body Tentacles information
    public List<TentacleGenome> tentacleGenomeList;
    // Eyes information
    public EyeGenome eyeGenome;


    public struct DecorationGenome {
        public Vector2 localPos;
        public Vector2 localDir;
        public Vector2 localScale;
        public float colorLerp;
        public float strength;
        public int brushType;
    }

    public struct TentacleGenome {
        public Vector2 attachDir;
        public float length;
        public float startWidth;
        public float endWidth;
    }

    public struct EyeGenome {
        public Vector2 localPos;
        public Vector2 localScale;  // avgSize, aspectRatio
        public Vector3 irisHue;
        public Vector3 pupilHue;
        public int eyeBrushType;
        //public float irisRadius;
        //public float pupilRadius;
    }

    //public List<HealthGenome> healthModuleList;
    //public List<OscillatorGenome> oscillatorInputList;
    //public List<ValueInputGenome> valueInputList;

    public BodyGenome() {

    }

    public void InitializeAsRandomGenome() {
        testModuleGenome = new TestModuleGenome(0, 0);

        sizeAndAspectRatio = new Vector2(1f, 1f);

        huePrimary = new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        hueSecondary = new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        bodyStrokeBrushTypeX = UnityEngine.Random.Range(0, 8);
        bodyStrokeBrushTypeY = UnityEngine.Random.Range(0, 8);

        decorationGenomeList = new List<DecorationGenome>();
        for(int i = 0; i < 4; i++) {
            DecorationGenome decorationGenome = new DecorationGenome();
            decorationGenome.localPos = UnityEngine.Random.insideUnitCircle;
            decorationGenome.localDir = UnityEngine.Random.insideUnitCircle.normalized;
            decorationGenome.localScale = new Vector2(UnityEngine.Random.Range(0.2f, 0.4f), UnityEngine.Random.Range(0.2f, 0.4f));
            decorationGenome.colorLerp = UnityEngine.Random.Range(0f, 1f);
            decorationGenome.strength = UnityEngine.Random.Range(0f, 1f);
            decorationGenome.brushType = UnityEngine.Random.Range(0, 8);
        }
        tentacleGenomeList = new List<TentacleGenome>();
        for(int i = 0; i < 0; i++) {
            // LATER!!!
        }
        eyeGenome = new EyeGenome();
        eyeGenome.localPos = new Vector2(0.5f, 0.8f);
        eyeGenome.localScale = new Vector2(0.32f, 1f);
        eyeGenome.irisHue = new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        eyeGenome.pupilHue = Vector3.zero;
        eyeGenome.eyeBrushType = UnityEngine.Random.Range(0, 8);
        //eyeGenome.irisRadius = 0.66f;
        //eyeGenome.pupilRadius = 0.33f;
    }

    //public void InitializeGenomeAsDefault() {
    //    testModuleGenome = new TestModuleGenome(0, 0);
    //}

    public void InitializeBrainGenome(List<NeuronGenome> neuronList) {

        testModuleGenome.InitializeBrainGenome(neuronList);

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
        // *** Result needs to be fully independent copy and share no references!!!
        testModuleGenome = new TestModuleGenome(0, 0); // for now.. **** Revisit if updating Modules/Abilites!!!
        
        // Set equal to parent at first, then check for possible mutation of that value:
        // SIZE IS: (x= size, y= aspectRatio)   aspect = x/y
        sizeAndAspectRatio = UtilityMutationFunctions.GetMutatedVector2Additive(parentBodyGenome.sizeAndAspectRatio, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, new Vector2(0.8f, 0.6f), new Vector2(2.4f, 1f));
        huePrimary = UtilityMutationFunctions.GetMutatedVector3Additive(parentBodyGenome.huePrimary, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        hueSecondary = UtilityMutationFunctions.GetMutatedVector3Additive(parentBodyGenome.hueSecondary, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        // ***** v v v Revisit when implementing #BrushTypes!! **** REVISIT!!
        bodyStrokeBrushTypeX = UtilityMutationFunctions.GetMutatedIntAdditive(parentBodyGenome.bodyStrokeBrushTypeX, settings.defaultBodyMutationChance, 2, 0, 7); // *****
        bodyStrokeBrushTypeY = UtilityMutationFunctions.GetMutatedIntAdditive(parentBodyGenome.bodyStrokeBrushTypeY, settings.defaultBodyMutationChance, 2, 0, 7);

        decorationGenomeList = new List<DecorationGenome>();
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
        }

        eyeGenome = new EyeGenome();

        eyeGenome.localPos = UtilityMutationFunctions.GetMutatedVector2Additive(parentBodyGenome.eyeGenome.localPos, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, new Vector2(0.2f, 0f), new Vector2(1f, 1f));
        // EYES SCALE IS: (x= size, y= aspectRatio)
        eyeGenome.localScale = UtilityMutationFunctions.GetMutatedVector2Additive(parentBodyGenome.eyeGenome.localScale, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, new Vector2(0.25f, 0.85f), new Vector2(0.4f, 1.2f));
        eyeGenome.irisHue = UtilityMutationFunctions.GetMutatedVector3Additive(parentBodyGenome.eyeGenome.irisHue, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        eyeGenome.pupilHue = UtilityMutationFunctions.GetMutatedVector3Additive(parentBodyGenome.eyeGenome.pupilHue, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        eyeGenome.eyeBrushType = UtilityMutationFunctions.GetMutatedIntAdditive(parentBodyGenome.eyeGenome.eyeBrushType, settings.defaultBodyMutationChance, 7, 0, 7);
        //eyeGenome.pupilRadius = UtilityMutationFunctions.GetMutatedFloatAdditive(parentBodyGenome.eyeGenome.pupilRadius, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.25f, 0.95f);
        
    }


    /*public void CopyBodyGenomeFromTemplate(BodyGenome templateGenome) { // This method creates a clone of the provided BodyGenome - should have no shared references!!!
        testModuleGenome = new TestModuleGenome(templateGenome.testModuleGenome);

        
    }*/
}
