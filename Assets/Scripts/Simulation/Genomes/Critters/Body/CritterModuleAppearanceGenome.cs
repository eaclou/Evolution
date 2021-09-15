using System.Collections.Generic;
using UnityEngine;

///  CHANGE ALL OF THIS!!!!!!! ****
[System.Serializable]
public class CritterModuleAppearanceGenome {

    public int parentID;
    public int inno;
	
    // BODY:
    //public Vector2 sizeAndAspectRatio;

    public Vector3 huePrimary;
    public Vector3 hueSecondary;

    // List of Pattern Modifiers?

    public int bodyStrokeBrushTypeX;  // derives rest of information from Agent size & hue?
    public int bodyStrokeBrushTypeY;
    public EyeGenome eyeGenome;
    
    [System.Serializable]
    public struct EyeGenome {
        public Vector2 localPos;
        public Vector2 localScale;  // avgSize, aspectRatio
        public Vector3 irisHue;
        public Vector3 pupilHue;
        public int eyeBrushType;
    }

    public CritterModuleAppearanceGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {

    }

    public void GenerateRandomInitialGenome() 
    {
        //sizeAndAspectRatio = new Vector2(1f, 1f);

        huePrimary = new Vector3(Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), Random.Range(0.1f, 1f));
        hueSecondary = new Vector3(Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), Random.Range(0.1f, 1f));
        bodyStrokeBrushTypeX = Random.Range(0, 8);
        bodyStrokeBrushTypeY = Random.Range(0, 4);
        
        eyeGenome = new EyeGenome();
        eyeGenome.localPos = new Vector2(Random.Range(0.45f, 1f), Random.Range(0.4f, 1f));
        eyeGenome.localScale = new Vector2(1f, 1f);
        eyeGenome.irisHue = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        eyeGenome.pupilHue = Vector3.zero;
        eyeGenome.eyeBrushType = Random.Range(0, 8);        
    }

    public void SetToMutatedCopyOfParentGenome(CritterModuleAppearanceGenome parentGenome, MutationSettings settings) 
    {
        //float mutationChanceMultiplier = 1f; // ******* settings.mutationStrengthSlot;
        huePrimary = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.huePrimary, settings.bodyColorsMutationChance, settings.bodyColorsMutationStepSize, 0f, 1f);
        hueSecondary = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.hueSecondary, settings.bodyColorsMutationChance, settings.bodyColorsMutationStepSize, 0f, 1f);
        // ***** v v v Revisit when implementing #BrushTypes!! **** REVISIT!!
        bodyStrokeBrushTypeX = UtilityMutationFunctions.GetMutatedIntAdditive(parentGenome.bodyStrokeBrushTypeX, settings.bodyCoreSizeMutationChance, 2, 0, 7); // *****
        bodyStrokeBrushTypeY = UtilityMutationFunctions.GetMutatedIntAdditive(parentGenome.bodyStrokeBrushTypeY, settings.bodyCoreSizeMutationChance, 2, 0, 3);

        eyeGenome = new EyeGenome();

        eyeGenome.localPos = UtilityMutationFunctions.GetMutatedVector2Additive(parentGenome.eyeGenome.localPos, settings.bodyEyeProportionsMutationChance, settings.bodyEyeProportionsMutationStepSize, new Vector2(0.45f, 0.4f), new Vector2(1f, 1f));
        // EYES SCALE IS: (x= size, y= aspectRatio)
        eyeGenome.localScale = UtilityMutationFunctions.GetMutatedVector2Additive(parentGenome.eyeGenome.localScale, settings.bodyEyeProportionsMutationChance, settings.bodyEyeProportionsMutationStepSize, new Vector2(1f, 1f), new Vector2(1f, 1f));
        eyeGenome.irisHue = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.eyeGenome.irisHue, settings.bodyEyeProportionsMutationChance, settings.bodyEyeProportionsMutationStepSize, 0f, 1f);
        eyeGenome.pupilHue = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.eyeGenome.pupilHue, settings.bodyEyeProportionsMutationChance, settings.bodyEyeProportionsMutationStepSize, 0f, 1f);
        eyeGenome.eyeBrushType = UtilityMutationFunctions.GetMutatedIntAdditive(parentGenome.eyeGenome.eyeBrushType, settings.bodyEyeProportionsMutationChance, 7, 0, 7);
        //eyeGenome.pupilRadius = UtilityMutationFunctions.GetMutatedFloatAdditive(parentBodyGenome.eyeGenome.pupilRadius, settings.bodyEyeProportionsMutationChance, settings.bodyEyeProportionsMutationStepSize, 0.25f, 0.95f);
    }
}
