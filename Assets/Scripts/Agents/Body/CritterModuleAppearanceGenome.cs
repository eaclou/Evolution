using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritterModuleAppearanceGenome {

    public int parentID;
    public int inno;
	
    // BODY:
    public Vector2 sizeAndAspectRatio;


    public Vector3 huePrimary;
    public Vector3 hueSecondary;
    public int bodyStrokeBrushTypeX;  // derives rest of information from Agent size & hue?
    public int bodyStrokeBrushTypeY;
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
    [System.Serializable]
    public struct EyeGenome {
        public Vector2 localPos;
        public Vector2 localScale;  // avgSize, aspectRatio
        public Vector3 irisHue;
        public Vector3 pupilHue;
        public int eyeBrushType;
    }

    public CritterModuleAppearanceGenome(int parentID, int inno) {
        //Debug.Log("CritterModuleAppearanceGenome() Constructor!!");

        this.parentID = parentID;
        this.inno = inno;

    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {

    }

    public void GenerateRandomGenome() {
        // Do stuff:

        sizeAndAspectRatio = new Vector2(1f, 1f);

        huePrimary = new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        hueSecondary = new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        bodyStrokeBrushTypeX = UnityEngine.Random.Range(0, 8);
        bodyStrokeBrushTypeY = UnityEngine.Random.Range(0, 8);
        
        eyeGenome = new EyeGenome();
        eyeGenome.localPos = new Vector2(UnityEngine.Random.Range(0.45f, 1f), UnityEngine.Random.Range(0f, 1f));
        eyeGenome.localScale = new Vector2(1f, 1f);
        eyeGenome.irisHue = new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        eyeGenome.pupilHue = Vector3.zero;
        eyeGenome.eyeBrushType = UnityEngine.Random.Range(0, 8);        
    }

    public void SetToMutatedCopyOfParentGenome(CritterModuleAppearanceGenome parentGenome, MutationSettings settings) {

        //testModuleGenome = new TestModuleGenome(0, 0); // for now.. **** Revisit if updating Modules/Abilites!!!
        
        // Set equal to parent at first, then check for possible mutation of that value:
        // SIZE IS: (x= size, y= aspectRatio)   aspect = x/y
        sizeAndAspectRatio = UtilityMutationFunctions.GetMutatedVector2Additive(parentGenome.sizeAndAspectRatio, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, new Vector2(0.8f, 0.6f), new Vector2(2.4f, 1f));



        huePrimary = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.huePrimary, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        hueSecondary = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.hueSecondary, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        // ***** v v v Revisit when implementing #BrushTypes!! **** REVISIT!!
        bodyStrokeBrushTypeX = UtilityMutationFunctions.GetMutatedIntAdditive(parentGenome.bodyStrokeBrushTypeX, settings.defaultBodyMutationChance, 2, 0, 7); // *****
        bodyStrokeBrushTypeY = UtilityMutationFunctions.GetMutatedIntAdditive(parentGenome.bodyStrokeBrushTypeY, settings.defaultBodyMutationChance, 2, 0, 7);

        eyeGenome = new EyeGenome();

        eyeGenome.localPos = UtilityMutationFunctions.GetMutatedVector2Additive(parentGenome.eyeGenome.localPos, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, new Vector2(0.45f, 0f), new Vector2(1f, 1f));
        // EYES SCALE IS: (x= size, y= aspectRatio)
        eyeGenome.localScale = UtilityMutationFunctions.GetMutatedVector2Additive(parentGenome.eyeGenome.localScale, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, new Vector2(1f, 1f), new Vector2(1f, 1f));
        eyeGenome.irisHue = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.eyeGenome.irisHue, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        eyeGenome.pupilHue = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.eyeGenome.pupilHue, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        eyeGenome.eyeBrushType = UtilityMutationFunctions.GetMutatedIntAdditive(parentGenome.eyeGenome.eyeBrushType, settings.defaultBodyMutationChance, 7, 0, 7);
        //eyeGenome.pupilRadius = UtilityMutationFunctions.GetMutatedFloatAdditive(parentBodyGenome.eyeGenome.pupilRadius, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.25f, 0.95f);
        
    }
}
